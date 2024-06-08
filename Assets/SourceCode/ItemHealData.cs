using System;
using UnityEngine;


public class ItemHealData: MonoBehaviour{
  [Serializable]
  public class ItemData{
    public uint HealPoint;
  }

  [SerializeField]
  private ItemData _ItemData;


  public void ItemDatabase_LoadData(TypeDataStorage data){
    data.AddData(_ItemData);
  }
}