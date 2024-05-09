using UnityEngine;


public class WeaponHandler: MonoBehaviour{  
  [SerializeField]
  private DamagerComponent _DamagerPrefab;

  #nullable enable
  private InventoryData? _bound_inventory = null;
  #nullable disable


  public void TriggerWeapon(){
    
  }


  public void LookAt(Vector2 position){

  }

  
  // kalau null, weapon tidak perlu deplete item
  #nullable enable
  public void SetInventory(InventoryData? inv_data){

  }
  #nullable disable
}