using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Komponen untuk menyimpan (berfungsi sebagai sistem Inventory) Item-Item yang dipunyai oleh suatu Object.
/// </summary>
public class InventoryData: MonoBehaviour{
  /// <summary>
  /// Event untuk memberitahu apakah ada Item yang terganti jumlahnya.
  /// </summary>
  public event OnItemCountChanged OnItemCountChangedEvent;
  public delegate void OnItemCountChanged(string item_id, uint new_count);

  /// <summary>
  /// Event untuk memberitahu apakah ada Item yang ditambahkan ke Inventory.
  /// </summary>
  public event OnItemAdded OnItemAddedEvent;
  public delegate void OnItemAdded(string item_id, uint count);
   
  /// <summary>
  /// Event untuk memberitahu apakah ada item yang dihapus/dihilangkan dari Inventory.
  /// </summary>
  public event OnItemRemoved OnItemRemovedEvent;
  public delegate void OnItemRemoved(string item_id);

  private struct _item_data{
    public string item_id;
    public uint item_count;
  }


  private Dictionary<string, _item_data> _item_list = new Dictionary<string, _item_data>();
  
  private ItemDatabase _item_database;


  public void Start(){
    _item_database = FindObjectOfType<ItemDatabase>();
  }


  #nullable enable
  /// <summary>
  /// Fungsi untuk menambahkan Item ke Inventory.
  /// </summary>
  /// <param name="item_id">ID Item yang mau dimasukkan</param>
  /// <param name="count">Jumlah Item yang mau ditambahkan</param>
  public void AddItem(string item_id, uint count = 1){
    TypeDataStorage? _item_data = _item_database.GetItemData(item_id);
    if(_item_data == null){
      Debug.LogError(string.Format("Item (ID: {0}) is not a valid ID.", item_id));
      return;
    }

    if(!_item_list.ContainsKey(item_id)){
      _item_list[item_id] = new _item_data{
        item_id = item_id,
        item_count = count
      };
      
      OnItemAddedEvent?.Invoke(item_id, count);
    }
    else{
      _item_data _item = _item_list[item_id];
      _item.item_count += count;
      _item_list[item_id] = _item;

      OnItemCountChangedEvent?.Invoke(item_id, _item.item_count);
    }
  }
  #nullable disable

  /// <summary>
  /// Fungsi untuk menghapus Item dari Inventory. Bisa juga yang dihapus cuma beberapa Item yang sama.
  /// </summary>
  /// <param name="item_id">ID Item yang mau dihapus</param>
  /// <param name="count">Jumlah Item yang mau dihapus</param>
  public void RemoveItem(string item_id, uint count){
    if(!_item_list.ContainsKey(item_id))
      return;

    _item_data _item = _item_list[item_id];
    if(_item.item_count > count){
      _item.item_count -= count;
      _item_list[item_id] = _item;
      
      OnItemCountChangedEvent?.Invoke(item_id, _item.item_count);
    }
    else{
      _item_list.Remove(item_id);

      OnItemRemovedEvent?.Invoke(item_id);
    }
  }

  /// <summary>
  /// Fungsi untuk menghilangkan Item berdasarkan list yang diberikan. Jumlah Item yang dikurangi tergantung ada berapa ID Item yang sama pada list tersebut.
  /// </summary>
  /// <param name="list_item">List ID Item yang mau dihapus</param>
  /// <returns>Apakah semua Item berhasil dihapus atau tidak</returns>
  public bool RemoveItemList(List<string> list_item){
    Dictionary<string, uint> _list_map = new Dictionary<string, uint>();
    foreach(string _item_id in list_item){
      if(!_item_list.ContainsKey(_item_id))
        return false;

      _item_data _data = _item_list[_item_id];
      uint _item_occurence = 0;
      if(_list_map.ContainsKey(_item_id))
        _item_occurence = _list_map[_item_id];
      _item_occurence++;

      if(_data.item_count < _item_occurence)
        return false;
      
      _list_map[_item_id] = _item_occurence;
    }

    foreach(string _key_item in _list_map.Keys){
      uint _item_count = _list_map[_key_item];
      RemoveItem(_key_item, _item_count);
    }

    return true;
  }


  /// <summary>
  /// Fungsi untuk mendapatkan list Item-Item yang ada di Inventory.
  /// </summary>
  /// <returns>List Item ID</returns>
  public List<string> GetContainedItems(){
    return _item_list.Keys.ToList();
  }

  /// <summary>
  /// Fungsi untuk mendapatkan berapa banyak Item berdasarkan Item ID.
  /// </summary>
  /// <param name="item_id">Item ID yang ingin dicek</param>
  /// <returns>Berapa banyak Item yang ada di Inventory</returns>
  public uint GetItemCount(string item_id){
    if(!_item_list.ContainsKey(item_id))
      return 0;

    return _item_list[item_id].item_count;
  }
}