using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TextCore.Text;


/// <summary>
/// Component that gives an object functionality to become an object that can damage another object that has <see cref="HealthComponent"/>. This class is handled by <see cref="WeaponHandler"/> or can be entirely independent (as an example <see cref="TrapComponent"/>).
/// 
/// This class uses external component(s);
/// - <b>SpriteRenderer</b> for displaying sprite for the damager/projectile (optional if displayed in another object).
/// - <see cref="SoundAlertTransceiver"/> for alerting any object using "sound" range.
/// - <b>AudioSource</b> for playing a sound when the object is interacted.
/// </summary>
public class DamagerComponent: MonoBehaviour{
  [Serializable]
  /// <summary>
  /// Data about the damager.
  /// </summary>
  public struct DamagerData{
    /// <summary>
    /// Value on how many points to deduct when damaging a <see cref="HealthComponent"/> object.
    /// </summary>
    public uint damage_points;
  }

  [Serializable] [Inspectable]
  /// <summary>
  /// Damager's context or configuration data.
  /// </summary>
  public struct DamagerContext{
    /// <summary>
    /// What the component should do when on collision with another object.
    /// </summary>
    public enum OnCollisionAction{
      Nothing,
      EraseOnCollision
    }

    /// <summary>
    /// What kind of type the damager would inherit that determines the behaviour of the component.
    /// </summary>
    public enum ProjectileType{
      Raycast,
      NormalProjectile
    }

    [Serializable]
    /// <summary>
    /// Data for telling the damager to use items when the component are triggered/fired.
    /// </summary>
    public struct ItemDeplete{
      /// <summary>
      /// Target item by ID.
      /// </summary>
      public string ItemID;

      /// <summary>
      /// How many items that the component should deplete.
      /// </summary>
      public uint count;
    }


    [Inspectable]
    /// <summary>
    /// Type of the <see cref="DamagerComponent"/>.
    /// </summary>
    public ProjectileType _ProjectileType;

    public OnCollisionAction OnCollisionEffect; 

    [Inspectable]
    /// <summary>
    /// Should the damage be dynamically changed based on the speed of the projectile.
    /// </summary>
    public bool DamageBasedOnSpeed;

    /// <summary>
    /// The gravity scale the projectile should use. This modifies <b>Rigidbody</b> properties.
    /// </summary>
    public float ProjectileGravityScale;

    /// <summary>
    /// The material configuration for physics.
    /// </summary>
    public PhysicsMaterial2D ProjectileMaterial;

    /// <summary>
    /// The size the projectile should use. This modifies <b>Transform</b> properties.
    /// </summary>
    public float ProjectileSize;

    /// <summary>
    /// The initial speed of the projectile.
    /// </summary>
    public float ProjectileSpeed;

    /// <summary>
    /// The lifetime of the projectile.
    /// </summary>
    public float DamagerLifetime;

    /// <summary>
    /// NOTE: Unused for now.
    /// Can the projectile becomes a collectible when the projectile is below the speed threshold stored in <see cref="AllowCollectibleOnSpeedThreshold"/>.
    /// </summary>
    public bool AsCollectible;

    /// <summary>
    /// NOTE: Unused for now.
    /// The speed threshold used for letting the projectile to become a collectible in below this threshold.
    /// </summary>
    public float AllowCollectibleOnSpeedThreshold;

    /// <summary>
    /// Mask to exclude certain physics layer when the projectile is below the pssed threshold stored in <see cref="SetExcludeLayerOnSpeedThreshold"/>.
    /// </summary>
    public LayerMask ExcludeLayerHide;

    /// <summary>
    /// The speed threshold used for letting the projectile use <see cref="ExcludeLayerHide"/> mask to exclude certain physics layer.
    /// </summary>
    public float SetExcludeLayerOnSpeedThreshold;

    /// <summary>
    /// List of items the damager should deplete to become functional.
    /// </summary>
    public List<ItemDeplete> ListDepleteItem;

    /// <summary>
    /// Should the damager use <see cref="SoundAlertTransceiver"/> when the projectile is colliding with another object.
    /// </summary>
    public bool SoundAlertOnCollision;

    /// <summary>
    /// Configuration for using "alerting" feature used in <see cref="SoundAlertTransceiver"/>.
    /// </summary>
    public SoundAlertTransceiver.AlertConfig SoundAlertConfig;

    /// <summary>
    /// The audio data used for when the projectile is colliding with another object.
    /// </summary>
    public AudioClip OnCollisionSound;
  }

  /// <summary>
  /// Data used as a parameter in <see cref="TriggerDamager"/> or interface function <b>DamagerComponent_TriggerDamager</b>.
  /// </summary>
  public struct DamagerTriggerData{
    public Vector2 Direction;
    public Vector3 StartPosition;
  }


  [SerializeField]
  private LayerMask _DamagerExcludeLayer;

  [SerializeField]
  private SpriteRenderer _ProjectileSprite;

  [SerializeField]
  private SoundAlertTransceiver _SoundAlertTranceiver;

  [SerializeField]
  private AudioSource _TargetAudioSource;
  

  // Using default data at start.
  private DamagerData _damager_data = new DamagerData{
    damage_points = 1
  };

  // Using default data at start.
  private DamagerContext _damager_context = new DamagerContext{
    _ProjectileType = DamagerContext.ProjectileType.NormalProjectile,
    ProjectileGravityScale = 0,
    OnCollisionEffect = DamagerContext.OnCollisionAction.Nothing,
    DamageBasedOnSpeed = false,
    DamagerLifetime = float.PositiveInfinity
  };


  private Rigidbody2D _projectile_rigidbody = null;

  private List<Collider2D> _collision_lists = new();
  private HashSet<int> _list_collided = new();

  private LayerMask _current_exclude;


  private Vector3 _direction;
  private Vector3 _last_pos;

  private float _object_lifetime = -1;

  private bool _is_triggered = false;

  public bool AllowMultipleHitsSameObject = false;



  // Check if object has HealthComponent, then damage it.
  private void _check_object(GameObject target_object){
    HealthComponent _health = target_object.GetComponent<HealthComponent>();
    if(_health == null)
      return;

    if(!AllowMultipleHitsSameObject && _list_collided.Contains(target_object.GetInstanceID()))
      return;

    _health.DoDamage(_damager_data, this);
    _list_collided.Add(target_object.GetInstanceID());
  }

  private void _object_collided(GameObject target_object){
    if(!enabled)
      return;

    _check_object(target_object);
    
    if(_SoundAlertTranceiver != null && _damager_context.SoundAlertOnCollision)
      _SoundAlertTranceiver.TriggerSound();

    if(_TargetAudioSource != null && _damager_context.OnCollisionSound != null)
      _TargetAudioSource.PlayOneShot(_damager_context.OnCollisionSound);

    switch(_damager_context.OnCollisionEffect){
      case DamagerContext.OnCollisionAction.EraseOnCollision:{
        Destroy(gameObject);
      }break;
    }
  }


  public void Start(){
    _projectile_rigidbody = GetComponent<Rigidbody2D>();

    Collider2D[] collisions = GetComponents<Collider2D>();
    _collision_lists = collisions.ToList();

    SetDamagerExcludeLayer(new LayerMask());
    _last_pos = transform.position;
  }

  public void FixedUpdate(){
    // This function handles the threshold and also the lifetime of the projectile.
    if(!_is_triggered)
      return;

    float _speed = (transform.position-_last_pos).magnitude/Time.fixedDeltaTime;
    if(_projectile_rigidbody != null){
      _projectile_rigidbody.excludeLayers =
        (_speed < _damager_context.SetExcludeLayerOnSpeedThreshold)?
        _damager_context.ExcludeLayerHide:
        _current_exclude
      ;
    }

    _last_pos = transform.position;

    if(float.IsFinite(_object_lifetime) && _object_lifetime > 0){
      _object_lifetime -= Time.fixedDeltaTime;
      if(_object_lifetime <= 0)
        Destroy(gameObject);
    }
  }


  /// <summary>
  /// Set and replace <see cref="DamagerData"/> with new one.
  /// </summary>
  /// <param name="damage_data">The new damager data</param>
  public void SetDamagerData(DamagerData damage_data){
    _damager_data = damage_data;

    gameObject.SendMessage("DamagerComponent_OnDataChanged", SendMessageOptions.DontRequireReceiver);
  }

  /// <summary>
  /// Set and apply the configuration of this component and another component related to this. 
  /// </summary>
  /// <param name="context">The new configuration data</param>
  public void SetDamagerContext(DamagerContext context){
    _damager_context = context;

    transform.localScale = Vector3.one * _damager_context.ProjectileSize;
    
    if(_projectile_rigidbody != null){
      _projectile_rigidbody.sharedMaterial = _damager_context.ProjectileMaterial != null? _damager_context.ProjectileMaterial: new();
    }

    context.SoundAlertConfig.SoundRangeMax *= 1/context.ProjectileSize;
    if(_SoundAlertTranceiver != null){
      _SoundAlertTranceiver.SetAlertConfig(context.SoundAlertConfig);
    }

    gameObject.SendMessage("DamagerComponent_OnContextChanged", SendMessageOptions.DontRequireReceiver);
  }


  /// <summary>
  /// Get current data of <see cref="DamagerData"/>.
  /// </summary>
  /// <returns>Current <see cref="DamagerData"/></returns>
  public DamagerData GetDamagerData(){
    return _damager_data;
  }

  /// <summary>
  /// Get current configuration of this component.
  /// </summary>
  /// <returns>Current <see cref="DamagerComponent"/></returns>
  public DamagerContext GetDamagerContext(){
    return _damager_context;
  }
  

  /// <summary>
  /// Set which layer to be excluded from the physics processing. This exclusion method will not interfere or instead adapting with any feature that modifies the layer mask.
  /// </summary>
  /// <param name="excluded_layer"></param>
  public void SetDamagerExcludeLayer(LayerMask excluded_layer){
    LayerMask _result_layer = excluded_layer | _DamagerExcludeLayer;
    DEBUGModeUtils.Log(string.Format("result exclude layer {0}", _result_layer));

    if(_projectile_rigidbody != null)
      _projectile_rigidbody.excludeLayers = _result_layer;

    foreach(Collider2D _col in _collision_lists)
      _col.excludeLayers = _result_layer;

    _current_exclude = _result_layer;
  }

  /// <summary>
  /// Set the sprite of this damager object.
  /// </summary>
  /// <param name="texture">The source sprite</param>
  public void SetDamagerSprite(Sprite texture){
    if(_ProjectileSprite == null)
      return;
      
    _ProjectileSprite.sprite = texture;
    _ProjectileSprite.size = Vector2.one;
  }


  /// <summary>
  /// To initialize this component with using current data for use as a damager and giving force to this object based on the trigger data.
  /// </summary>
  /// <param name="data">The trigger data</param>
  public void TriggerDamager(DamagerTriggerData data){
    _is_triggered = true;
    _direction = data.Direction;

    _object_lifetime = _damager_context.DamagerLifetime <= 0? float.PositiveInfinity: _damager_context.DamagerLifetime;

    transform.position = data.StartPosition;
    switch(_damager_context._ProjectileType){
      case DamagerContext.ProjectileType.NormalProjectile:{
        if(_projectile_rigidbody != null){
          _projectile_rigidbody.velocity = _direction * _damager_context.ProjectileSpeed;
          _projectile_rigidbody.gravityScale = _damager_context.ProjectileGravityScale;
        }
      }break;

      case DamagerContext.ProjectileType.Raycast:{
        RaycastHit _raycast_result;
        Physics.Raycast(
          gameObject.transform.position,
          _direction,
          out _raycast_result,
          _damager_context.ProjectileSpeed,
          gameObject.layer
        );

        _check_object(_raycast_result.collider.gameObject);
        Destroy(gameObject);
      }break;
    }

    gameObject.SendMessage("DamagerComponent_TriggerDamager", data, SendMessageOptions.DontRequireReceiver);
  }


  /// <summary>
  /// Function to catch Unity's <b>Collider2D</b> event when an object entered. 
  /// </summary>
  /// <param name="collision">The entered object</param>
  public void OnCollisionEnter2D(Collision2D collision){
    _object_collided(collision.gameObject);
  }

  /// <summary>
  /// Function to catch Unity's <b>Collider2D</b> event when an object exited.
  /// </summary>
  /// <param name="collider">The exited object</param>
  public void OnTriggerEnter2D(Collider2D collider){
    _object_collided(collider.gameObject);
  }
}