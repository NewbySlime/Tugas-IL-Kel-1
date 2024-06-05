using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.TextCore.Text;


public class DamagerComponent: MonoBehaviour{
  [Serializable]
  public struct DamagerData{
    public uint damage_points;
  }

  [Serializable] [Inspectable]
  public struct DamagerContext{
    public enum OnCollisionAction{
      Nothing,
      EraseOnCollision
    }

    public enum ProjectileType{
      Raycast,
      NormalProjectile
    }

    [Serializable]
    public struct ItemDeplete{
      public string ItemID;
      public uint count;
    }


    [Inspectable]
    public ProjectileType _ProjectileType;

    public OnCollisionAction OnCollisionEffect; 
    [Inspectable]
    public bool DamageBasedOnSpeed;

    public float ProjectileGravityScale;
    public PhysicsMaterial2D ProjectileMaterial;
    public float ProjectileSize;
    public float ProjectileSpeed;

    public float DamagerLifetime;

    public bool AsCollectible;
    public float AllowCollectibleOnSpeedThreshold;

    public LayerMask ExcludeLayerHide;
    public float SetExcludeLayerOnSpeedThreshold;

    public List<ItemDeplete> ListDepleteItem;
  }

  public struct DamagerTriggerData{
    public Vector2 Direction;
    public Vector3 StartPosition;
  }


  [SerializeField]
  private LayerMask _DamagerExcludeLayer;

  [SerializeField]
  private SpriteRenderer _ProjectileSprite;
  

  private DamagerData _damager_data = new DamagerData{
    damage_points = 1
  };

  private DamagerContext _damager_context = new DamagerContext{
    _ProjectileType = DamagerContext.ProjectileType.NormalProjectile,
    ProjectileGravityScale = 0,
    OnCollisionEffect = DamagerContext.OnCollisionAction.Nothing,
    DamageBasedOnSpeed = false,
    DamagerLifetime = float.PositiveInfinity
  };


  private Rigidbody2D _projectile_rigidbody = null;

  private List<Collider2D> _collision_lists = new();


  private Vector3 _direction;

  private float _object_lifetime = -1;

  private bool _is_triggered = false;


  private void _check_object(GameObject target_object){
    HealthComponent _health = target_object.GetComponent<HealthComponent>();
    if(_health == null)
      return;

    _health.DoDamage(_damager_data, this);
  }

  private void _object_collided(GameObject target_object){
    if(!enabled)
      return;

    _check_object(target_object);

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
  }

  public void FixedUpdate(){
    if(!_is_triggered)
      return;

    if(float.IsFinite(_object_lifetime) && _object_lifetime > 0){
      _object_lifetime -= Time.fixedDeltaTime;
      if(_object_lifetime <= 0)
        Destroy(gameObject);
    }
  }


  public void SetDamagerData(DamagerData damage_data){
    _damager_data = damage_data;

    gameObject.SendMessage("DamagerComponent_OnDataChanged", SendMessageOptions.DontRequireReceiver);
  }

  public void SetDamagerContext(DamagerContext context){
    _damager_context = context;

    transform.localScale = Vector3.one * _damager_context.ProjectileSize;
    
    if(_projectile_rigidbody != null){
      _projectile_rigidbody.sharedMaterial = _damager_context.ProjectileMaterial != null? _damager_context.ProjectileMaterial: new();
    }

    gameObject.SendMessage("DamagerComponent_OnContextChanged", SendMessageOptions.DontRequireReceiver);
  }


  public DamagerData GetDamagerData(){
    return _damager_data;
  }

  public DamagerContext GetDamagerContext(){
    return _damager_context;
  }
  

  public void SetDamagerExcludeLayer(LayerMask excluded_layer){
    LayerMask _result_layer = excluded_layer | _DamagerExcludeLayer;
    Debug.Log(string.Format("result exclude layer {0}", _result_layer));

    if(_projectile_rigidbody != null)
      _projectile_rigidbody.excludeLayers = _result_layer;

    foreach(Collider2D _col in _collision_lists)
      _col.excludeLayers = _result_layer;
  }

  public void SetDamagerSprite(Sprite texture){
    _ProjectileSprite.sprite = texture;
    _ProjectileSprite.size = Vector2.one;
  }


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



  public void OnCollisionEnter2D(Collision2D collision){
    _object_collided(collision.gameObject);
  }

  public void OnTriggerEnter2D(Collider2D collider){
    _object_collided(collider.gameObject);
  }
}