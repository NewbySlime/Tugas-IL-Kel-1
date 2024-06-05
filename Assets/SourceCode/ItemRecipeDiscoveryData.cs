using System;
using UnityEngine;


public class ItemRecipeDiscoveryData: MonoBehaviour{
  [Serializable]
  public class ItemData{
    public string RecipeForItemID;
  }


  [SerializeField]
  private ItemData _item_data;



  public void ItemDatabase_LoadData(TypeDataStorage data){
    data.AddData(_item_data);
  }
}