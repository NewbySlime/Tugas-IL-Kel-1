using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;


public class ScenarioHandler: MonoBehaviour{
  public delegate void ScenarioSubscenarioChanged(string scenario, string last_sub, string new_sub);
  public event ScenarioSubscenarioChanged ScenarioSubscenarioChangedEvent;

  public delegate void ScenarioSubscenarioFinished(string scenario, string subscenario);
  public event ScenarioSubscenarioFinished ScenarioSubscenarioFinishedEvent;


  public delegate void ScenarioFinished(string scenario);
  public event ScenarioFinished ScenarioFinishedevent;


  [Serializable]
  private struct _ScenarioData{
    public string SubScenarioID;

    public SequenceHandler StartTrigger;
    public QuestObserver Observer;
    public SequenceHandler EndTrigger;
  }
  

  [SerializeField]
  private string _ScenarioID;

  [SerializeField]
  private string _ScenarioTitle;

  [SerializeField]
  private List<_ScenarioData> _ScenarioDataList;
  
  private Dictionary<string, int> _scenario_data_ref_idx = new Dictionary<string, int>();

  private int _scenario_idx = -1;

  private bool _component_initialized = false;
  public bool ComponentIntialized{
    get{
      return _component_initialized;
    }
  }


  private void _on_scenario_data_updated(){
    for(int i = 0; i < _ScenarioDataList.Count; i++){
      _ScenarioData _data = _ScenarioDataList[i];
      
      _scenario_data_ref_idx[_data.SubScenarioID] = i;
    }
  }


  private IEnumerator __quest_finished(string subscenario){
    Debug.Log("Quest finished");
    string _last_subid = "";
    string _next_subid = "";
    Debug.Log(string.Format("current scenario {0}", _scenario_idx));

    int _last_scenario_idx = _scenario_idx;
    if(_last_scenario_idx >= 0 || _last_scenario_idx < _ScenarioDataList.Count){
      _ScenarioData _data = _ScenarioDataList[_last_scenario_idx];
      _last_subid = _data.SubScenarioID;

      if(_data.Observer != null)
        _data.Observer.gameObject.SetActive(false);

      yield return StartTriggerEnd(_data.SubScenarioID);
    }
    
    int _next_scenario_idx = _scenario_idx+1;
    if(_next_scenario_idx >= 0 || _next_scenario_idx < _ScenarioDataList.Count){
      _ScenarioData _data = _ScenarioDataList[_next_scenario_idx];
      _next_subid = _data.SubScenarioID;

      if(_data.Observer != null)
        _data.Observer.gameObject.SetActive(true);

      yield return StartTriggerStart(_data.SubScenarioID);
    }

    _scenario_idx = _next_scenario_idx;
    ScenarioSubscenarioChangedEvent?.Invoke(_ScenarioID, _last_subid, _next_subid);
    ScenarioSubscenarioFinishedEvent?.Invoke(_ScenarioID, subscenario);

    if(_next_scenario_idx >= _ScenarioDataList.Count)
      ScenarioFinishedevent?.Invoke(_ScenarioID);
  }

  private void _quest_finished(string subscenario){
    if(!_scenario_data_ref_idx.ContainsKey(subscenario))
      return;

    StartCoroutine(__quest_finished(subscenario));
  }


  private HashSet<QuestObserver> _quest_queue_wait = new HashSet<QuestObserver>();
  private IEnumerator _quest_observer_queue_initialize(int idx){
    if(idx < 0 || idx >= _ScenarioDataList.Count)
      yield break;

    QuestObserver _quest = _ScenarioDataList[idx].Observer;
    _quest_queue_wait.Add(_quest);

    _quest.gameObject.SetActive(true);
    yield return new WaitUntil(() => _quest.ComponentIntialized);
    _quest.gameObject.SetActive(false);

    _quest_queue_wait.Remove(_quest);
  }

  private IEnumerator _StartAsCoroutine(){
    Debug.Log("Scenario handler init");
    _scenario_idx = 0;

    _on_scenario_data_updated();
    for(int i = 0; i < _ScenarioDataList.Count; i++){
      _ScenarioData _scenario = _ScenarioDataList[i];

      if(_scenario.Observer != null){
        _scenario.Observer.QuestFinishedEvent += _quest_finished;
        StartCoroutine(_quest_observer_queue_initialize(i));
      }
    }

    yield return new WaitUntil(() => _quest_queue_wait.Count <= 0);
    _component_initialized = true;
  }

  public void Start(){
    StartCoroutine(_StartAsCoroutine());
  }


  public IEnumerator SwitchSubScenario(int idx, bool new_use_trigger = true, bool last_use_trigger = false){
    if(idx < 0 || idx >= _ScenarioDataList.Count())
      yield break;

    string _last_subid = "";
    if(_scenario_idx >= 0 || _scenario_idx < _ScenarioDataList.Count()){
      _ScenarioData _last_data = _ScenarioDataList[_scenario_idx];
      if(_last_data.Observer != null)
        _last_data.Observer.gameObject.SetActive(false);

      _last_subid = _last_data.SubScenarioID;
      if(last_use_trigger)
        yield return StartTriggerEnd(_last_subid);
    }

    _ScenarioData _data = _ScenarioDataList[idx];
    if(new_use_trigger)
      yield return StartTriggerStart(_data.SubScenarioID);

    _scenario_idx = idx;
    ScenarioSubscenarioChangedEvent?.Invoke(_ScenarioID, _last_subid, _data.SubScenarioID);

    if(_data.Observer != null)
      _data.Observer.gameObject.SetActive(true);
  }


  public IEnumerator StartTriggerStart(int idx){
    if(idx < 0 || idx >= _scenario_data_ref_idx.Count){
      Debug.LogError("Index is out of range.");
      yield break;
    }

    _ScenarioData _data = _ScenarioDataList[idx];
    if(_data.StartTrigger == null)
      yield break;

    yield return _data.StartTrigger.StartTrigger();
  }

  public IEnumerator StartTriggerStart(string SubScenario){
    if(!_scenario_data_ref_idx.ContainsKey(SubScenario)){
      Debug.LogError(string.Format("SubScenario ID: '{0}' cannot be found.", SubScenario));
      yield break;
    }

    yield return StartTriggerStart(_scenario_data_ref_idx[SubScenario]);
  }


  public IEnumerator StartTriggerEnd(int idx){
    if(idx < 0 || idx >= _scenario_data_ref_idx.Count){
      Debug.LogError("Index is out of range.");
      yield break;
    }

    _ScenarioData _data = _ScenarioDataList[idx];
    if(_data.EndTrigger == null)
      yield break;

    yield return _data.EndTrigger.StartTrigger();
  }

  public IEnumerator StartTriggerEnd(string SubScenario){
    if(!_scenario_data_ref_idx.ContainsKey(SubScenario)){
      Debug.LogError(string.Format("SubScenario ID: '{0}' cannot be found.", SubScenario));
      yield break;
    }

    yield return StartTriggerEnd(_scenario_data_ref_idx[SubScenario]);
  }


  public string GetCurrentSubScenario(){
    if(_scenario_idx < 0 || _scenario_idx >= _ScenarioDataList.Count)
      return "";

    return _ScenarioDataList[_scenario_idx].SubScenarioID;
  }

  #nullable enable
  public QuestObserver? GetCurrentQuest(){
    if(_scenario_idx < 0 || _scenario_idx >= _ScenarioDataList.Count)
      return null;

    return _ScenarioDataList[_scenario_idx].Observer;
  }
  #nullable disable


  #nullable enable
  public QuestObserver? GetQuest(string subscenario){
    if(!_scenario_data_ref_idx.ContainsKey(subscenario))
      return null;

    int idx = _scenario_data_ref_idx[subscenario];
    if(idx < 0 || idx >= _ScenarioDataList.Count)
      return null;

    return _ScenarioDataList[idx].Observer;
  }
  #nullable disable


  public string GetScenarioID(){
    return _ScenarioID;
  }

  public string GetScenarioTitle(){
    return _ScenarioTitle;
  }
} 