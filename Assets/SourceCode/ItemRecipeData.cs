using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Custom item data for <see cref="ItemDatabase"/> that stores recipe data used to create this item. This data will be specifically handled by <see cref="ItemRecipeDatabase"/>, but still be stored within <see cref="ItemDatabase"/>.
/// </summary>
public class ItemRecipeData: MonoBehaviour{
  [Serializable]
  /// <summary>
  /// Data structure for storing recipe info for this item.
  /// </summary>
  public class ItemData{
    [SerializeField]
    private List<string> _ItemList = new List<string>();

    /// <summary>
    /// List of items used to create this item. Cannot be changed.
    /// </summary>
    public List<string> ItemList{
      get{
        return _ItemList;
      }
    }

    /// <summary>
    /// Note: Later Feature
    /// Maximum score point an actor can achieve for next cooking system.
    /// </summary>
    public float FoodScore = 100f;

    /// <summary>
    /// Note: Later Feature
    /// Allow adding other items, as long as the combination still matches the original recipe.
    /// </summary>
    public bool AllowCustomRecipe = false;

    /// <summary>
    /// Note: Later Feature
    /// Stores which items are going to affect the resulting cook.
    /// </summary>
    public AddCustomRecipe CustomRecipe;
  }

  [Serializable]
  /// <summary>
  /// Note: Later Feature
  /// Data for affecting the results of the item cooked based on what items are added.
  /// </summary>
  public class AddCustomRecipe{
    [Serializable]
    /// <summary>
    /// Data used for scoring affected by adding unrelated items to the recipe.
    /// </summary>
    public struct ItemScore{
      public string ItemID;
      public float ScoreAdd;
    }

    /// <summary>
    /// If there are no specific items stored in <see cref="ItemScoreList"/>, use the default score.
    /// </summary>
    public float DefaultScoreAdd = 10;

    /// <summary>
    /// List of items that can affect the result of the cook.
    /// </summary>
    public List<ItemScore> ItemScoreList;
  }

  
  [SerializeField]
  private ItemData _item_recipe;


  /// <inheritdoc cref="ItemMetadata.ItemDatabase_LoadData(TypeDataStorage)"/>
  public void ItemDatabase_LoadData(TypeDataStorage data){
    data.AddData(_item_recipe);
  } 
}