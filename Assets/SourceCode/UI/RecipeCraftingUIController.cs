using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;



/// <summary>
/// NOTE: unused for now.
/// 
/// UI Class for handling crafting interaction.
/// NOTE: this uses wrapper object that contains all the UI elements so this class would stay enabled.
/// 
/// This class uses external component(s);
/// - <see cref="ItemContainerController"/> UI controllers for handle item buttons inside the controllers
/// - <see cref="InventoryData"/> component from <see cref="PlayerController"/>.
/// 
/// This class uses autoload(s);
/// - <see cref="ItemRecipeDatabase"/> for getting data of items.
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
  /// To enable or disable the wrapper object (which hide all the UI elements except this class).
  /// </summary>
  /// <param name="_enable">Enable or disable the object</param>
  private void _SetEnable(bool _enable){
    _WrapperContent.SetActive(_enable);
  } 


  // to trigger updating the UI
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


  // to update the container size based on how many the objects are in the container.
  // this function is for trigger updating the scroll class from Unity.
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
  /// To update and check the current item combination for resulting item of certain valid recipe.
  /// </summary>
  private void _UpdateRecipeResult(){
    _recipe_craft_res_controller.RemoveAllItem();

    List<string> _item_list = _recipe_craft_list_controller.GetListItem();
    string _result_id = _item_recipe_database.RecipeParse(_item_list);
    DEBUGModeUtils.Log(string.Format("hasil masakan: {0}", _result_id));

    if(_result_id.Length > 0)
      _recipe_craft_res_controller.AddItem(_result_id, 1);
  }


  private void _PlayerInv_OnItemAdded(string item_id, uint count){
    _item_inv_controller.RemoveItem(item_id);
    _item_inv_controller.AddItem(item_id, count);
    _UpdateInventoryContainerSize();
  }

  private void _PlayerInv_OnItemCountChanged(string item_id, uint new_count){
    _item_inv_controller.ChangeItemCount(item_id, new_count);
    _UpdateInventoryContainerSize();
  }

  private void _PlayerInv_OnItemRemoved(string item_id){
    _item_inv_controller.RemoveItem(item_id);
    _UpdateInventoryContainerSize();
  }


  private void _OnInventory_ItemButtonPressed(string item_id){
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

  private void _OnCraftList_ItemButtonPressed(string item_id){
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

  private void _OnCraftResult_ItemButtonPressed(string item_id){
    List<string> _recipe_res = _recipe_craft_res_controller.GetListItem();
    if(_recipe_res.Count <= 0)
      return;

    List<string> _recipe_list = _recipe_craft_list_controller.GetListItem();
    bool _successful = _player_inv_data.RemoveItemList(_recipe_list);
    if(!_successful)
      return;
    
    _player_inv_data.AddItem(_recipe_res[0], 1);

    _recipe_craft_list_controller.RemoveAllItem();
    _recipe_craft_res_controller.RemoveAllItem();
  }


  private void _scene_changed_finished(string scene_id, GameHandler.GameContext context){
    _this_enable_toggle = false;
    _SetEnable(false);

    PlayerController _player = FindAnyObjectByType<PlayerController>();
    if(_player == null){
      Debug.LogError("Cannot find player, inventory cannot be used.");
      return;
    }

    _player_inv_data = _player.gameObject.GetComponent<InventoryData>();
    if(_player_inv_data != null){
      _player_inv_data.OnItemAddedEvent += _PlayerInv_OnItemAdded;
      _player_inv_data.OnItemCountChangedEvent += _PlayerInv_OnItemCountChanged;
      _player_inv_data.OnItemRemovedEvent += _PlayerInv_OnItemRemoved;

      _UpdateInventory();
    }
    else{
      Debug.LogError("Player doesn't have inventory?");
      return;
    }

    _UpdateInventoryContainerSize();
  }


  public void Start(){
    GameHandler _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find Game Handler.");
      throw new UnityEngine.MissingComponentException();
    }

    _game_handler.SceneChangedFinishedEvent += _scene_changed_finished;

    ItemDatabase _item_database = FindAnyObjectByType<ItemDatabase>();
    if(_item_database == null){
      Debug.LogError("Cannot find Item Database.");
      throw new UnityEngine.MissingComponentException();
    }

    _item_recipe_database = _item_database.gameObject.GetComponent<ItemRecipeDatabase>();
    if(_item_recipe_database == null){
      Debug.LogError("Database for Items doesn't have database Recipes.");
      throw new UnityEngine.MissingComponentException();
    }

    _item_inv_controller = _ItemInventoryListContent.GetComponent<ItemContainerController>();
    _item_inv_controller.OnItemButtonPressedEvent += _OnInventory_ItemButtonPressed;

    _recipe_craft_list_controller = _RecipeCraftListContent.GetComponent<ItemContainerController>();
    _recipe_craft_list_controller.OnItemButtonPressedEvent += _OnCraftList_ItemButtonPressed;

    _recipe_craft_res_controller = _RecipeCraftResultContent.GetComponent<ItemContainerController>();
    _recipe_craft_res_controller.OnItemButtonPressedEvent += _OnCraftResult_ItemButtonPressed;

    _item_inv_grouping = _ItemInventoryListContent.GetComponent<GridLayoutGroup>();


    if(_game_handler.SceneInitialized)
      _scene_changed_finished(_game_handler.GetCurrentSceneID(), _game_handler.GetCurrentSceneContext());
  }

  /// <summary>
  /// To catch "InventoryToggle" input event. This function to toggle the enable/show flag.
  /// </summary>
  /// <param name="value">Unity's input data</param>
  public void OnInventoryToggle(InputValue value){
    if(value.isPressed){
      _this_enable_toggle = !_this_enable_toggle;

      _SetEnable(_this_enable_toggle);
    }
  }
}