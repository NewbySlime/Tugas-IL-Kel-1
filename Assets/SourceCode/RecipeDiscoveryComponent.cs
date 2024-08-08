using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;


[RequireComponent(typeof(InventoryData))]
/// <summary>
/// Discovery system component for handling recipe discovery that the player can use as a "remembering" system for recipes.
/// 
/// This class uses following component(s);
/// - <see cref="InventoryData"/> to watch this object's inventory system.
///
/// This class uses autoload(s);
/// - <see cref="ItemDatabase"/> to get certain data about an item.
/// </summary>
public class RecipeDiscoveryComponent: MonoBehaviour{
  /// <summary>
  /// Event for when a recipe has been discovered.
  /// </summary>
  public event OnRecipeDiscovered OnRecipeDiscoveredEvent;
  public delegate void OnRecipeDiscovered(string recipe_item_id);

  [Serializable]
  /// <summary>
  /// Data structure for storing to a save file in for <see cref="PersistanceContext"/>.
  /// </summary>
  public class RuntimeData: PersistanceContext.IPersistance{
    /// <summary>
    /// A list of item ID that the object can create (not the item ID for recipe).
    /// </summary>
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
  // Instantly remove item that has ItemRecipeData when added to this inventory system.
  private bool _RemoveDiscoveryItemOnAdded = true;

  private HashSet<string> _list_known_recipe = new();

  private ItemDatabase _item_database;

  private InventoryData _inventory;


  // In case of bugs happening, this function will wait until all process has ran for this update.
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

  /// <summary>
  /// List for item IDs that the object has the recipe for.
  /// </summary>
  /// <returns>List of item IDs</returns>
  public List<string> GetListKnownRecipe(){
    List<string> _result = new();
    foreach(string _recipe_id in _list_known_recipe)
      _result.Add(_recipe_id);

    return _result;
  }


  /// <summary>
  /// Get the stored state of discovery component. For applying a state to this discovery component, see <see cref="FromRuntimeData"/>.
  /// </summary>
  /// <returns>The resulting state</returns>
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


  /// <summary>
  /// Apply and modify this discovery component to recreate a state. For getting current state of the discovery component, see <see cref="AsRuntimeData"/>.
  /// </summary>
  /// <param name="data">The state to use for recreating</param>
  public void FromRuntimeData(RuntimeData data){
    if(data == null)
      return;

    _list_known_recipe.Clear();
    foreach(string _id in data.ListDiscoveredRecipe)
      _list_known_recipe.Add(_id);
  }
}