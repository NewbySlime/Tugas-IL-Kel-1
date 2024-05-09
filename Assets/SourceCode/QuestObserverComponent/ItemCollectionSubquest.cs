using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public class ItemCollectionSubquest: MonoBehaviour, IQuestObserver{
  public event IQuestObserver.QuestUpdated QuestUpdatedEvent;

  public event IQuestObserver.QuestFinished QuestFinishedEvent;


  [Serializable]
  private struct _ItemMetadata{
    public string ItemID;
    public uint ItemCount;
  }

  private struct _ProgessData{
    public uint CurrentCount;
    public uint TargetCount;
  }


  [SerializeField]
  private string _QuestID;

  [SerializeField]
  private _ItemMetadata _ItemCheck;


  private ItemDatabase _item_database;
  private InventoryData _player_inventory;

  private ItemMetadata.ItemData _item_metadata;

  private _ProgessData _progression_data = new _ProgessData();


  private bool _quest_initialization_failed = true;



  private void _scene_changed(string scene_id, GameHandler.GameContext context){
    _player_inventory = null;
    if(context != GameHandler.GameContext.InGame)
      return;

    PlayerController _player = FindAnyObjectByType<PlayerController>();
    if(_player == null){
      Debug.LogError("Cannot find Player Object.");
      return;
    }

    _player_inventory = _player.gameObject.GetComponent<InventoryData>();
    if(_player_inventory == null){
      Debug.LogError("Player doesn't have inventory?");
      return;
    }

    _player_inventory.OnItemAddedEvent += _player_inventory_on_item_added;
    _player_inventory.OnItemCountChangedEvent += _player_inventory_on_item_count_changed;
    _player_inventory.OnItemRemovedEvent += _player_inventory_on_item_removed;

    _check_player_inventory();
    Debug.Log("Scene Changed");
  }


  private void _check_player_inventory(){
    Debug.Log(string.Format("is Active = {0}", gameObject.activeInHierarchy));
    if(!gameObject.activeInHierarchy)
      return;

    if(_player_inventory == null)
      return;
      
    uint _item_count = _player_inventory.GetItemCount(_ItemCheck.ItemID);

    Debug.Log(string.Format("{0} {1}/{2}", _ItemCheck.ItemID, _item_count, _ItemCheck.ItemCount));
    QuestUpdatedEvent?.Invoke(_QuestID);

    _progression_data.CurrentCount = _item_count;
    if(_item_count >= _progression_data.TargetCount)
      QuestFinishedEvent?.Invoke(_QuestID);
  }
  

  private void _player_inventory_on_item_added(string item_id, uint count){
    Debug.Log(string.Format("Item added {0}", item_id));
    if(item_id != _ItemCheck.ItemID)
      return;

    _check_player_inventory();
  } 

  private void _player_inventory_on_item_count_changed(string item_id, uint count){
    if(item_id != _ItemCheck.ItemID)
      return;

    _check_player_inventory();
  }

  private void _player_inventory_on_item_removed(string item_id){
    if(item_id != _ItemCheck.ItemID)
      return;

    _check_player_inventory();
  }


  #nullable enable
  private void _item_database_on_initialized(){
    TypeDataStorage? _item_data =_item_database.GetItemData(_ItemCheck.ItemID);
    if(_item_data != null){
      _item_metadata = _item_data.GetData<ItemMetadata.ItemData>();
    }
    else{
      Debug.LogWarning(string.Format("Item (ID: '{0}') not found, quest (ID: '{1}') will be ignored.", _ItemCheck.ItemID, _QuestID));
      return;
    }
  }
  #nullable disable


  public void Start(){
    _item_database = FindAnyObjectByType<ItemDatabase>();
    if(_item_database == null){
      Debug.LogError("Cannot find Item Database.");
      throw new UnityEngine.MissingComponentException();
    }

    _item_database.OnInitializedEvent += _item_database_on_initialized;

    GameHandler _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Game Handler cannot be found?");
      throw new UnityEngine.MissingComponentException();
    }

    _game_handler.SceneChangedFinishedEvent += _scene_changed;

    _progression_data.TargetCount = _ItemCheck.ItemCount;
    _quest_initialization_failed = false;
    Debug.Log("ItemCollectionSubquest initialized.");
  }


  public QuestObserver.QuestInfo GetQuestInfo(){
    float _progress = GetQuestProgress();

    string _item_name = _item_metadata.Name; 
    string _goal_message = string.Format("Ambil <b>{0}</b> {1} ", _item_name, _progression_data.TargetCount);
    return new QuestObserver.QuestInfo{
      QuestObject = this,
      QuestID = GetQuestID(),
      QuestName = GetQuestName(),

      GoalMessage = _goal_message,
      Progress = _progress,

      QuestState = _progress < 1?
        QuestObserver.QuestInfo.State.NotFinished:
        QuestObserver.QuestInfo.State.Finished,

      UserInterfaceType = QuestObserver.QuestInfo.UIType.PercentageBar
    };
  }

  public string GetQuestID(){
    return _QuestID;
  }

  public string GetQuestName(){
    return "";
  }


  public float GetQuestProgress(){
    return (float)_progression_data.CurrentCount/_progression_data.TargetCount;
  }


  public void BindObject(QuestObserver observer, QuestObserver.BindIQuestObserverCallback callback){
    if(!_quest_initialization_failed)
      callback(this);
  }
}