using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;


/// <summary>
/// Component to store items. Items can be added manually or automatically by class like <see cref="CollectibleComponent"/>.
/// This component store items by using list structure. Every stored item will be compiled in a list.
/// 
/// This class uses autoload(s);
/// - <see cref="ItemDatabase"/> database for getting data about certain item.
/// </summary>
public class InventoryData: MonoBehaviour{
  /// <summary>
  /// Event for when a count for an item in the inventory have been modified (added or removed for x item(s)).
  /// </summary>
  public event OnItemCountChanged OnItemCountChangedEvent;
  public delegate void OnItemCountChanged(string item_id, uint new_count);

  /// <summary>
  /// Event for when item with x count has been added to the inventory.
  /// </summary>
  public event OnItemAdded OnItemAddedEvent;
  public delegate void OnItemAdded(string item_id, uint count);

  /// <summary>
  /// Event for when item with x count has been removed from the inventory.
  /// </summary>
  public event OnItemRemoved OnItemRemovedEvent;
  public delegate void OnItemRemoved(string item_id);


  [Serializable]
  /// <summary>
  /// Data structure for storing data used by <see cref="InventoryData"/> of certain item. 
  /// </summary>
  public class ItemData{
    public string item_id;
    public uint item_count;
  }


  [Serializable]
  /// <summary>
  /// Data structure for save file that contains data related to the <see cref="InventoryData"/>.
  /// </summary>
  public class RuntimeData: PersistanceContext.IPersistance{
    /// <summary>
    /// The items stored in the inventory.
    /// </summary>
    public ItemData[] ListItem = new ItemData[0];


    public string GetDataID(){
      return "Inv.RuntimeData";
    }


    public string GetData(){
      return ConvertExt.ToBase64String(JsonUtility.ToJson(this));
    }

    public void SetData(string data){
      JsonUtility.FromJsonOverwrite(ConvertExt.FromBase64String(data), this);
    }
  }


  private Dictionary<string, ItemData> _item_list = new Dictionary<string, ItemData>();
  
  private ItemDatabase _item_database;
  

  public void Start(){
    _item_database = FindObjectOfType<ItemDatabase>();
  }


  #nullable enable
  /// <summary>
  /// To add an item to this inventory system.
  /// </summary>
  /// <param name="item_id">What kind of item to be added</param>
  /// <param name="count">How many items to be added</param>
  public void AddItem(string item_id, uint count = 1){
    TypeDataStorage? _item_data = _item_database.GetItemData(item_id);
    if(_item_data == null){
      Debug.LogError(string.Format("Item (ID: {0}) is not a valid ID.", item_id));
      return;
    }

    if(!_item_list.ContainsKey(item_id)){
      DEBUGModeUtils.Log(string.Format("item added {0}", item_id));
      _item_list[item_id] = new ItemData{
        item_id = item_id,
        item_count = count
      };
      
      OnItemAddedEvent?.Invoke(item_id, count);
    }
    else{
      DEBUGModeUtils.Log(string.Format("item count changed {0}", item_id));
      ItemData _item = _item_list[item_id];
      _item.item_count += count;
      _item_list[item_id] = _item;

      OnItemCountChangedEvent?.Invoke(item_id, _item.item_count);
    }
  }
  #nullable disable


  /// <summary>
  /// To remove an item from this inventory system.
  /// </summary>
  /// <param name="item_id">What kind of item to be removed</param>
  /// <param name="count">How many items to be removed</param>
  public void RemoveItem(string item_id, uint count){
    if(!_item_list.ContainsKey(item_id))
      return;

    ItemData _item = _item_list[item_id];
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
  /// Remove all item based on the filter (list of item ID) supplied. This will actually remove all the related items if all referenced item in the filter exist in the inventory. 
  /// This will remove any item regardless of how many they are in the inventory.
  /// </summary>
  /// <param name="list_item">The filter of items to be removed</param>
  /// <returns></returns>
  public bool RemoveItemList(List<string> list_item){
    Dictionary<string, uint> _list_map = new Dictionary<string, uint>();
    foreach(string _item_id in list_item){
      if(!_item_list.ContainsKey(_item_id))
        return false;

      ItemData _data = _item_list[_item_id];
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
  /// Remove all items stored in the inventory.
  /// </summary>
  public void RemoveAllItem(){
    List<string> _list_remove = _item_list.Keys.ToList();
    foreach(string key in _list_remove)
      RemoveItem(key, uint.MaxValue);
  }


  /// <summary>
  /// Get a list of item IDs contained in the inventory. This does not get the count of the item in the inventory.
  /// </summary>
  /// <returns>The list of items ocntained</returns>
  public List<string> GetContainedItems(){
    return _item_list.Keys.ToList();
  }

  /// <summary>
  /// Get the count of certain item contained in the inventory.
  /// </summary>
  /// <param name="item_id">The target item to get</param>
  /// <returns>Item count</returns>
  public uint GetItemCount(string item_id){
    if(!_item_list.ContainsKey(item_id))
      return 0;

    return _item_list[item_id].item_count;
  }



  /// <summary>
  /// Get the stored state of this component. For applying the state to this component, see <see cref="FromRuntimeData"/>.
  /// </summary>
  /// <returns>The resulting state</returns>
  public RuntimeData AsRuntimeData(){
    RuntimeData _res = new();
    _res.ListItem = new ItemData[_item_list.Count];
    
    int idx = 0;
    foreach(ItemData _item in _item_list.Values){
      DEBUGModeUtils.Log(string.Format("runtime removing item {0}", _item.item_id));
      _res.ListItem[idx] = _item;

      idx++;
    }

    return _res;
  }

  /// <summary>
  /// Apply and modify this component to recreate from the supplied state. For getting current state of the object, see <see cref="AsRuntimeData"/>.
  /// </summary>
  /// <param name="data">The previous state or any other stored state</param>
  public void FromRuntimeData(RuntimeData data){
    if(data == null)
      return;

    DEBUGModeUtils.Log("runtime from");
    RemoveAllItem();
    foreach(ItemData _item in data.ListItem){
      DEBUGModeUtils.Log(string.Format("runtime adding item {0}", _item.item_id));
      AddItem(_item.item_id, _item.item_count);
    }
  }
}