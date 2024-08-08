using System;
using UnityEngine;


/// <summary>
/// Custom item data for <see cref="ItemDatabase"/> that stores healing data to be used by <see cref="ItemCollectionHealerComponent"/>.
/// </summary>
public class ItemHealData: MonoBehaviour{
  [Serializable]
  /// <summary>
  /// Data structure for storing heal info for an item.
  /// </summary>
  public class ItemData{
    /// <summary>
    /// The amount of heal point in this item.
    /// </summary>
    public uint HealPoint;
  }

  [SerializeField]
  private ItemData _ItemData;


  /// <inheritdoc cref="ItemMetadata.ItemDatabase_LoadData(TypeDataStorage)"/>
  public void ItemDatabase_LoadData(TypeDataStorage data){
    data.AddData(_ItemData);
  }
}