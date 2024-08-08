using System;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Base item data for <see cref="ItemDatabase"/> that stores identifiers about the item. This data is important for creating an item.
/// </summary>
public class ItemMetadata: MonoBehaviour{
  /// <summary>
  /// Identification data for <see cref="ItemDatabase"/>.
  /// </summary>
  [Serializable]
  public struct ItemData{
    /// <summary>
    /// ID for this item.
    /// </summary>
    public string ItemID;

    /// <summary>
    /// Name of the item.
    /// </summary>
    public string Name;

    /// <summary>
    /// The description of the item.
    /// </summary>
    public string Description;
  }

  [SerializeField]
  private ItemData _item_metadata;


  /// <summary>
  /// Interface class for catching message from <see cref="ItemDatabase"/> and storing this class' data.
  /// </summary>
  /// <param name="data">Data storage from the database</param>
  public void ItemDatabase_LoadData(TypeDataStorage data){
    data.AddData(_item_metadata);
  }
  

  /// <summary>
  /// Get ID for this item.
  /// </summary>
  /// <returns>The ID</returns>
  public string GetItemID(){
    return _item_metadata.ItemID;
  }
}