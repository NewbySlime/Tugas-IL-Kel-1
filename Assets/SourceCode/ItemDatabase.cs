using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Database pada game untuk menyimpan data-data semua Item.
/// </summary>
public class ItemDatabase: MonoBehaviour{
  public delegate void OnInitializedCallback();
  public event OnInitializedCallback OnInitializedEvent;
  
  /// <summary>
  /// Folder-folder yang digunakan untuk menyimpan data item (Prefabs)
  /// </summary>
  static private string item_data_folder = "Items";

  private struct _item_metadata{
    public string item_id;

    public TypeDataStorage _data_storage;

    public GameObject _this;
  }


  private Dictionary<string, _item_metadata> _item_map = new Dictionary<string, _item_metadata>();

  
  public bool IsInitialized{get; private set;} = false;
  

  public void Start(){
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
  /// Untuk mendapatkan list semua ItemID yang ada pada game.
  /// </summary>
  /// <returns>List ItemID</returns>
  public List<string> GetItemList(){
    return _item_map.Keys.ToList();
  }


  #nullable enable
  /// <summary>
  /// Fungsi untuk mendapatkan data-data terkait item berdasarkan ItemID.
  /// </summary>
  /// <param name="ItemID">ID yang dicari</param>
  /// <returns>Data-data pada item</returns>
  public TypeDataStorage? GetItemData(string ItemID){
    if(_item_map.ContainsKey(ItemID))
      return _item_map[ItemID]._data_storage;
    else
      return null;
  }
  #nullable disable
}