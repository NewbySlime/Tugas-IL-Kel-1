using System;
using UnityEngine;


public class WeaponItem: MonoBehaviour{
  [Serializable]
  public struct ItemData{
    public DamagerComponent.DamagerData DamagerData;
    public DamagerComponent.DamagerContext DamagerContext;

    public float WeaponFireDelay;

    public GameObject ProjectilePrefab;
    public Sprite ProjectileTexture;
  }

  [SerializeField]
  private ItemData _WeaponData;


  public void ItemDatabase_LoadData(TypeDataStorage data){
    data.AddData(_WeaponData);
  }
}