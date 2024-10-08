using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// Class to create and handle the list of quest with the data supplied.
/// This class doesn't handle specific quest with specific functionality, hence this class only serve as the server for the quest contained. To check which class that acts as specific quest functionality see <see cref="IQuestHandler"/> interface class.
/// 
/// This class uses autoload(s);
/// - <see cref="QuestDatabase"/> for creating quest handler with specific functionality based on the supplied ID.
/// </summary>
public class QuestHandlerVS: MonoBehaviour, IQuestHandler, ILoadingQueue{ 
  public event IQuestHandler.QuestFinished QuestFinishedEvent;
  public event IQuestHandler.QuestUpdated QuestUpdatedEvent;

  /// <summary>
  /// Quest data for initializing the quest handling classes.
  /// </summary>
  public class InitQuestInfo{
    /// <summary>
    /// The quest ID to create.
    /// </summary>
    public string QuestID;

    /// <summary>
    /// The quest data that is specific to the target quest handler.
    /// </summary>
    public object QuestData;

    /// <summary>
    /// List of subquest contained within the quest.
    /// </summary>
    /// <typeparam name="InitQuestInfo"></typeparam>
    /// <returns></returns>
    public List<InitQuestInfo> SubquestList = new List<InitQuestInfo>();
  }

  /// <summary>
  /// Quest data related to this class.
  /// </summary>
  public class QuestData{
    /// <summary>
    /// The title for this quest.
    /// </summary>
    public string QuestTitle;

    /// <summary>
    /// The description for this quest.
    /// </summary>
    public string QuestDescription;
  }


  /// <summary>
  /// Data representing the current state of the quest in question.
  /// </summary>
  public class QuestInfo{
    /// <summary>
    /// The prefered types of UI the quest should be presented with.
    /// </summary>
    public enum UIType{
      Normal,
      PercentageBar,
      Nested
    }

    /// <summary>
    /// The quest handler object.
    /// </summary>
    public IQuestHandler QuestObject;

    /// <summary>
    /// The message for telling what the goal is.
    /// </summary>
    public string GoalMessage;

    /// <summary>
    /// Current progress of the quest.
    /// </summary>
    public float Progress;

    /// <summary>
    /// The preferred UI for presenting the quest.
    /// </summary>
    public UIType UserInterfaceType;

    /// <summary>
    /// List of similar info for subquest. 
    /// </summary>
    public List<QuestInfo> SubQuestInfo = new();
  }


  private struct _quest_metadata{
    public IQuestHandler _handler;
    public IQuestData _data;

    public InitQuestInfo _init_data;
  }

  
  private HashSet<IQuestHandler> _unfinished_quest = new();
  private List<_quest_metadata> _quest_metadata_list = new();

  private QuestDatabase _quest_database;

  private QuestData _quest_data = null;
  private InitQuestInfo _init_data = null;

  /// <summary>
  /// Is the object initialized or not yet.
  /// </summary>
  public bool IsInitialized{get; private set;} = false;

  
  // if one of the quest are updated.
  private void _quest_updated(IQuestHandler quest){
    if(!_unfinished_quest.Contains(quest) && quest.GetProgress() < 1)
      _unfinished_quest.Add(quest);

    QuestUpdatedEvent?.Invoke(quest);
  }

  // if one of the quest are finished.
  private void _quest_finished(IQuestHandler quest){
    DEBUGModeUtils.Log("another quest finished.");
    if(_unfinished_quest.Contains(quest))
      _unfinished_quest.Remove(quest);

    if(_unfinished_quest.Count <= 0)
      QuestFinishedEvent?.Invoke(quest);
  }


  /// <summary>
  /// Function to set own quest data.
  /// </summary>
  /// <param name="data">The quest data</param>
  private void _set_quest_data(object data){
    if(data is not QuestData){
      Debug.LogError("Data is not QuestData.");
      return;
    }

    _quest_data = (QuestData)data;
  }


  private HashSet<ILoadingQueue> _quest_loading_list = new();
  // Function to check if the newly created queues are initialized/loaded.
  private bool _check_loading_list(){
    List<ILoadingQueue> _delete_list = new();
    foreach(ILoadingQueue _obj in _quest_loading_list){
      if(_obj.IsLoaded())
        _delete_list.Add(_obj);
    }

    foreach(ILoadingQueue _obj in _delete_list)
      _quest_loading_list.Remove(_obj);

    return _quest_loading_list.Count <= 0;
  }


  // To add/remove quest handlers based on _init_data.
  private IEnumerator _update_quest(){
    DEBUGModeUtils.Log(string.Format("scenario update quest {0} {1}", _quest_database == null, _init_data == null));
    if(_quest_database == null || _init_data == null)
      yield break;

    for(int i = 0; i < transform.childCount; i++)
      Destroy(transform.GetChild(i).gameObject);

    _set_quest_data(_init_data.QuestData);
    foreach(InitQuestInfo _subquest_info in _init_data.SubquestList){
      GameObject _quest_prefab = _quest_database.GetQuestPrefab(_subquest_info.QuestID);
      if(_quest_prefab == null){
        Debug.LogError(string.Format("Quest ID: '{0}' does not have Prefab.", _subquest_info.QuestID));
        continue;
      }

      GameObject _quest_obj = Instantiate(_quest_prefab);
      _quest_obj.transform.SetParent(transform);

      ILoadingQueue _loading_queue = _quest_obj.GetComponent<ILoadingQueue>();
      if(_loading_queue != null)
        _quest_loading_list.Add(_loading_queue);

      IQuestData _quest_data = _quest_obj.GetComponent<IQuestData>();
      IQuestHandler _quest_handler = _quest_obj.GetComponent<IQuestHandler>();
      _unfinished_quest.Add(_quest_handler);

      _quest_metadata_list.Add(new _quest_metadata{
        _handler = _quest_handler,
        _data = _quest_data,

        _init_data = _subquest_info
      });

      _quest_handler.QuestFinishedEvent += _quest_finished;
      _quest_handler.QuestUpdatedEvent += _quest_updated;
    }

    yield return null;
    yield return new WaitForEndOfFrame();

    foreach(_quest_metadata _metadata in _quest_metadata_list)
      _metadata._data.SetQuestData(_metadata._init_data.QuestData);

    yield return new WaitUntil(_check_loading_list);
    DEBUGModeUtils.Log("scenario quest initialized");
    IsInitialized = true;
  }


  // Extended Start for waiting until all objects are initiliazed for immediate use.
  private IEnumerator _StartAsCoroutine(){
    _quest_database = FindAnyObjectByType<QuestDatabase>();
    if(_quest_database == null){
      Debug.LogError("Cannot find Quest Database.");
      throw new MissingComponentException();
    }

    yield return new WaitUntil(() => _quest_database.IsInitialized);
    yield return _update_quest();
  }

  public void Start(){
    StartCoroutine(_StartAsCoroutine());
  }


  public QuestInfo GetQuestInfo(){
    QuestInfo _result = new QuestInfo{
      QuestObject = this,

      GoalMessage = GetGoalMessage(),
      Progress = GetProgress(),

      UserInterfaceType = QuestInfo.UIType.Nested
    };

    foreach(_quest_metadata _metadata in _quest_metadata_list)
      _result.SubQuestInfo.Add(_metadata._handler.GetQuestInfo());

    return _result;
  }


  public string GetGoalMessage(){
    if(_quest_data == null)
      return "";

    return _quest_data.QuestTitle;
  }

  public float GetProgress(){
    float _result = 1;
    foreach(_quest_metadata _metadata in _quest_metadata_list)
      _result *= _metadata._handler.GetProgress();

    return _result;
  }


  public string GetQuestDescription(){
    if(_quest_data == null)
      return "";

    return _quest_data.QuestDescription;
  }


  /// <summary>
  /// Function to set this <see cref="InitQuestInfo"/> data for prepping the subquests.
  /// This function can be used directly from Visual Scripting.
  /// </summary>
  /// <param name="init_data"></param>
  public void SetInitData(InitQuestInfo init_data){
    DEBUGModeUtils.Log("scenario set quest init data");
    DEBUGModeUtils.Log(string.Format("scenario quest data {0}", init_data == null));

    _init_data = init_data;
    StartCoroutine(_update_quest());
  }

  public bool IsLoaded(){
    return IsInitialized;
  }
}