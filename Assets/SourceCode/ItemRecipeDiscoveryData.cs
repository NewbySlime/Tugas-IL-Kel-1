using System;
using UnityEngine;


/// <summary>
/// Custom item data for <see cref="ItemDatabase"/> as a data for signalling to "discovery" system (<see cref="RecipeDiscoveryComponent"/>) where it will tell the system that this item has an info on the recipe that an actor can use that previously unknown or the actor cannot create certain item unless they "discovered" the "recipe info".
/// </summary>
public class ItemRecipeDiscoveryData: MonoBehaviour{
  [Serializable]
  /// <summary>
  /// Data structure for storing target item recipe to be discovered.
  /// </summary>
  public class ItemData{
    /// <summary>
    /// Which recipe for a target item ID.
    /// </summary>
    public string RecipeForItemID;
  }


  [SerializeField]
  private ItemData _item_data;


  /// <inheritdoc cref="ItemMetadata.ItemDatabase_LoadData(TypeDataStorage)"/>
  public void ItemDatabase_LoadData(TypeDataStorage data){
    data.AddData(_item_data);
  }
}