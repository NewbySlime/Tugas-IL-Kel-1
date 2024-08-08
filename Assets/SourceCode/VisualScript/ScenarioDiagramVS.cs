using System;
using UnityEngine;
using Unity.VisualScripting;
using System.Collections;
using System.Collections.Generic;



/// <summary>
/// Class for handling Scenarios. This class acts as a server that holds a list of <see cref="ScenarioHandlerVS"/>. The difference between <see cref="ScenarioHandlerVS"/> is that this class handles a collection of scenarios contained while <see cref="ScenarioHandlerVS"/> handles a collection of subscenarios contained in a scenario.
/// For further explanation on how scenario works in the Game, read the diagram contained in <b>Reference/Diagrams/Scenario.drawio</b>
/// 
/// This class uses prefab(s);
/// - Prefab that has <see cref="ScenarioHandlerVS"/> for prepping scenario from scenario data.
/// </summary>
public class ScenarioDiagramVS: MonoBehaviour, ILoadingQueue{
  /// <summary>
  /// Event for when a scenario has been enabled/disabled.
  /// </summary>
  public event ScenarioChangedState ScenarioChangedStateEvent;
  public delegate void ScenarioChangedState(string scenario_id);

  /// <summary>
  /// Scenario data used for prepping a scenario.
  /// </summary>
  public class ScenarioData{
    /// <summary>
    /// Subscenario data.
    /// </summary>
    public struct SubData{
      /// <summary>
      /// The ID for a subscenario.
      /// </summary>
      public string SubID;
      
      /// <summary>
      /// Sequence data that will be triggered at the start of the subscenario.
      /// </summary>
      public SequenceHandlerVS.SequenceInitializeData SequenceStartData;

      /// <summary>
      /// Quest data used as a completion to the subscenario.
      /// </summary>
      public QuestHandlerVS.InitQuestInfo QuestData;

      /// <summary>
      /// Sequence data that will be triggered when a subscenario is completed.
      /// </summary>
      public SequenceHandlerVS.SequenceInitializeData SequenceFinishData;
    }

  
    /// <summary>
    /// The ID for a scenario.
    /// </summary>
    public string ScenarioID;

    /// <summary>
    /// The list of subscenario data in sequence.
    /// </summary>
    public List<SubData> SubscenarioList = new List<SubData>();
  }


  /// <summary>
  /// Data used for creating list of scenario data for this class by Visual Script or another source.
  /// </summary>
  public class ScenarioCollection{
    public List<ScenarioData> ScenarioList = new List<ScenarioData>();
  }


  [Serializable]
  /// <summary>
  /// Data for persistance use.
  /// This data contains about the state of the scenarios contained in <see cref="ScenarioDiagramVS"/>.
  /// </summary>
  public class PersistanceData: PersistanceContext.IPersistance{
    /// <summary>
    /// List of scenario that is active at the time of saving event.
    /// </summary>
    public string[] ListActivePersistance = new string[0];

    /// <summary>
    /// List of data related to <see cref="ScenarioHandlerVS"/>.
    /// </summary>
    public ScenarioHandlerVS.PersistanceData[] ScenarioDataCollections = new ScenarioHandlerVS.PersistanceData[0];


    public string GetDataID(){
      return "ScenarioData";
    }


    public string GetData(){
      return ConvertExt.ToBase64String(JsonUtility.ToJson(this));
    }

    public void SetData(string data){
      JsonUtility.FromJsonOverwrite(ConvertExt.FromBase64String(data), this);
    }
  }


  private class _scenario_metadata{
    public string scenario_id;
    public ScenarioData scenario_data;
    
    public ScenarioHandlerVS _handler;
  }

  
  [SerializeField]
  private GameObject _ScenarioHandlerPrefab;


  private Dictionary<string, _scenario_metadata> _scenario_map = new Dictionary<string, _scenario_metadata>();

  private int _scenario_idx = -1;


  /// <summary>
  /// Flag if the class is ready or not yet.
  /// </summary>
  /// <value></value>
  public bool IsInitialized{get; private set;} = false;


  private HashSet<ILoadingQueue> _init_queue_list = new HashSet<ILoadingQueue>();
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
  /// This function is an extension from <see cref="SetInitData"/> function which to sets and preps the new scenario with using coroutine for yielding functions.
  /// </summary>
  /// <param name="scenarios">A list of scenario data</param>
  /// <returns>Coroutine helper object</returns>
  private IEnumerator _set_init_data(ScenarioCollection scenarios){
    // Instantiate handler
    foreach(ScenarioData _data in scenarios.ScenarioList){
      if(_scenario_map.ContainsKey(_data.ScenarioID)){
        Debug.LogWarning(string.Format("Exists a duplicate of Scenario '{0}'", _data.ScenarioID));
        continue;
      }

      GameObject _new_obj = Instantiate(_ScenarioHandlerPrefab);
      ScenarioHandlerVS _handler = _new_obj.GetComponent<ScenarioHandlerVS>();
      if(_handler == null){
        Debug.LogError(string.Format("Handler Prefab is not a type of '{0}'", typeof(ScenarioHandlerVS).Name));
        throw new MissingComponentException();
      }

      _new_obj.transform.SetParent(transform);

      _scenario_map[_data.ScenarioID] = new _scenario_metadata{
        scenario_id = _data.ScenarioID,
        scenario_data = _data,

        _handler = _handler
      };
    }

    // wait until next Update()
    yield return null;
    yield return new WaitForEndOfFrame();

    // set data
    foreach(_scenario_metadata _metadata in _scenario_map.Values){
      _metadata._handler.SetInitData(_metadata.scenario_data);

      _init_queue_list.Add(_metadata._handler);
    }

    DEBUGModeUtils.Log("scenario check queue");
    yield return new WaitUntil(_check_queue_list);
    
    DEBUGModeUtils.Log("scenario disable");
    // disable
    foreach(_scenario_metadata _metadata in _scenario_map.Values)
      SetEnableScenario(_metadata.scenario_id, false);

    IsInitialized = true;
  }
  

  /// <summary>
  /// Modify scenario states based on the supplied persistance data.
  /// </summary>
  /// <param name="data">Saved data in persistance</param>
  /// <returns>Coroutine helper object</returns>
  private IEnumerator _set_persistance_data(PersistanceData data){
    yield return ResetAllScenario();

    foreach(ScenarioHandlerVS.PersistanceData _scenario_data in data.ScenarioDataCollections){
      ScenarioHandlerVS _handler = GetScenario(_scenario_data.ScenarioID);
      if(_handler == null){
        Debug.LogWarning(string.Format("LOAD: Cannot get handler for Scenario ID: '{0}'.", _scenario_data.ScenarioID));
        continue;
      }

      yield return _handler.SetPersistanceData(_scenario_data);
    }

    foreach(string _active_scenario in data.ListActivePersistance)
      SetEnableScenario(_active_scenario, true);
  }


  public void Start(){
    GameHandler _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find Game Handler.");
      throw new MissingComponentException();
    }

    _game_handler.AddLoadingQueue(this);
  }


  /// <summary>
  /// Starts a scenario from the beginning based on the ID.
  /// </summary>
  /// <param name="scenario_id">Target scenario ID</param>
  /// <returns>Coroutine helper object</returns>
  public IEnumerator StartScenario(string scenario_id){
    if(!_scenario_map.ContainsKey(scenario_id)){
      Debug.LogError(string.Format("Cannot get Scenario with ID: '{0}', Scenario will not be triggered", scenario_id));
      yield break;
    }

    _scenario_metadata _metadata = _scenario_map[scenario_id];
    if(_metadata._handler.gameObject.activeInHierarchy)
      Debug.LogWarning(string.Format("Scenario (ID: '{0}') already active.", _metadata.scenario_id));

    _metadata._handler.gameObject.SetActive(true);

    ScenarioChangedStateEvent?.Invoke(scenario_id);
    yield return _metadata._handler.SwitchSubScenario(0);
  }


  /// <summary>
  /// Enable or disable a scenario based on the ID. Not preferred to start a scenario using this class. For that, see <see cref="StartScenario"/>.
  /// </summary>
  /// <param name="scenario_id">Target scenario ID</param>
  /// <param name="enabled">Enable/disable flag</param>
  public void SetEnableScenario(string scenario_id, bool enabled){
    DEBUGModeUtils.Log(string.Format("Scenario {0} {1}", scenario_id, enabled));
    if(!_scenario_map.ContainsKey(scenario_id))
      return;

    _scenario_metadata _metadata = _scenario_map[scenario_id];
    _metadata._handler.gameObject.SetActive(enabled);

    ScenarioChangedStateEvent?.Invoke(scenario_id);
  }

  /// <summary>
  /// Check if a scenario is enabled or disabled.
  /// </summary>
  /// <param name="scenario_id">Target scenario ID to check</param>
  /// <returns>Is the scenario enabled</returns>
  public bool GetEnableScenario(string scenario_id){
    if(!_scenario_map.ContainsKey(scenario_id))
      return false;

    _scenario_metadata _metadata = _scenario_map[scenario_id];
    return _metadata._handler.gameObject.activeInHierarchy;
  }


  /// <summary>
  /// Resets all scenario to initial state (also disables them).
  /// </summary>
  /// <returns>Coroutine helper object</returns>
  public IEnumerator ResetAllScenario(){
    foreach(string _scenario_id in _scenario_map.Keys){
      _scenario_metadata _metadata = _scenario_map[_scenario_id];
      yield return _metadata._handler.SwitchSubScenario(0, false, false);

      SetEnableScenario(_scenario_id, false);
    }
  }


  /// <summary>
  /// Trigger skipping to a scenario for switching to the next subscenario (skipping completions needed to continue). 
  /// </summary>
  /// <param name="scenario_id">Target scenario ID</param>
  public void SkipToNextSubScenario(string scenario_id){
    if(!_scenario_map.ContainsKey(scenario_id)){
      Debug.LogWarning(string.Format("Cannot find Scenario. (ID: {0})", scenario_id));
      return;
    }

    _scenario_metadata _metadata = _scenario_map[scenario_id];
    _metadata._handler.SkipToNextSubScenario();
  }


  #nullable enable
  /// <summary>
  /// Get <see cref="ScenarioHandlerVS"/> based on the ID.
  /// </summary>
  /// <param name="scenario_id">Target scenario ID</param>
  /// <returns>The handler object</returns>
  public ScenarioHandlerVS? GetScenario(string scenario_id){
    if(!_scenario_map.ContainsKey(scenario_id))
      return null;

    return _scenario_map[scenario_id]._handler;
  }
  #nullable disable


  /// <summary>
  /// To set the prepping data for this class to create and prep a list of scenarios.
  /// This function can be used by Visual Scripting using normal calls.
  /// The function does preparation asynchronously, that redirects to <see cref="_set_init_data"/>.
  /// </summary>
  /// <param name="scenarios">List of scenario data</param>
  public void SetInitData(ScenarioCollection scenarios){
    StartCoroutine(_set_init_data(scenarios));
  }


  public bool IsLoaded(){
    return IsInitialized;
  }


  /// <summary>
  /// Get data related to this class as <see cref="PersistanceData"/>.
  /// </summary>
  /// <returns>The resulting data</returns>
  public PersistanceData GetPersistanceData(){
    PersistanceData _result = new();
    foreach(_scenario_metadata _metadata in _scenario_map.Values){
      DEBUGModeUtils.Log(string.Format("scenario id {0} {1}", _metadata.scenario_id, GetEnableScenario(_metadata.scenario_id)));
      if(GetEnableScenario(_metadata.scenario_id)){
        Array.Resize(ref _result.ListActivePersistance, _result.ListActivePersistance.Length+1);
        _result.ListActivePersistance[_result.ListActivePersistance.Length-1] = _metadata.scenario_id;
      }

      ScenarioHandlerVS _scenario = GetScenario(_metadata.scenario_id);
      if(_scenario == null){
        Debug.LogWarning(string.Format("SAVE: Cannot get handler for Scenario ID: '{0}'.", _metadata.scenario_id));
        continue;
      }
      
      Array.Resize(ref _result.ScenarioDataCollections, _result.ScenarioDataCollections.Length+1);
      _result.ScenarioDataCollections[_result.ScenarioDataCollections.Length-1] =
        _scenario.GetPersistanceData();
    }

    return _result;
  }

  /// <summary>
  /// Apply and modify this class based on <see cref="PersistanceData"/>.
  /// </summary>
  /// <param name="data">This class' data</param>
  public void SetPersistanceData(PersistanceData data){
    StartCoroutine(_set_persistance_data(data));
  }
}