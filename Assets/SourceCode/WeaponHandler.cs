using System;
using System.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// Component for handling weapon and gives its wielder (such as <see cref="PlayerController"/>) Game interactions for combat system. The weapon bound in this component is based on the bound item that has data about <see cref="WeaponItem"/>.
/// For ammo count handling, it is controlled by the wielder due to some wielder such as enemies might use the weapon with indefinite ammot count.
/// 
/// This class uses autoload(s);
/// - <see cref="ItemDatabase"/> for getting data about certain item. 
/// </summary>
public class WeaponHandler: MonoBehaviour{
  private ItemDatabase _item_database;

  private Vector3 _last_position;

  private Vector2 _weapon_direction;

  private string _weapon_id;
  private WeaponItem.ItemData? _weapon_item = null;

  private float _weapon_delay = 0;


  // This function uses coroutine to wait DamagerComponent finished initializing.
  private IEnumerator _trigger_weapon(){
    DEBUGModeUtils.Log(string.Format("firing has value {0}", _weapon_item.HasValue));
    if(!_weapon_item.HasValue)
      yield break;

    WeaponItem.ItemData _weapon_data = _weapon_item.Value;
    GameObject _projectile = Instantiate(_weapon_data.ProjectilePrefab);
    
    yield return null;
    yield return new WaitForEndOfFrame();

    try{
      DEBUGModeUtils.Log("firing damager instantiated");
      DamagerComponent _damager = _projectile.GetComponent<DamagerComponent>();
      if(_damager == null)
        throw new Exception(string.Format("Projectile doesn't have DamagerComponent. (Item ID: '{0}')", _weapon_id));

      _damager.SetDamagerData(_weapon_data.DamagerData);
      _damager.SetDamagerContext(_weapon_data.DamagerContext);
      _damager.SetDamagerSprite(_weapon_data.ProjectileTexture);
      _damager.SetDamagerExcludeLayer(1 << gameObject.layer);

      DEBUGModeUtils.Log(string.Format("exclude layer: {0}", gameObject.layer));

      _damager.TriggerDamager(new DamagerComponent.DamagerTriggerData{
        StartPosition = transform.position,
        Direction = _weapon_direction
      });

      DEBUGModeUtils.Log("firing damager triggered");
    }
    catch(Exception e){
      Debug.LogError(string.Format("Something went wrong when firing weapon. (ID: {0})", _weapon_id));
      Debug.LogError(e.ToString());

      Destroy(_projectile);
      yield break;
    }
  }


  public void Start(){
    _item_database = FindAnyObjectByType<ItemDatabase>();
    if(_item_database == null){
      Debug.LogError("Cannot find database for Items.");
      throw new MissingReferenceException();
    }

    
    _last_position = transform.position;
  }

  public void FixedUpdate(){
    // handles timing for weapon's cooldown
    _last_position = transform.position;

    if(_weapon_delay > 0)
      _weapon_delay -= Time.fixedDeltaTime;
  }


  /// <summary>
  /// Trigger (fire/shoot/slash) the weapon.
  /// This will not be triggered based on <see cref="CanShoot"/> function and if this class does not have bound weapon item.
  /// For redirecting where the weapon should be pointing, see <see cref="LookAt"/>
  /// </summary>
  /// <returns>If triggering successful</returns>
  public bool TriggerWeapon(){
    if(!CanShoot() || !_weapon_item.HasValue)
      return false;

    _weapon_delay = _weapon_item.Value.WeaponFireDelay;
    StartCoroutine(_trigger_weapon());

    return true;
  }

  /// <summary>
  /// Check if this class can trigger another one.
  /// </summary>
  /// <returns>Is this weapon can be triggered</returns>
  public bool CanShoot(){
    return _weapon_delay <= 0;
  }


  /// <summary>
  /// Points the weapon to a target position.
  /// </summary>
  /// <param name="position">The target position to point at</param>
  public void LookAt(Vector2 position){
    _weapon_direction = (position-(Vector2)transform.position).normalized;
  }


  /// <summary>
  /// Bind this component with certain item that has <see cref="WeaponItem"/> data and configure this component based on it.
  /// </summary>
  /// <param name="item_id">The target item ID to bind with</param>
  /// <returns>If the binding was successful</returns>
  public bool SetWeaponItem(string item_id){
    TypeDataStorage _item_data = _item_database.GetItemData(item_id);
    if(_item_data == null)
      return false;

    WeaponItem.ItemData? _weapon_data = _item_data.GetData<WeaponItem.ItemData>();
    if(!_weapon_data.HasValue)
      return false;

    _weapon_id = item_id;
    _weapon_item = _weapon_data;
    return true;
  }

  /// <summary>
  /// Get the bound item ID for the weapon configuration.
  /// </summary>
  /// <returns></returns>
  public string GetWeaponItem(){
    return _weapon_id;
  }


#if UNITY_EDITOR
  private void OnDrawGizmos(){
    Gizmos.color = Color.gray;
    Gizmos.DrawLine(transform.position, transform.position + (Vector3)(_weapon_direction * 2));
  }
#endif
}