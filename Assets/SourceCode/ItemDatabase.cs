using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


/// <summary>
/// Database pada game untuk menyimpan data-data semua Item.
/// </summary>
public class ItemDatabase: MonoBehaviour{
  /// <summary>
  /// Folder-folder yang digunakan untuk menyimpan data item (Prefabs)
  /// </summary>
  static private string[] item_data_folder = {
    "Assets/Items/Makanan",
    "Assets/Items/BahanBahan"
  };

  private struct _item_metadata{
    public string item_id;

    public TypeDataStorage _data_storage;

    public GameObject _this;
    public string guid;
  }


  private Dictionary<string, _item_metadata> _item_map = new Dictionary<string, _item_metadata>();

  
  public bool IsInitialized{get; private set;} = false;
  
  public delegate void OnInitializedCallback();
  public event OnInitializedCallback OnInitializedEvent;
  

  public void Start(){
    string[] _prefab_guid_list = AssetDatabase.FindAssets("t:prefab", item_data_folder);
    foreach(string _guid in _prefab_guid_list){
      string _object_path = AssetDatabase.GUIDToAssetPath(_guid);
      GameObject _prefab_obj = AssetDatabase.LoadAssetAtPath<GameObject>(_object_path);

      GameObject _tmp_gameobj = Instantiate(_prefab_obj);
      ItemMetadata _metadata = _tmp_gameobj.GetComponent<ItemMetadata>();
      if(_metadata == null){
        Debug.LogError(String.Format("GUID: \"{0}\" is not an Item.", _guid));
        continue;
      }

      TypeDataStorage _new_item_data = new TypeDataStorage();
      _tmp_gameobj.SendMessage("ItemDatabase_LoadData", _new_item_data, SendMessageOptions.DontRequireReceiver);

      Destroy(_tmp_gameobj);

      Debug.Log(string.Format("id: {0}", _metadata.GetItemID()));
      _item_map.Add(_metadata.GetItemID(), new _item_metadata{
        _this = _prefab_obj,
        _data_storage = _new_item_data,
        item_id = _metadata.GetItemID(),
        guid = _guid
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