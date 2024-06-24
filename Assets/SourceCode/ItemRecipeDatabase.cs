using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Database untuk membantu mencari resep Item.
/// Komponen ini memerlukan komponen lain:
///   - ItemDatabase
/// </summary>
public class ItemRecipeDatabase: MonoBehaviour{
  private class _recipe_node{
    public string ResultItemID = "";
    public Dictionary<string, _recipe_node> NextNodes = new Dictionary<string, _recipe_node>();
  }

  private _recipe_node _start_recipe_tree = new _recipe_node();
  private Dictionary<string, ItemRecipeData.ItemData> _item_recipe_map = new();

  private ItemDatabase _item_database;


  #nullable enable
  /// <summary>
  /// Fungsi untuk menerima "message" dari ItemDatabase sesudah inisialisasi.
  /// </summary>
  private void ItemDatabase_Initialized(){
    DEBUGModeUtils.Log("test start");
    List<string> _item_list = _item_database.GetItemList();
    DEBUGModeUtils.Log(_item_list.Count);
    foreach(string _item_id in _item_list){
      DEBUGModeUtils.Log("test");
      TypeDataStorage? _item_data = _item_database.GetItemData(_item_id);
      if(_item_data == null)
        continue;
        
      DEBUGModeUtils.Log("test1");
      ItemRecipeData.ItemData? _recipe_data = _item_data.GetData<ItemRecipeData.ItemData>();
      if(_recipe_data == null)  
        continue;

      DEBUGModeUtils.Log(string.Format("recipe id: {0}", _item_id));
      _item_recipe_map[_item_id] = _recipe_data;

      List<string> _sorted_item_list = _recipe_data.ItemList; _sorted_item_list.Sort();
      if(_sorted_item_list.Count() <= 0){
        Debug.LogWarning(string.Format("Item (ID: {0}) recipe is empty.", _item_id));
        continue;
      }

      _recipe_node _rnode = _start_recipe_tree;
      foreach(string _sorted_item in _sorted_item_list){
        // ngecek apakah item valid/ada?
        TypeDataStorage? _target_data = _item_database.GetItemData(_sorted_item);
        if(_target_data == null)
          Debug.LogWarning(string.Format("ItemID (ID: {0}) is not valid.", _sorted_item));

        if(!_rnode.NextNodes.ContainsKey(_sorted_item))
          _rnode.NextNodes[_sorted_item] = new _recipe_node();

        _rnode = _rnode.NextNodes[_sorted_item];
      }

      _rnode.ResultItemID = _item_id;
    }
  }
  #nullable disable


  public void Awake(){
    _item_database = GetComponent<ItemDatabase>();
    _item_database.OnInitializedEvent += ItemDatabase_Initialized;
  }


  /// <summary>
  /// Fungsi untuk mendapatkan hasil dari resep (list ItemID) atau pilihan yang diberikan.
  /// </summary>
  /// <param name="item_list">List pilihan item</param>
  /// <returns>ItemID jika resep berhasil, string kosong jika resep salah. </returns>
  public string RecipeParse(List<string> item_list){
    item_list.Sort();

    _recipe_node _rnode = _start_recipe_tree;
    foreach(string _item_id in item_list){
      DEBUGModeUtils.Log(string.Format("next item id: {0}", _item_id));
      foreach(string _key_item in _rnode.NextNodes.Keys)
        DEBUGModeUtils.Log(string.Format("list in _rnode: {0}", _key_item));

      if(!_rnode.NextNodes.ContainsKey(_item_id))
        return "";
      
      _rnode = _rnode.NextNodes[_item_id];
    }

    return _rnode.ResultItemID;
  }


  public List<string> GetItemNeeded(string item_id){
    List<string> _result = new();
    if(!_item_recipe_map.ContainsKey(item_id))
      return _result;

    ItemRecipeData.ItemData _recipe_data = _item_recipe_map[item_id];
    foreach(string _id in _recipe_data.ItemList)
      _result.Add(_id);

    return _result;
  }
}