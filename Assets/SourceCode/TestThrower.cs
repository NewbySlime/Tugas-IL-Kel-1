using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(WeaponHandler))]
public class TestThrower: MonoBehaviour{
  [SerializeField]
  private Transform _TargetToThrow;

  [SerializeField]
  private WeaponHandler _WeaponHandler;

  [SerializeField]
  private string _WeaponItemID;

  private WeaponHandler _weapon_handler;
  private ItemDatabase _item_database;




  public void Start(){
    _item_database = FindAnyObjectByType<ItemDatabase>();
    if(_item_database == null){
      Debug.LogError("Cannot find database for Items.");
      throw new MissingReferenceException();
    }
  }


  public void OnTestTrigger(InputValue value){
    if(value.isPressed){
      _WeaponHandler.SetWeaponItem(_WeaponItemID);
      _WeaponHandler.TriggerWeapon();

      _WeaponHandler.LookAt(_TargetToThrow.position);
    }
  }
}