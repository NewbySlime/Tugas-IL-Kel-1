using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;



public class ScenarioHandlerVS: MonoBehaviour, ILoadingQueue{
  public delegate void ScenarioSubscenarioChanged(string scenario, string last_sub, string new_sub);
  public event ScenarioSubscenarioChanged ScenarioSubscenarioChangedEvent;

  public delegate void ScenarioSubscenarioFinished(string scenario, string subscenario);
  public event ScenarioSubscenarioFinished ScenarioSubscenarioFinishedEvent;

  public delegate void ScenarioFinished(string scenario);
  public event ScenarioFinished ScenarioFinishedevent;


  [Serializable]
  public class PersistanceData: PersistanceContext.IPersistance{
    public int SubscenarioIdx;
    public string ScenarioID;


    public string GetDataID(){
      return "SubscenarioData";
    }


    public string GetData(){
      return ConvertExt.ToBase64String(JsonUtility.ToJson(this));
    }

    public void SetData(string data){
      JsonUtility.FromJsonOverwrite(ConvertExt.FromBase64String(data), this);
    }
  }


  [SerializeField]
  private GameObject _QuestHandlerPrefab;
  [SerializeField]
  private GameObject _SequenceHandlerPrefab;


  private struct _subscenario_data{
    public string _subscenario_id;

    public ScenarioDiagramVS.ScenarioData.SubData _subdata;

    public SequenceHandlerVS _start_sequence;
    public QuestHandlerVS _quest_handler;
    public SequenceHandlerVS _finish_sequence;
  }

  [SerializeField]
  private string DEBUG_ScenarioID;

  private string __scenario_id;

  private string _scenario_id{
    get{return __scenario_id;}
    set{__scenario_id = value; DEBUG_ScenarioID = value;}
  }


  private List<_subscenario_data> _subscenario_list = new();
  private Dictionary<string, int> _scenario_data_ref_idx = new();

  private int _scenario_idx = -1;

  public bool IsInitialized{get; private set;} = false;


  private void _set_enable_subscenario(int idx, bool enabled){
    _subscenario_data _data = _subscenario_list[idx];

    if(_data._quest_handler != null)
      _data._quest_handler.gameObject.SetActive(enabled);
  }


  private IEnumerator __quest_finished(string subscenario){
    DEBUGModeUtils.Log("Quest finished");
    string _last_subid = "";
    string _next_subid = "";
    DEBUGModeUtils.Log(string.Format("current scenario {0}", _scenario_idx));

    int _last_scenario_idx = _scenario_idx;
    _scenario_idx++;
    
    if(_last_scenario_idx >= 0 && _last_scenario_idx < _subscenario_list.Count){
      _subscenario_data _data = _subscenario_list[_last_scenario_idx];
      _last_subid = _data._subscenario_id;

      _set_enable_subscenario(_last_scenario_idx, false);

      yield return StartTriggerEnd(_data._subscenario_id);
    }
    
    int _next_scenario_idx = _scenario_idx;
    if(_next_scenario_idx >= 0 && _next_scenario_idx < _subscenario_list.Count){
      _subscenario_data _data = _subscenario_list[_next_scenario_idx];
      _next_subid = _data._subscenario_id;

      _set_enable_subscenario(_next_scenario_idx, true);

      yield return StartTriggerStart(_data._subscenario_id);
    }

    ScenarioSubscenarioChangedEvent?.Invoke(_scenario_id, _last_subid, _next_subid);
    ScenarioSubscenarioFinishedEvent?.Invoke(_scenario_id, subscenario);

    DEBUGModeUtils.Log(string.Format("scenario idx {0}/{1}", _next_scenario_idx, _subscenario_list.Count));
    if(_next_scenario_idx >= _subscenario_list.Count){
      DEBUGModeUtils.Log("scenario finished");
      ScenarioFinishedevent?.Invoke(_scenario_id);
    }
  }

  private void _quest_finished(IQuestHandler handler){
    if(_scenario_idx < 0 || _scenario_idx >= _subscenario_list.Count)
      return;

    _subscenario_data _data = _subscenario_list[_scenario_idx];
    if(_data._quest_handler.GetProgress() < 1)
      return;

    StartCoroutine(__quest_finished(_data._subscenario_id));
  }


  private HashSet<ILoadingQueue> _init_queue_list = new();
  private bool _check_queue_list(){
    List<ILoadingQueue> _list_delete = new();
    foreach(ILoadingQueue _obj in _init_queue_list){
      if(_obj.IsLoaded())
        _list_delete.Add(_obj);
    }

    foreach(ILoadingQueue _obj in _list_delete)
      _init_queue_list.Remove(_obj);

    return _init_queue_list.Count <= 0;
  }


  private IEnumerator _set_init_data(ScenarioDiagramVS.ScenarioData init_data){
    _scenario_id = init_data.ScenarioID;
    
    _subscenario_list.Clear();
    _scenario_data_ref_idx.Clear();

    foreach(ScenarioDiagramVS.ScenarioData.SubData _subdata in init_data.SubscenarioList){
      SequenceHandlerVS _seq_start = null;
      SequenceHandlerVS _seq_finish = null;
      QuestHandlerVS _quest_handler = null;

      if(_subdata.SequenceStartData != null){
        GameObject _start_seq_obj = Instantiate(_SequenceHandlerPrefab);
        _start_seq_obj.transform.SetParent(transform);
        
        _seq_start = _start_seq_obj.GetComponent<SequenceHandlerVS>();
      }

      if(_subdata.SequenceFinishData != null){
        GameObject _finish_seq_obj = Instantiate(_SequenceHandlerPrefab);
        _finish_seq_obj.transform.SetParent(transform);

        _seq_finish = _finish_seq_obj.GetComponent<SequenceHandlerVS>();
      }

      if(_subdata.QuestData != null){
        GameObject _quest_obj = Instantiate(_QuestHandlerPrefab);
        _quest_obj.transform.SetParent(transform);

        _quest_handler = _quest_obj.GetComponent<QuestHandlerVS>();
        _quest_handler.QuestFinishedEvent += _quest_finished;
      }

      _scenario_data_ref_idx[_subdata.SubID] = _subscenario_list.Count;
      _subscenario_list.Add(new _subscenario_data{
        _subscenario_id = _subdata.SubID,

        _start_sequence = _seq_start,
        _finish_sequence = _seq_finish,

        _quest_handler = _quest_handler,
        _subdata = _subdata
      });
    }

    // tunggu sampai Start() selanjutnya
    yield return null;
    yield return new WaitForEndOfFrame();

    // set data
    foreach(_subscenario_data _data in _subscenario_list){
      if(_data._start_sequence != null){
        _data._start_sequence.SetInitData(_data._subdata.SequenceStartData);
      }

      if(_data._finish_sequence != null){
        _data._finish_sequence.SetInitData(_data._subdata.SequenceFinishData);
      }

      if(_data._quest_handler != null){
        _data._quest_handler.SetInitData(_data._subdata.QuestData);
        _init_queue_list.Add(_data._quest_handler);
      }
    }

    DEBUGModeUtils.Log("scenario handler check queue");
    yield return new WaitUntil(_check_queue_list);
    DEBUGModeUtils.Log("scenario handler disable");

    // disable
    for(int i = 0; i < _subscenario_list.Count; i++)
      _set_enable_subscenario(i, false);

    IsInitialized = true;
  }


  public void Start(){
    // cek prefab
    GameObject _seq_obj = Instantiate(_SequenceHandlerPrefab);
    if(_seq_obj.GetComponent<SequenceHandlerVS>() == null){
      Debug.LogError("No Prefab for Sequence Handler.");
      throw new MissingFieldException();
    }

    Destroy(_seq_obj);


    GameObject _quest_obj = Instantiate(_QuestHandlerPrefab);
    if(_quest_obj.GetComponent<QuestHandlerVS>() == null){
      Debug.LogError("No Prefab for Quest Handler.");
      throw new MissingFieldException();
    }

    Destroy(_quest_obj);
  }


  public IEnumerator SwitchSubScenario(int idx, bool new_use_trigger = true, bool last_use_trigger = false){
    DEBUGModeUtils.Log(string.Format("set scenario {0} idx {1}", _scenario_id, idx));
    if(idx < 0 || idx >= _subscenario_list.Count())
      yield break;

    string _last_subid = "";
    if(_scenario_idx >= 0 && _scenario_idx < _subscenario_list.Count()){
      _subscenario_data _last_data = _subscenario_list[_scenario_idx];
      _set_enable_subscenario(_scenario_idx, false);

      _last_subid = _last_data._subscenario_id;
      if(last_use_trigger)
        yield return StartTriggerEnd(_last_subid);
    }

    _subscenario_data _data = _subscenario_list[idx];
    if(new_use_trigger)
      yield return StartTriggerStart(_data._subscenario_id);

    _scenario_idx = idx;
    ScenarioSubscenarioChangedEvent?.Invoke(_scenario_id, _last_subid, _data._subscenario_id);

    _set_enable_subscenario(idx, true);
    DEBUGModeUtils.Log(string.Format("setted scenario {0} idx {1}", _scenario_id, GetCurrentSubScenario()));
  }


  public IEnumerator StartTriggerStart(int idx){
    if(idx < 0 || idx >= _scenario_data_ref_idx.Count){
      Debug.LogError("Index is out of range.");
      yield break;
    }

    _subscenario_data _data = _subscenario_list[idx];
    DEBUGModeUtils.Log(string.Format("start sequence exist {0}", _data._start_sequence == null));
    if(_data._start_sequence == null)
      yield break;

    yield return _data._start_sequence.StartTrigger();
  }

  public IEnumerator StartTriggerStart(string SubScenario){
    DEBUGModeUtils.Log(string.Format("Starting trigger ID:{0}", SubScenario));
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

    _subscenario_data _data = _subscenario_list[idx];
    if(_data._finish_sequence == null)
      yield break;

    yield return _data._finish_sequence.StartTrigger();
  }

  public IEnumerator StartTriggerEnd(string SubScenario){
    if(!_scenario_data_ref_idx.ContainsKey(SubScenario)){
      Debug.LogError(string.Format("SubScenario ID: '{0}' cannot be found.", SubScenario));
      yield break;
    }

    yield return StartTriggerEnd(_scenario_data_ref_idx[SubScenario]);
  }


  public string GetCurrentSubScenario(){
    if(_scenario_idx < 0 || _scenario_idx >= _subscenario_list.Count)
      return "";

    return _subscenario_list[_scenario_idx]._subscenario_id;
  }

  #nullable enable
  public QuestHandlerVS? GetCurrentQuest(){
    if(_scenario_idx < 0 || _scenario_idx >= _subscenario_list.Count)
      return null;

    return _subscenario_list[_scenario_idx]._quest_handler;
  }
  #nullable disable


  #nullable enable
  public QuestHandlerVS? GetQuest(string subscenario){
    if(!_scenario_data_ref_idx.ContainsKey(subscenario))
      return null;

    int idx = _scenario_data_ref_idx[subscenario];
    if(idx < 0 || idx >= _subscenario_list.Count)
      return null;

    return _subscenario_list[idx]._quest_handler;
  }
  #nullable disable


  public string GetScenarioID(){
    return _scenario_id;
  }

  public bool IsScenarioFinished(){
    return _scenario_idx >= _subscenario_list.Count;
  }


  public void SkipToNextSubScenario(){
    if(_scenario_idx >= _subscenario_list.Count)
      return;

    string _current_subscenario_id = _subscenario_list[_scenario_idx]._subscenario_id;
    StartCoroutine(__quest_finished(_current_subscenario_id));
  }


  public void SetInitData(ScenarioDiagramVS.ScenarioData init_data){
    StartCoroutine(_set_init_data(init_data));
  }


  public bool IsLoaded(){
    return IsInitialized;
  }


  public PersistanceData GetPersistanceData(){
    return new PersistanceData{
      ScenarioID = _scenario_id,
      SubscenarioIdx = _scenario_idx
    };
  }

  public IEnumerator SetPersistanceData(PersistanceData data){
    yield return SwitchSubScenario(data.SubscenarioIdx, false, false);
  }
}