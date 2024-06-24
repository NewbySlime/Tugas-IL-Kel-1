using System;
using System.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;


public class WeaponHandler: MonoBehaviour{
  private ItemDatabase _item_database;

  private Vector3 _last_position;

  private Vector2 _weapon_direction;

  private string _weapon_id;
  private WeaponItem.ItemData? _weapon_item = null;

  private float _weapon_delay = 0;


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
    _last_position = transform.position;

    if(_weapon_delay > 0)
      _weapon_delay -= Time.fixedDeltaTime;
  }


  public bool TriggerWeapon(){
    if(!CanShoot() || !_weapon_item.HasValue)
      return false;

    _weapon_delay = _weapon_item.Value.WeaponFireDelay;
    StartCoroutine(_trigger_weapon());

    return true;
  }

  public bool CanShoot(){
    return _weapon_delay <= 0;
  }


  public void LookAt(Vector2 position){
    _weapon_direction = (position-(Vector2)transform.position).normalized;
  }


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