using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;



/// <summary>
/// Komponen untuk mengontrol UI pada Crafting masakan.
/// </summary>
public class RecipeCraftingUIController: MonoBehaviour{
  [SerializeField]
  private GameObject _ItemInventoryListContent;
  [SerializeField]
  private GameObject _RecipeCraftListContent;
  [SerializeField]
  private GameObject _RecipeCraftResultContent;

  [SerializeField]
  private GameObject _WrapperContent;

  [SerializeField]
  private GameObject _ItemButtonPrefab;


  private ItemRecipeDatabase _item_recipe_database;
  private InventoryData _player_inv_data;

  private ItemContainerController _item_inv_controller;
  private ItemContainerController _recipe_craft_list_controller;
  private ItemContainerController _recipe_craft_res_controller;

  private GridLayoutGroup _item_inv_grouping;


  private bool _this_enable_toggle = false;


  /// <summary>
  /// Fungsi untuk mengubah "keaktifan" fitur terkait pada komponen ini.
  /// </summary>
  /// <param name="_enable">Aktif atau tidak</param>
  private void _SetEnable(bool _enable){
    _WrapperContent.SetActive(_enable);
  } 

  /// <summary>
  /// Fungsi untuk mengupdate ItemController Inventory pada Player.
  /// </summary>
  private void _UpdateInventory(){
    _item_inv_controller.RemoveAllItem();

    if(_player_inv_data == null)
      return;

    List<string> _list_items = _player_inv_data.GetContainedItems();
    foreach(string _item_id in _list_items){
      uint _item_count = _player_inv_data.GetItemCount(_item_id);
      _item_inv_controller.AddItem(_item_id, _item_count);
    }

    _UpdateInventoryContainerSize();
  }

  /// <summary>
  /// Fungsi untuk mengupdate size pada ItemContainer Inventory pada Player.
  /// Ini diperlukan agar fungsi scroll pada ItemContainer bisa dipakai.
  /// </summary>
  private void _UpdateInventoryContainerSize(){
    int _total_size = _ItemInventoryListContent.transform.childCount;

    RectTransform _item_inv_transform = _ItemInventoryListContent.GetComponent<RectTransform>();
    switch(_item_inv_grouping.constraint){
      case GridLayoutGroup.Constraint.FixedColumnCount:{
        int _y_size = (_total_size / _item_inv_grouping.constraintCount) + 1;
        _item_inv_transform.sizeDelta = new Vector2(_item_inv_transform.sizeDelta.x,
          _y_size *
          (_item_inv_grouping.cellSize.y + _item_inv_grouping.spacing.y)
        );
      }break;

      case GridLayoutGroup.Constraint.FixedRowCount:{
        int _x_size = (_total_size / _item_inv_grouping.constraintCount) + 1;
        _item_inv_transform.sizeDelta = new Vector2(
          _x_size *
          (_item_inv_grouping.cellSize.x + _item_inv_grouping.spacing.x),
        _item_inv_transform.sizeDelta.y);
      }break;
    }
  }

  /// <summary>
  /// Fungsi untuk mengecek apakah resep yang dipakai benar. Output kebenarannya diberikan ke UI tampilan hasil resep.
  /// </summary>
  private void _UpdateRecipeResult(){
    _recipe_craft_res_controller.RemoveAllItem();

    List<string> _item_list = _recipe_craft_list_controller.GetListItem();
    string _result_id = _item_recipe_database.RecipeParse(_item_list);
    Debug.Log(string.Format("hasil masakan: {0}", _result_id));

    if(_result_id.Length > 0)
      _recipe_craft_res_controller.AddItem(_result_id, 1);
  }


  public void Start(){
    _this_enable_toggle = false;
    _SetEnable(false);

    ItemDatabase _item_database = FindAnyObjectByType<ItemDatabase>();
    _item_recipe_database = _item_database.gameObject.GetComponent<ItemRecipeDatabase>();

    _item_inv_controller = _ItemInventoryListContent.GetComponent<ItemContainerController>();
    _item_inv_controller.OnItemButtonPressedEvent += OnInventory_ItemButtonPressed;

    _recipe_craft_list_controller = _RecipeCraftListContent.GetComponent<ItemContainerController>();
    _recipe_craft_list_controller.OnItemButtonPressedEvent += OnCraftList_ItemButtonPressed;

    _recipe_craft_res_controller = _RecipeCraftResultContent.GetComponent<ItemContainerController>();
    _recipe_craft_res_controller.OnItemButtonPressedEvent += OnCraftResult_ItemButtonPressed;

    _item_inv_grouping = _ItemInventoryListContent.GetComponent<GridLayoutGroup>();

    PlayerController _player = FindAnyObjectByType<PlayerController>();
    if(_player == null){
      Debug.LogError("Cannot find player, inventory cannot be used.");
      return;
    }

    _player_inv_data = _player.gameObject.GetComponent<InventoryData>();
    if(_player_inv_data != null){
      _player_inv_data.OnItemAddedEvent += PlayerInv_OnItemAdded;
      _player_inv_data.OnItemCountChangedEvent += PlayerInv_OnItemCountChanged;
      _player_inv_data.OnItemRemovedEvent += PlayerInv_OnItemRemoved;

      _UpdateInventory();
    }
    else{
      Debug.LogError("Player doesn't have inventory?");
      return;
    }

    _UpdateInventoryContainerSize();
  }

  /// <summary>
  /// Fungsi untuk menerima "InventoryToggle" Input.
  /// </summary>
  /// <param name="value">Value Input dari Unity</param>
  public void OnInventoryToggle(InputValue value){
    if(value.isPressed){
      // DEBUG
      _player_inv_data.AddItem("bawang_merah");
      _player_inv_data.AddItem("nasi_putih");

      _this_enable_toggle = !_this_enable_toggle;

      _SetEnable(_this_enable_toggle);
    }
  }


  /// <summary>
  /// Fungsi untuk menerima event OnItemAdded (penambahan Item) dari InventoryData pada Player.
  /// </summary>
  /// <param name="item_id">ID Item yang tertambah</param>
  /// <param name="count">Jumlah Item yang diterima</param>
  public void PlayerInv_OnItemAdded(string item_id, uint count){
    _item_inv_controller.RemoveItem(item_id);
    _item_inv_controller.AddItem(item_id, count);
    _UpdateInventoryContainerSize();
  }

  /// <summary>
  /// Fungsi untuk menerima event OnItemCountChanged (perubahan jumlah Item) dari InventoryData pada Player.
  /// </summary>
  /// <param name="item_id">ID Item yang terubah</param>
  /// <param name="new_count">Jumlah baru Item</param>
  public void PlayerInv_OnItemCountChanged(string item_id, uint new_count){
    _item_inv_controller.ChangeItemCount(item_id, new_count);
    _UpdateInventoryContainerSize();
  }

  /// <summary>
  /// Fungsi untuk menerima event OnItemRemoved (penghapusan Item dari Inventory) dari InventoryData pada Player.
  /// </summary>
  /// <param name="item_id">ID Item yang dihapus</param>
  public void PlayerInv_OnItemRemoved(string item_id){
    _item_inv_controller.RemoveItem(item_id);
    _UpdateInventoryContainerSize();
  }



  /// <summary>
  /// Fungsi untuk menerima OnButtonPressed dari ItemContainerController untuk Inventory pada Player.
  /// </summary>
  /// <param name="item_id">ID Item yang terkait</param>
  public void OnInventory_ItemButtonPressed(string item_id){
    uint _item_count = _player_inv_data.GetItemCount(item_id);
    if(_item_count <= 0){
      _item_inv_controller.RemoveItem(item_id);
      return;
    }
    
    _item_inv_controller.ChangeItemCount(item_id, _item_count-1);
    _recipe_craft_list_controller.AddItem(item_id, 1);

    _UpdateRecipeResult();
    _UpdateInventoryContainerSize();
  }

  /// <summary>
  /// Fungsi untuk menerima OnButtonPressed dari ItemContainerController untuk  Crafting Table.
  /// </summary>
  /// <param name="item_id">ID Item yang terkait</param>
  public void OnCraftList_ItemButtonPressed(string item_id){
    uint _item_count = _player_inv_data.GetItemCount(item_id);
    if(_item_count <= 0){
      _item_inv_controller.RemoveItem(item_id);
      return;
    }

    _item_inv_controller.ChangeItemCount(item_id, _item_count);
    _recipe_craft_list_controller.RemoveItem(item_id);

    _UpdateRecipeResult();
    _UpdateInventoryContainerSize();
  }

  /// <summary>
  /// Fungsi untuk menerima OnButtonPressed dari ItemContainerController untuk hasil resep.
  /// </summary>
  /// <param name="item_id">ID Item yang terkait</param>
  public void OnCraftResult_ItemButtonPressed(string item_id){
    List<string> _recipe_res = _recipe_craft_res_controller.GetListItem();
    if(_recipe_res.Count <= 0)
      return;

    List<string> _recipe_list = _recipe_craft_list_controller.GetListItem();
    bool _successful = _player_inv_data.RemoveItemList(_recipe_list);
    if(!_successful)
      return;
    
    _player_inv_data.AddItem(_recipe_res[0], 1);
  }
}