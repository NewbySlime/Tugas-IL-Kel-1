using System;
using UnityEngine;
using Unity.VisualScripting;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using TMPro.EditorUtilities;
using Unity.Jobs;



public class ScenarioDiagramVS: MonoBehaviour, ILoadingQueue{
  public delegate void ScenarioChangedState(string scenario);
  public event ScenarioChangedState ScenarioChangedStateEvent;

  public class ScenarioData{
    public struct SubData{
      public string SubID;
      
      public SequenceHandlerVS.SequenceInitializeData SequenceStartData;
      public QuestHandlerVS.InitQuestInfo QuestData;
      public SequenceHandlerVS.SequenceInitializeData SequenceFinishData;
    }

  
    public string ScenarioID;
    public List<SubData> SubscenarioList = new List<SubData>();
  }


  public class ScenarioCollection{
    public List<ScenarioData> ScenarioList = new List<ScenarioData>();
  }


  [Serializable]
  public class PersistanceData: PersistanceContext.IPersistance{
    public string[] ListActivePersistance = new string[0];
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

      _handler.transform.SetParent(transform);

      _scenario_map[_data.ScenarioID] = new _scenario_metadata{
        scenario_id = _data.ScenarioID,
        scenario_data = _data,

        _handler = _handler
      };
    }

    // tunggu sampai Start() selanjutnya
    yield return new WaitForNextFrameUnit();
    yield return new WaitForEndOfFrame();

    // Set data
    foreach(_scenario_metadata _metadata in _scenario_map.Values){
      _metadata._handler.SetInitData(_metadata.scenario_data);

      _init_queue_list.Add(_metadata._handler);
    }

    Debug.Log("scenario check queue");
    yield return new WaitUntil(_check_queue_list);
    
    Debug.Log("scenario disable");
    // disable
    foreach(_scenario_metadata _metadata in _scenario_map.Values)
      SetEnableScenario(_metadata.scenario_id, false);

    IsInitialized = true;
  }


  public void Start(){
    GameHandler _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find Game Handler.");
      throw new MissingComponentException();
    }

    _game_handler.AddLoadingQueue(this);
  }


  public IEnumerator StartScenario(string scenario_id){
    if(!_scenario_map.ContainsKey(scenario_id)){
      Debug.LogError(string.Format("Cannot get Scenario with ID: '{0}', Scenario will not be triggered", scenario_id));
      yield break;
    }

    _scenario_metadata _metadata = _scenario_map[scenario_id];
    if(_metadata._handler.gameObject.activeInHierarchy)
      Debug.LogWarning(string.Format("Scenario (ID: '{0}') already active."));

    _metadata._handler.gameObject.SetActive(true);

    ScenarioChangedStateEvent?.Invoke(scenario_id);
    yield return _metadata._handler.SwitchSubScenario(0);
  }


  public void SetEnableScenario(string scenario_id, bool enabled){
    Debug.Log(string.Format("Scenario {0} {1}", scenario_id, enabled));
    if(!_scenario_map.ContainsKey(scenario_id))
      return;

    _scenario_metadata _metadata = _scenario_map[scenario_id];
    _metadata._handler.gameObject.SetActive(enabled);

    ScenarioChangedStateEvent?.Invoke(scenario_id);
  }

  public bool GetEnableScenario(string scenario_id){
    if(!_scenario_map.ContainsKey(scenario_id))
      return false;

    _scenario_metadata _metadata = _scenario_map[scenario_id];
    return _metadata._handler.gameObject.activeInHierarchy;
  }


  #nullable enable
  public ScenarioHandlerVS? GetScenario(string scenario_id){
    if(!_scenario_map.ContainsKey(scenario_id))
      return null;

    return _scenario_map[scenario_id]._handler;
  }
  #nullable disable


  public void SetInitData(ScenarioCollection scenarios){
    StartCoroutine(_set_init_data(scenarios));
  }


  public bool IsLoaded(){
    return IsInitialized;
  }


  public PersistanceData GetPersistanceData(){
    PersistanceData _result = new();
    foreach(_scenario_metadata _metadata in _scenario_map.Values){
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

  public void SetPersistanceData(PersistanceData data){
    foreach(ScenarioHandlerVS.PersistanceData _scenario_data in data.ScenarioDataCollections){
      ScenarioHandlerVS _handler = GetScenario(_scenario_data.ScenarioID);
      if(_handler == null){
        Debug.LogWarning(string.Format("LOAD: Cannot get handler for Scenario ID: '{0}'.", _scenario_data.ScenarioID));
        continue;
      }

      _handler.SetPersistanceData(_scenario_data);
    }

    foreach(string _active_scenario in data.ListActivePersistance)
      SetEnableScenario(_active_scenario, true);
  }
}