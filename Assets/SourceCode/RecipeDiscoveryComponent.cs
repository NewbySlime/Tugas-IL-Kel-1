using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;


[RequireComponent(typeof(InventoryData))]
public class RecipeDiscoveryComponent: MonoBehaviour{
  public delegate void OnRecipeDiscovered(string recipe_item_id);
  public event OnRecipeDiscovered OnRecipeDiscoveredEvent;

  [Serializable]
  public class RuntimeData: PersistanceContext.IPersistance{
    public string[] ListDiscoveredRecipe = new string[0];

    public string GetDataID(){
      return "RecipeDiscoveryComponent.Data";
    }


    public string GetData(){
      return ConvertExt.ToBase64String(JsonUtility.ToJson(this));
    }

    public void SetData(string data){
      JsonUtility.FromJsonOverwrite(ConvertExt.FromBase64String(data), this);
    }
  }

  [SerializeField]
  private bool _RemoveDiscoveryItemOnAdded = true;

  private HashSet<string> _list_known_recipe = new();

  private ItemDatabase _item_database;

  private InventoryData _inventory;


  // in case of bugs happening
  private IEnumerator _remove_item_co_func(string item_id){
    yield return new WaitForEndOfFrame();
    _inventory.RemoveItem(item_id, uint.MaxValue);
  }


  private void _on_item_added(string item_id, uint count){
    TypeDataStorage _data_storage = _item_database.GetItemData(item_id);
    if(_data_storage == null){
      Debug.LogWarning(string.Format("Item not found? (ID: {0})", item_id));
      return;
    }

    ItemRecipeDiscoveryData.ItemData _discovery_data = _data_storage.GetData<ItemRecipeDiscoveryData.ItemData>();
    if(_discovery_data == null)
      return;

    _list_known_recipe.Add(_discovery_data.RecipeForItemID);

    if(_RemoveDiscoveryItemOnAdded)
      StartCoroutine(_remove_item_co_func(item_id));

    OnRecipeDiscoveredEvent?.Invoke(item_id);
  }


  public void Start(){
    _item_database = FindAnyObjectByType<ItemDatabase>();
    if(_item_database == null){
      Debug.LogError("Cannot find database for Items.");
      throw new MissingReferenceException();
    }

    _inventory = GetComponent<InventoryData>();
    _inventory.OnItemAddedEvent += _on_item_added;
  }

  public List<string> GetListKnownRecipe(){
    List<string> _result = new();
    foreach(string _recipe_id in _list_known_recipe)
      _result.Add(_recipe_id);

    return _result;
  }


  public RuntimeData AsRuntimeData(){
    RuntimeData _data = new(){
      ListDiscoveredRecipe = new string[_list_known_recipe.Count]
    };

    int _idx = 0;
    foreach(string _id in _list_known_recipe){
      _data.ListDiscoveredRecipe[_idx] = _id;
      _idx++;
    }

    return _data;
  }


  public void FromRuntimeData(RuntimeData data){
    if(data == null)
      return;

    _list_known_recipe.Clear();
    foreach(string _id in data.ListDiscoveredRecipe)
      _list_known_recipe.Add(_id);
  }
}