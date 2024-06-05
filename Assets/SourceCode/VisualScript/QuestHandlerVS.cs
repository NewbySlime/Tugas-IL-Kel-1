using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using Unity.VisualScripting;
using UnityEngine;


public class QuestHandlerVS: MonoBehaviour, IQuestHandler, ILoadingQueue{
  public event IQuestHandler.QuestFinished QuestFinishedEvent;
  public event IQuestHandler.QuestUpdated QuestUpdatedEvent;

  public class InitQuestInfo{
    public string QuestID;
    public object QuestData;

    public List<InitQuestInfo> SubquestList = new List<InitQuestInfo>();
  }

  public class QuestData{
    public string QuestTitle;
    public string QuestDescription;
  }

  public class QuestInfo{
    public enum UIType{
      Normal,
      PercentageBar,
      Nested
    }

    public IQuestHandler QuestObject;

    public string GoalMessage;
    public float Progress;

    public UIType UserInterfaceType;

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

  public bool IsInitialized{get; private set;} = false;

  
  private void _quest_updated(IQuestHandler quest){
    if(!_unfinished_quest.Contains(quest) && quest.GetProgress() < 1)
      _unfinished_quest.Add(quest);

    QuestUpdatedEvent?.Invoke(quest);
  }

  private void _quest_finished(IQuestHandler quest){
    Debug.Log("another quest finished.");
    if(_unfinished_quest.Contains(quest))
      _unfinished_quest.Remove(quest);

    if(_unfinished_quest.Count <= 0)
      QuestFinishedEvent?.Invoke(quest);
  }


  private void _set_quest_data(object data){
    if(data is not QuestData){
      Debug.LogError("Data is not QuestData.");
      return;
    }

    _quest_data = (QuestData)data;
  }


  private HashSet<ILoadingQueue> _quest_loading_list = new();
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


  private IEnumerator _update_quest(){
    Debug.Log(string.Format("scenario update quest {0} {1}", _quest_database == null, _init_data == null));
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
    Debug.Log("scenario quest initialized");
    IsInitialized = true;
  }


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


  public void SetInitData(InitQuestInfo init_data){
    Debug.Log("scenario set quest init data");
    Debug.Log(string.Format("scenario quest data {0}", init_data == null));

    _init_data = init_data;
    StartCoroutine(_update_quest());
  }

  public bool IsLoaded(){
    return IsInitialized;
  }
}