using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Database Class for loading and storing data for items.
/// The database will find any prefab (data) in a determined folder with the prefab that has <see cref="ItemMetadata"/> data component for early identification.
/// NOTE: for creating custom data component for an item, the database will use interface function <b>ItemDatabase_LoadData(TypeDataStorage)</b>.
/// </summary>
public class ItemDatabase: MonoBehaviour{
  /// <summary>
  /// Event for when the database has been initialized.
  /// </summary>
  public event OnInitializedCallback OnInitializedEvent;
  public delegate void OnInitializedCallback();
  
  /// <summary>
  /// Path to a folder containing prefabs of item data in Resources folder.
  /// </summary>
  static private string item_data_folder = "Items";


  private struct _item_metadata{
    public string item_id;

    // custom data about the item
    public TypeDataStorage _data_storage;

    // object prefab
    public GameObject _this;
  }


  private Dictionary<string, _item_metadata> _item_map = new Dictionary<string, _item_metadata>();

  
  /// <summary>
  /// Flag if this class is ready or not yet.
  /// </summary>
  public bool IsInitialized{get; private set;} = false;
  

  public void Start(){
    // load all prefab and store the data
    GameObject[] _prefab_list = Resources.LoadAll<GameObject>(item_data_folder);
    foreach(GameObject _prefab_obj in _prefab_list){
      GameObject _tmp_gameobj = Instantiate(_prefab_obj);
      ItemMetadata _metadata = _tmp_gameobj.GetComponent<ItemMetadata>();
      if(_metadata == null){
        Debug.LogError(string.Format("Prefab ({0}) is not an Item.", _prefab_obj.name));
        continue;
      }

      TypeDataStorage _new_item_data = new TypeDataStorage();
      _tmp_gameobj.SendMessage("ItemDatabase_LoadData", _new_item_data, SendMessageOptions.DontRequireReceiver);

      Destroy(_tmp_gameobj);

      DEBUGModeUtils.Log(string.Format("id: {0}", _metadata.GetItemID()));
      _item_map.Add(_metadata.GetItemID(), new _item_metadata{
        _this = _prefab_obj,
        _data_storage = _new_item_data,
        item_id = _metadata.GetItemID()
      });
    }

    IsInitialized = true;
    OnInitializedEvent?.Invoke();
  }

  
  /// <summary>
  /// Get a list of all items (by ID) stored in the database.
  /// </summary>
  /// <returns>The list of item ID</returns>
  public List<string> GetItemList(){
    return _item_map.Keys.ToList();
  }


  #nullable enable
  /// <summary>
  /// Get a certain item data based on the supplied ID.
  /// </summary>
  /// <param name="ItemID">The target item</param>
  /// <returns>Item data for the target item</returns>
  public TypeDataStorage? GetItemData(string ItemID){
    if(_item_map.ContainsKey(ItemID))
      return _item_map[ItemID]._data_storage;
    else
      return null;
  }
  #nullable disable
}