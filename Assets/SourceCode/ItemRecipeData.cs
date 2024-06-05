using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Komponen untuk menyimpan data resep pada Item.
/// Komponen yang diperlukan:
///   - ItemMetadata
/// </summary>
public class ItemRecipeData: MonoBehaviour{
  [Serializable]
  public class ItemData{
    [SerializeField]
    private List<string> _ItemList = new List<string>();
    public List<string> ItemList{
      get{
        return _ItemList;
      }
    }

    public float FoodScore = 100f;

    public bool AllowCustomRecipe = false;
    public AddCustomRecipe CustomRecipe;
  }

  [Serializable]
  public class AddCustomRecipe{
    [Serializable]
    public struct ItemScore{
      public string ItemID;
      public float ScoreAdd;
    }

    public float DefaultScoreAdd = 10;
    public List<ItemScore> ItemScoreList;
  }

  
  [SerializeField]
  private ItemData _item_recipe;


  /// <summary>
  /// Fungsi PENTING untuk membantu proses inisialisasi ItemDatabase.
  /// </summary>
  /// <param name="data">Penyimpanan data berdasarkan tipe yang diberikan dari ItemDatabase</param>
  public void ItemDatabase_LoadData(TypeDataStorage data){
    data.AddData(_item_recipe);
  } 
}