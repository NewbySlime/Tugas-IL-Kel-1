using System;
using UnityEngine;


/// <summary>
/// Custom item data for <see cref="ItemDatabase"/> to store weapon configuration data that will be used by <see cref="WeaponHandler"/>.
/// </summary>
public class WeaponItem: MonoBehaviour{
  [Serializable]
  /// <summary>
  /// Data structure for storing weapon configuration data.
  /// </summary>
  public struct ItemData{
    public DamagerComponent.DamagerData DamagerData;
    public DamagerComponent.DamagerContext DamagerContext;

    /// <summary>
    /// Weapon trigger cooldown.
    /// </summary>
    public float WeaponFireDelay;

    /// <summary>
    /// Prefab for spawning projectile.
    /// </summary>
    public GameObject ProjectilePrefab;
    public Sprite ProjectileTexture;
  }

  [SerializeField]
  private ItemData _WeaponData;


  /// <summary>
  /// <inheritdoc cref="ItemMetadata.ItemDatabase_LoadData(TypeDataStorage)"/>
  /// </summary>
  /// <param name="data"></param>
  public void ItemDatabase_LoadData(TypeDataStorage data){
    data.AddData(_WeaponData);
  }
}