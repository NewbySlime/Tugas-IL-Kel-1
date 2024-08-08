using System;
using UnityEngine;


/// <summary>
/// Custom item data for <see cref="ItemDatabase"/> to store sprites used for this item.
/// </summary>
public class ItemTextureData: MonoBehaviour{
  [Serializable]
  /// <summary>
  /// Data structure for storing item images.
  /// </summary>
  public class ItemData{
    /// <summary>
    /// General usage texture for item.
    /// </summary>
    public Sprite SpriteTexture;
  }

  [SerializeField]
  private ItemData _ItemData;


  /// <inheritdoc cref="ItemMetadata.ItemDatabase_LoadData(TypeDataStorage)"/>
  public void ItemDatabase_LoadData(TypeDataStorage item_data){
    item_data.AddData(_ItemData);
  }
}