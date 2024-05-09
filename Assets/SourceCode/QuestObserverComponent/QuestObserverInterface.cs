using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;



public class QuestObserverInterface: MonoBehaviour, IQuestObserver{
  // dummy, this class is just an interface
  public event IQuestObserver.QuestUpdated QuestUpdatedEvent;
  // dummy, this class is just an interface
  public event IQuestObserver.QuestFinished QuestFinishedEvent;


  [SerializeField]
  private string _SubquestID;

  [SerializeField]
  private string _SubquestName;

  [SerializeField]
  private string _SubquestGoalMessage;


  private bool _component_initialized = false;
  public bool ComponentIntialized{
    get{
      return _component_initialized;
    }
  }


  public void Start(){
    _component_initialized = true;
  }


  public QuestObserver.QuestInfo GetQuestInfo(){
    IQuestObserver[] _quest_list = GetIQuestObservers();
    if(_quest_list.Length > 1){
      float _progress = GetQuestProgress();
      QuestObserver.QuestInfo _quest_info = new QuestObserver.QuestInfo{
        QuestObject = this,
        QuestID = _SubquestID,
        QuestName = _SubquestName,

        GoalMessage = _SubquestGoalMessage,
        Progress = _progress,

        QuestState = _progress < 1?
          QuestObserver.QuestInfo.State.NotFinished:
          QuestObserver.QuestInfo.State.Finished,
        
        UserInterfaceType = QuestObserver.QuestInfo.UIType.Nested
      };

      foreach(IQuestObserver _quest in _quest_list){
        string _subquest_id = _quest.GetQuestID();
        QuestObserver.QuestInfo _subquest_info = _quest.GetQuestInfo();

        _quest_info.SubQuestInfo.Add(_subquest_id, _subquest_info);
      }

      return _quest_info;
    }
    else if(_quest_list.Length == 1)
      return _quest_list[0].GetQuestInfo();
    else
      return new QuestObserver.QuestInfo{
        UserInterfaceType = QuestObserver.QuestInfo.UIType.Nothing
      };
  }

  public string GetQuestID(){
    return _SubquestID;
  }

  public string GetQuestName(){
    return _SubquestName;
  }


  public float GetQuestProgress(){
    IQuestObserver[] _quest_list = GetIQuestObservers();
    if(_quest_list.Length <= 0)
      return 1;

    float total_progress = 0; 
    foreach(IQuestObserver _quest in _quest_list){
      if(_quest == (IQuestObserver)this)
        continue;

      total_progress += _quest.GetQuestProgress();
    }

    return total_progress / _quest_list.Length;
  }


  public void BindObject(QuestObserver observer, QuestObserver.BindIQuestObserverCallback bind_func){
    IQuestObserver[] _quest_list = GetIQuestObservers();
    foreach(IQuestObserver _quest in _quest_list){
      if(_quest == (IQuestObserver)this)
        continue;

      _quest.BindObject(observer, bind_func);
    }
  }

  public IQuestObserver[] GetIQuestObservers(){
    IQuestObserver[] _result = new IQuestObserver[0];

    IQuestObserver[] _quest_list = GetComponents<IQuestObserver>();
    if(_quest_list.Length > 0){
      Array.Resize(ref _result, _quest_list.Length-1);
      
      int _i = 0;
      foreach(IQuestObserver _quest in _quest_list){
        if(_quest == (IQuestObserver)this)
          continue;

        _result[_i] = _quest;
        _i++;
      }
    }

    return _result;
  }
}