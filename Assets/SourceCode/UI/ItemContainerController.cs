using System.Collections.Generic;
using System.Linq;
using UnityEngine;



/// <summary>
/// UI Component for creating, handling and containing the list of items in the component that can be interacted.
/// 
/// This class uses prefab(s);
/// - Prefab that has <see cref="ItemButton"/> component for creating buttons for items.
/// </summary>
public class ItemContainerController: MonoBehaviour{
  /// <summary>
  /// Event for when one of the item buttons interacted (pressed).
  /// This event also gives the ID for the contained item related to the interacted button. 
  /// </summary>
  public event OnItemButtonPressed OnItemButtonPressedEvent;
  public delegate void OnItemButtonPressed(string item_id);


  [SerializeField]
  private GameObject _ButtonPrefab;

  private Dictionary<string, ItemButton> _button_list = new Dictionary<string, ItemButton>();
  private List<string> _item_list = new List<string>();



  private ItemButton _CreateNewItemButton(){
    GameObject _new_obj = Instantiate(_ButtonPrefab);
    ItemButton _item_button = _new_obj.GetComponent<ItemButton>();
    if(_item_button == null)
      return null;

    _item_button.OnButtonPressedEvent += _OnItemButtonPressed;
    return _item_button;
  }

  private void _OnItemButtonPressed(string item_id){
    OnItemButtonPressedEvent?.Invoke(item_id);
  }


  public void Start(){
    for(int i = 0; i < transform.childCount; i++)
      Destroy(transform.GetChild(i).gameObject);

    RemoveAllItem();
  }


  /// <summary>
  /// Function for adding item to the container based on the ID.
  /// </summary>
  /// <param name="item_id">The item ID</param>
  /// <param name="count">The item count</param>
  public void AddItem(string item_id, uint count){
    if(_button_list.ContainsKey(item_id))
      RemoveItem(item_id);

    ItemButton _this_button = _CreateNewItemButton();
    if(_this_button == null){
      Debug.LogError("Cannot create ItemButton.");
      return;
    }

    _button_list[item_id] = _this_button;
    _item_list.Add(item_id);
    _item_list.Sort();

    int _button_index = _item_list.IndexOf(item_id);
    _this_button.transform.SetParent(transform);
    _this_button.transform.SetSiblingIndex(_button_index+1);

    _this_button.SetItem(item_id);
    _this_button.SetItemCount(count);
  }

  /// <summary>
  /// Function to change the item count for one of the contained item.
  /// But if the container does not have the supposed item, the container will add the item using <see cref="AddItem"/> function.
  /// </summary>
  /// <param name="item_id">The target item ID</param>
  /// <param name="count">The item count</param>
  public void ChangeItemCount(string item_id, uint count){
    if(!_button_list.ContainsKey(item_id))
      AddItem(item_id, count);
    else{
      ItemButton _item_button = _button_list[item_id];
      _item_button.SetItemCount(count);
    }
  }

  /// <summary>
  /// Function to remove the target item from the container.
  /// </summary>
  /// <param name="item_id">The target item ID</param>
  public void RemoveItem(string item_id){
    if(!_button_list.ContainsKey(item_id))
      return;

    ItemButton _item_button = _button_list[item_id];
    Destroy(_item_button.gameObject);

    _button_list.Remove(item_id);
    _item_list.Remove(item_id);
  }


  /// <summary>
  /// Function to get the list of contained item(s).
  /// </summary>
  /// <returns>The resulting list of item(s)</returns>
  public List<string> GetListItem(){
    return _item_list;
  }


  /// <summary>
  /// Function to remove all item from the container.
  /// </summary>
  public void RemoveAllItem(){
    List<string> _item_keylist = _button_list.Keys.ToList();
    foreach(string _item_id in _item_keylist)
      RemoveItem(_item_id);
  }
}