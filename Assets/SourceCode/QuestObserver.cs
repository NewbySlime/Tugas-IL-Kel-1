using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public class QuestObserver: MonoBehaviour{
  public delegate void SubQuestUpdated(string quest_id, string subquest_id);
  public event SubQuestUpdated SubQuestUpdatedEvent;

  public delegate void SubQuestFinished(string quest_id, string subquest_id);
  public event SubQuestFinished SubQuestFinishedEvent;

  public delegate void QuestFinished(string quest_id);
  public event QuestFinished QuestFinishedEvent;

  public delegate void BindIQuestObserverCallback(IQuestObserver subquest);


  public class QuestInfo{
    public enum State{
      NotFinished,
      Finished
    }

    public enum UIType{
      Main,
      PercentageBar,
      Nested,
      Nothing // this will pass
    }

    public IQuestObserver QuestObject;
    public string QuestID;
    public string QuestName;

    public string GoalMessage;
    public float Progress;

    public State QuestState;
    public UIType UserInterfaceType;

    public Dictionary<string, QuestInfo> SubQuestInfo = new Dictionary<string, QuestInfo>();
  }


  [SerializeField]
  private string _QuestID;

  [SerializeField]
  private string _QuestName;


  [SerializeField]
  private List<QuestObserverInterface> _QuestObserverList;


  private bool _component_initialized = false;
  public bool ComponentIntialized{
    get{
      return _component_initialized;
    }
  }
  
  private HashSet<IQuestObserver> _UnfinishedQuestMap = new HashSet<IQuestObserver>();
  private Dictionary<string, IQuestObserver> _QuestObserverDictionary = new Dictionary<string, IQuestObserver>();



  private void _on_quest_updated(string subquest_id){
    SubQuestUpdatedEvent?.Invoke(_QuestID, subquest_id);

    IQuestObserver _subquest = _QuestObserverDictionary[subquest_id];
    if(!_UnfinishedQuestMap.Contains(_subquest))
      _UnfinishedQuestMap.Add(_subquest);
  }

  private void _on_quest_finished(string subquest_id){
    if(!_QuestObserverDictionary.ContainsKey(subquest_id))
      return;
    
    SubQuestFinishedEvent?.Invoke(_QuestID, subquest_id);

    IQuestObserver _subquest = _QuestObserverDictionary[subquest_id];
    if(_UnfinishedQuestMap.Contains(_subquest))
      _UnfinishedQuestMap.Remove(_subquest);

    if(_UnfinishedQuestMap.Count <= 0)
      QuestFinishedEvent?.Invoke(_QuestID);
  }


  private void _BindSubQuest(IQuestObserver subquest){
    string _subquest_id = subquest.GetQuestID();
    if(_QuestObserverDictionary.ContainsKey(_subquest_id)){
      Debug.LogWarning(string.Format("Dupe (QuestID: '{0}') found, the dupe will not be accounted.", _subquest_id));
      return;
    }

    subquest.QuestUpdatedEvent += _on_quest_updated;
    subquest.QuestFinishedEvent += _on_quest_finished;

    _QuestObserverDictionary[_subquest_id] = subquest;
  }


  private HashSet<QuestObserverInterface> _quest_interface_queue_list = new HashSet<QuestObserverInterface>();
  private IEnumerator _quest_interface_queue_initialize(QuestObserverInterface qinterface){
    _quest_interface_queue_list.Add(qinterface);

    yield return new WaitUntil(() => qinterface.ComponentIntialized);
    qinterface.BindObject(this, _BindSubQuest);

    _quest_interface_queue_list.Remove(qinterface);
  }

  private IEnumerator _StartAsCoroutine(){
    foreach(QuestObserverInterface _interface in _QuestObserverList)
      StartCoroutine(_quest_interface_queue_initialize(_interface));

    yield return new WaitUntil(() => _quest_interface_queue_list.Count <= 0);
    _component_initialized = true;
  }

  public void Start(){
    StartCoroutine(_StartAsCoroutine());
  }


  public QuestInfo GetQuestInfo(){
    QuestInfo _result = new QuestInfo{
      QuestObject = null,
      QuestID = _QuestID,
      QuestName = _QuestName,

      GoalMessage = "",
      Progress = GetQuestProgress(),

      QuestState = _UnfinishedQuestMap.Count <= 0? QuestInfo.State.Finished: QuestInfo.State.NotFinished,
      UserInterfaceType = QuestInfo.UIType.Main
    };

    foreach(QuestObserverInterface _interface in _QuestObserverList){
      QuestInfo _subquest_info = _interface.GetQuestInfo();
      _result.SubQuestInfo.Add(_interface.GetQuestID(), _subquest_info);
    }

    return _result;
  }

  #nullable enable
  public IQuestObserver? GetSubQuest(string subquest_id){
    if(!_QuestObserverDictionary.ContainsKey(subquest_id))
      return null;

    return _QuestObserverDictionary[subquest_id];
  }
  #nullable disable


  public string GetQuestID(){
    return _QuestID;
  }

  public string GetQuestName(){
    return _QuestName;
  }

  public float GetQuestProgress(){
    if(_QuestObserverDictionary.Count <= 0)
      return 1;

    float _total_progress = 0;
    foreach(IQuestObserver _quest in _QuestObserverDictionary.Values)
      _total_progress += _quest.GetQuestProgress();

    return _total_progress / _QuestObserverDictionary.Count;
  }
}