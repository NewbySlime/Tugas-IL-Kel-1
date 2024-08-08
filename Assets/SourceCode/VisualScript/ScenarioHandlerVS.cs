using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;



/// <summary>
/// Class for handling a list of subscenario contained in this class. For a class that handles sets of scenarios, see <see cref="ScenarioDiagramVS"/>.
/// For further explanation on how scenario works in the Game, read the diagram contained in <b>Reference/Diagrams/Scenario.drawio</b>
/// 
/// This class uses prefab(s);
/// - Prefab that has <see cref="QuestHandlerVS"/> for quest handling in a scenario.
/// - Prefab that has <see cref="SequenceHandlerVS"/> for sequence handling in a scenario.
/// </summary>
public class ScenarioHandlerVS: MonoBehaviour, ILoadingQueue{
  /// <summary>
  /// Event for when scenario switch its subscenario.
  /// </summary>
  public event ScenarioSubscenarioChanged ScenarioSubscenarioChangedEvent;
  public delegate void ScenarioSubscenarioChanged(string scenario, string last_sub, string new_sub);

  /// <summary>
  /// Event when subscenario has been finished/completed.
  /// </summary>
  public event ScenarioSubscenarioFinished ScenarioSubscenarioFinishedEvent;
  public delegate void ScenarioSubscenarioFinished(string scenario, string subscenario);

  /// <summary>
  /// Event when a scenario (whole subscenario) has been finished/completed.
  /// </summary>
  public event ScenarioFinished ScenarioFinishedevent;
  public delegate void ScenarioFinished(string scenario);


  [Serializable]
  /// <summary>
  /// Data for persistance use.
  /// This data contains about the state of this scneario.
  /// </summary>
  public class PersistanceData: PersistanceContext.IPersistance{
    /// <summary>
    /// Index of current subscenario.
    /// </summary>
    public int SubscenarioIdx;

    /// <summary>
    /// This scenario ID.
    /// </summary>
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


  // Data for certain subscenario.
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

  /// <summary>
  /// Flag for checking if this object ready or not yet.
  /// </summary>
  public bool IsInitialized{get; private set;} = false;


  private void _set_enable_subscenario(int idx, bool enabled){
    _subscenario_data _data = _subscenario_list[idx];

    if(_data._quest_handler != null)
      _data._quest_handler.gameObject.SetActive(enabled);
  }


  /// <summary>
  /// Extension from <see cref="_quest_finished"/> function but using coroutine to help trigger sequencing and other stuff that requires yielding.
  /// This function also handles switching to new subscenario.
  /// </summary>
  /// <param name="subscenario">Finished subscenario ID</param>
  /// <returns>Coroutine helper object</returns>
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

  /// <summary>
  /// Function to catch event when a subscenario's quest has been completed.
  /// This function only catches the event which then redirects to <see cref="__quest_finished"/>.
  /// </summary>
  /// <param name="handler">The quest handler object</param>
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


  /// <summary>
  /// This function is an extension from <see cref="SetInitData"/> which to prep this scenario and its subscenario with using coroutine for yeilding funcitons.
  /// </summary>
  /// <param name="init_data">This scenario data</param>
  /// <returns>Coroutine helper object</returns>
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


  /// <summary>
  /// Switch subscenario from previous subscenario to new subscenario. Does not have to switch within order.
  /// Also handles sequence triggering, hence the use of coroutine.
  /// </summary>
  /// <param name="idx">The index of the new subscenario</param>
  /// <param name="new_use_trigger">Should the new subscenario's sequence be triggered</param>
  /// <param name="last_use_trigger">Should the previous subscenario's sequence be triggered</param>
  /// <returns></returns>
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


  /// <summary>
  /// Coroutine function to trigger sequence using "starting" subscenario state sequence.
  /// </summary>
  /// <param name="idx">The target subscenario index</param>
  /// <returns>Coroutine helper object</returns>
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

  /// <summary>
  /// <inheritdoc cref="StartTriggerStart"/>
  /// </summary>
  /// <param name="SubScenario">The target subscenario ID</param>
  /// <returns>Coroutine helper object</returns>
  public IEnumerator StartTriggerStart(string SubScenario){
    DEBUGModeUtils.Log(string.Format("Starting trigger ID:{0}", SubScenario));
    if(!_scenario_data_ref_idx.ContainsKey(SubScenario)){
      Debug.LogError(string.Format("SubScenario ID: '{0}' cannot be found.", SubScenario));
      yield break;
    }

    yield return StartTriggerStart(_scenario_data_ref_idx[SubScenario]);
  }


  /// <summary>
  /// Coroutine function to trigger sequence using "ending" subscenario state sequence.
  /// </summary>
  /// <param name="idx">The target subscenario index</param>
  /// <returns>Coroutine helper object</returns>
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

  /// <summary>
  /// <inheritdoc cref="StartTriggerEnd"/>
  /// </summary>
  /// <param name="SubScenario">The target subscenario ID</param>
  /// <returns>Coroutine helper object</returns>
  public IEnumerator StartTriggerEnd(string SubScenario){
    if(!_scenario_data_ref_idx.ContainsKey(SubScenario)){
      Debug.LogError(string.Format("SubScenario ID: '{0}' cannot be found.", SubScenario));
      yield break;
    }

    yield return StartTriggerEnd(_scenario_data_ref_idx[SubScenario]);
  }


  /// <summary>
  /// Get currently active subscenario.
  /// </summary>
  /// <returns>Active subscenario ID</returns>
  public string GetCurrentSubScenario(){
    if(_scenario_idx < 0 || _scenario_idx >= _subscenario_list.Count)
      return "";

    return _subscenario_list[_scenario_idx]._subscenario_id;
  }

  #nullable enable
  /// <summary>
  /// Get quest from active subscenario.
  /// </summary>
  /// <returns>The quest handler of active subscenario</returns>
  public QuestHandlerVS? GetCurrentQuest(){
    if(_scenario_idx < 0 || _scenario_idx >= _subscenario_list.Count)
      return null;

    return _subscenario_list[_scenario_idx]._quest_handler;
  }
  #nullable disable


  #nullable enable
  /// <summary>
  /// Get quest from a subscenario.
  /// </summary>
  /// <param name="subscenario">Target subscenario ID</param>
  /// <returns>The quest handler from a subscenario</returns>
  public QuestHandlerVS? GetQuest(string subscenario){
    if(!_scenario_data_ref_idx.ContainsKey(subscenario))
      return null;

    int idx = _scenario_data_ref_idx[subscenario];
    if(idx < 0 || idx >= _subscenario_list.Count)
      return null;

    return _subscenario_list[idx]._quest_handler;
  }
  #nullable disable


  /// <summary>
  /// Get this scenario ID.
  /// </summary>
  /// <returns>The ID</returns>
  public string GetScenarioID(){
    return _scenario_id;
  }

  /// <summary>
  /// Check if this scenario has been completed.
  /// </summary>
  /// <returns>Is this scenario finished or not yet</returns>
  public bool IsScenarioFinished(){
    return _scenario_idx >= _subscenario_list.Count;
  }


  /// <summary>
  /// Skips the current subscenario to force continue its completion.
  /// </summary>
  public void SkipToNextSubScenario(){
    if(_scenario_idx >= _subscenario_list.Count)
      return;

    string _current_subscenario_id = _subscenario_list[_scenario_idx]._subscenario_id;
    StartCoroutine(__quest_finished(_current_subscenario_id));
  }


  /// <summary>
  /// To set the prepping data for prepping subscenario and another related functionality.
  /// This function used by <see cref="ScenarioDiagramVS"/> or can be used by Visual Scripting using normal calls.
  /// The function does preparation asynchronously, that redirects to <see cref="_set_init_data"/>.
  /// </summary>
  /// <param name="init_data">New scenario data</param>
  public void SetInitData(ScenarioDiagramVS.ScenarioData init_data){
    StartCoroutine(_set_init_data(init_data));
  }


  public bool IsLoaded(){
    return IsInitialized;
  }


  /// <summary>
  /// Get data related to this clsas as <see cref="PersistanceData"/>.
  /// </summary>
  /// <returns>The resulting data</returns>
  public PersistanceData GetPersistanceData(){
    return new PersistanceData{
      ScenarioID = _scenario_id,
      SubscenarioIdx = _scenario_idx
    };
  }


  /// <summary>
  /// Apply and modify this class based on <see cref="PersistanceData"/>.
  /// </summary>
  /// <param name="data">This class' data</param>
  /// <returns></returns>
  public IEnumerator SetPersistanceData(PersistanceData data){
    yield return SwitchSubScenario(data.SubscenarioIdx, false, false);
  }
}