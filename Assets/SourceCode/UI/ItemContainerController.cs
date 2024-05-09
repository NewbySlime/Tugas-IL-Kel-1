using System.Collections.Generic;
using System.Linq;
using UnityEngine;



/// <summary>
/// Komponen untuk mengatur Container kumpulan dari ItemButton.
/// </summary>
public class ItemContainerController: MonoBehaviour{
  /// <summary>
  /// Event jika salah satu ItemButton pada ItemContainerController diklik.
  /// </summary>
  public event OnItemButtonPressed OnItemButtonPressedEvent;
  public delegate void OnItemButtonPressed(string item_id);


  [SerializeField]
  private GameObject _ButtonPrefab;

  private Dictionary<string, ItemButton> _button_list = new Dictionary<string, ItemButton>();
  private List<string> _item_list = new List<string>();



  /// <summary>
  /// Fungsi helper untuk membantu proses pembuatan ItemButton dari Prefab.
  /// </summary>
  /// <returns>Komponen ItemButton dari GameObject yang dibuat</returns>
  private ItemButton _CreateNewItemButton(){
    GameObject _new_obj = Instantiate(_ButtonPrefab);
    ItemButton _item_button = _new_obj.GetComponent<ItemButton>();
    if(_item_button == null)
      return null;

    _item_button.OnButtonPressedEvent += ItemButton_OnButtonPressed;
    return _item_button;
  }


  public void Start(){
    for(int i = 0; i < transform.childCount; i++)
      Destroy(transform.GetChild(i).gameObject);

    RemoveAllItem();
  }


  /// <summary>
  /// Fungsi untuk menambahkan Item ke Container, tombol dan UI lainnya akan dipersiapkan.
  /// </summary>
  /// <param name="item_id">ID Item yang mau ditambahkan</param>
  /// <param name="count">Jumlah Item yang mau ditambahkan</param>
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
  /// Fungsi untuk mengubah jumlah Item pada Tombol atau UI lainnya.
  /// </summary>
  /// <param name="item_id">ID Item yang mau dimodifikasi</param>
  /// <param name="count">ID Item yang mau diganti jumlahnya</param>
  public void ChangeItemCount(string item_id, uint count){
    if(!_button_list.ContainsKey(item_id))
      AddItem(item_id, count);
    else{
      ItemButton _item_button = _button_list[item_id];
      _item_button.SetItemCount(count);
    }
  }

  /// <summary>
  /// Fungsi untuk menghilangkan Item pada Container ini.
  /// </summary>
  /// <param name="item_id">ID Item yang mau dihapus</param>
  public void RemoveItem(string item_id){
    if(!_button_list.ContainsKey(item_id))
      return;

    ItemButton _item_button = _button_list[item_id];
    Destroy(_item_button.gameObject);

    _button_list.Remove(item_id);
    _item_list.Remove(item_id);
  }


  /// <summary>
  /// Fungsi untuk mendapatkan list Item yang ada pada Container ini.
  /// </summary>
  /// <returns>List ID Item pada Container</returns>
  public List<string> GetListItem(){
    return _item_list;
  }


  /// <summary>
  /// Fungsi untuk menghapus semua Item dari Container.
  /// </summary>
  public void RemoveAllItem(){
    List<string> _item_keylist = _button_list.Keys.ToList();
    foreach(string _item_id in _item_keylist)
      RemoveItem(_item_id);
  }


  /// <summary>
  /// Fungsi untuk menerima event dari ItemButton, event OnButtonPressed
  /// </summary>
  /// <param name="item_id"></param>
  public void ItemButton_OnButtonPressed(string item_id){
    OnItemButtonPressedEvent?.Invoke(item_id);
  }
}