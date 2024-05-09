using System;
using UnityEngine;


public class WeaponItem: MonoBehaviour{
  [Serializable]
  public struct ItemData{
    public DamagerComponent.DamageData DamageData;
    public DamagerComponent.DamagerContext DamagerContext;
  }

  [SerializeField]
  private ItemData _WeaponData;


  public void ItemDatabase_LoadData(TypeDataStorage data){
    data.AddData(_WeaponData);
  }
}