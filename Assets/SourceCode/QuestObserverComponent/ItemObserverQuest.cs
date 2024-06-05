using UnityEngine;
using Unity.VisualScripting;
using System.Reflection;


public class ItemObserverQuest: MonoBehaviour, IQuestData, IQuestHandler{
  public const string QuestID = "item_observer";

  public event IQuestHandler.QuestUpdated QuestUpdatedEvent;
  public event IQuestHandler.QuestFinished QuestFinishedEvent;

  public struct QuestData{
    public string ItemID;
    public uint ItemCount;

    public string GoalMessage;
  }

  private string _item_id;
  private uint _item_count;

  private string _quest_message;

  private uint _current_count = 0;

  private ItemDatabase _item_database;
  private InventoryData _inv_data = null;


  private void _check_inventory(){
    if(_inv_data == null || _item_id.Length <= 0)
      return;

    uint _new_count = _inv_data.GetItemCount(_item_id);

    bool _is_finished = _current_count < _item_count && _new_count >= _item_count;
    _current_count = _new_count;

    Debug.Log(string.Format("item observer get item {0}/{1}", _new_count, _item_count));

    QuestUpdatedEvent?.Invoke(this);

    if(_is_finished)
      QuestFinishedEvent?.Invoke(this);
  }


  private void _item_added_event(string item_id, uint count){
    if(item_id != _item_id)
      return;

    _check_inventory();
  }

  private void _item_count_changed_event(string item_id, uint count){
    if(item_id != _item_id)
      return;
      
    _check_inventory();
  }

  private void _item_removed_event(string item_id){
    if(item_id != _item_id)
      return;
      
    _check_inventory();
  }


  private void _on_scene_changed(string scene_id, GameHandler.GameContext context){
    PlayerController _player = FindAnyObjectByType<PlayerController>();
    if(_player == null)
      return;

    _inv_data = FindAnyObjectByType<InventoryData>();
    if(_inv_data != null){
      _inv_data.OnItemAddedEvent += _item_added_event;
      _inv_data.OnItemCountChangedEvent += _item_count_changed_event;
      _inv_data.OnItemRemovedEvent += _item_removed_event;
    }

    _check_inventory();
  }


  public void Start(){
    GameHandler _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot get Game Handler.");
      throw new MissingComponentException();
    }

    _game_handler.SceneChangedFinishedEvent += _on_scene_changed;

    _item_database = FindAnyObjectByType<ItemDatabase>();
    if(_item_database == null){
      Debug.LogError("Cannot get database for items.");
      throw new MissingComponentException();
    }

    if(_game_handler.SceneInitialized)
      _on_scene_changed(_game_handler.GetCurrentSceneID(), _game_handler.GetCurrentSceneContext());
  }


  public QuestHandlerVS.QuestInfo GetQuestInfo(){
    return new QuestHandlerVS.QuestInfo{
      QuestObject = this,

      GoalMessage = GetGoalMessage(),
      Progress = GetProgress(),
      UserInterfaceType = QuestHandlerVS.QuestInfo.UIType.Normal
    };
  }

  public string GetGoalMessage(){
    return string.Format(_quest_message, _current_count, _item_count);
  }

  public float GetProgress(){
    return (float)_current_count/_item_count;
  }


  public string GetQuestID(){
    return QuestID;
  }

  public void SetQuestData(object data){
    Debug.Log(string.Format("item database is null {0}", _item_database == null));
    if(data is not QuestData){
      Debug.LogError(string.Format("Data is not '{0}'", typeof(QuestData).Name));
      return;
    }

    QuestData _quest_data = (QuestData)data;
    _item_id = _quest_data.ItemID;
    _item_count = _quest_data.ItemCount;
    _quest_message = _quest_data.GoalMessage;

    _current_count = 0;

    TypeDataStorage _item_data = _item_database.GetItemData(_item_id);
    if(_item_data == null)
      Debug.LogWarning(string.Format("Item ID is invalid. (ID: '{0}')", _item_id));

    _check_inventory();
  }
}



[UnitCategory("Quest")]
public class ItemObserverQuestVS: AddQuest{
  [DoNotSerialize]
  private ValueInput _item_id_input;
  [DoNotSerialize]
  private ValueInput _item_count_input;

  [DoNotSerialize]
  private ValueInput _goal_message_input;

  protected override void Definition(){
    base.Definition();

    _item_id_input = ValueInput<string>("ItemID");
    _item_count_input = ValueInput<uint>("ItemCount"); 

    _goal_message_input = ValueInput<string>("GoalMessage");
  }

  protected override void AddData(Flow flow, out QuestHandlerVS.InitQuestInfo init_data){
    init_data = new QuestHandlerVS.InitQuestInfo{
      QuestID = ItemObserverQuest.QuestID,
      QuestData = new ItemObserverQuest.QuestData{
        ItemID = flow.GetValue<string>(_item_id_input),
        ItemCount = flow.GetValue<uint>(_item_count_input),
        
        GoalMessage = flow.GetValue<string>(_goal_message_input)
      }
    };
  }
}