using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameHandler: MonoBehaviour{

  public const string DefaultSceneID = "main_menu_scene";
  public const string DefaultScenarioID = "default_scenario";

  public delegate void SceneChangedInitializing(string scene_id, GameContext context);
  public event SceneChangedInitializing SceneChangedInitializingEvent;

  public delegate void SceneChangedFinished(string scene_id, GameContext context);
  public event SceneChangedFinished SceneChangedFinishedEvent;

  public delegate void SceneRemoving();
  public event SceneRemoving SceneRemovingEvent;

  public delegate void ScenarioStateChanged(string scenario);
  public event ScenarioStateChanged ScenarioStateChangedEvent;

  public delegate void ScenarioFinished(string scenario);
  public event ScenarioFinished ScenarioFinishedEvent;


  public delegate void SubScenarioStateChanged(string scenario, string last_subid, string next_subid);
  public event SubScenarioStateChanged SubScenarioStateChangedEvent;

  public delegate void SubScenarioFinished(string scenario, string subscenario);
  public event SubScenarioFinished SubScenarioFinishedEvent;



  public enum GameContext{
    InGame,
    MainMenu
  }

  [Serializable]
  public class SceneMetadata{
    public string SceneID;
    public GameContext SceneContext;   
  }

  [Serializable]
  private class _SceneMetadata{
    public SceneMetadata Metadata;

    public UnityEngine.Object SceneFile;
  }


  [SerializeField]
  private GameObject _ScenarioObjectContainer;

  [SerializeField]
  private List<_SceneMetadata> _SceneMetadataList;


#if DEBUG
  [SerializeField]
  private bool DEBUG_LoadDataOnStart = false;

  [SerializeField]
  private bool DEBUG_ChangeSceneStartLoad = true;
#endif


  private PersistanceContext _persistance_context;


  private Dictionary<string, _SceneMetadata> _scene_map = new Dictionary<string, _SceneMetadata>();

  private Dictionary<string, ScenarioHandler> _scenario_map = new Dictionary<string, ScenarioHandler>();


  private HashSet<ILoadingQueue> _scene_loading_object_list = new HashSet<ILoadingQueue>();


  private string _current_scene;
  private string _last_scene;



  private void _on_scenario_finished(string scenario){
    ScenarioFinishedEvent?.Invoke(scenario);
  }

  private void _on_subscenario_changed(string scenario, string last_subid, string next_subid){
    SubScenarioStateChangedEvent?.Invoke(scenario, last_subid, next_subid);
  }

  private void _on_subscenario_finished(string scenario, string subscenario){
    SubScenarioFinishedEvent?.Invoke(scenario, subscenario);
  }


  private void _check_loading_object(){
    List<ILoadingQueue> _delete_loading = new List<ILoadingQueue>();
    foreach(ILoadingQueue _queue in _scene_loading_object_list){
      if(_queue.IsLoaded())
        _delete_loading.Add(_queue);
    }

    foreach(ILoadingQueue _delete in _delete_loading)
      _scene_loading_object_list.Remove(_delete);
  }

  private IEnumerator _wait_scene_initialize(SceneMetadata _current_metadata){
    SceneChangedInitializingEvent?.Invoke(_current_metadata.SceneID, _current_metadata.SceneContext);

    // tunggu sampai semua komponen sudah ter-inisialisasi
    yield return new WaitForNextFrameUnit();
    while(_scene_loading_object_list.Count > 0){
      yield return null;
      _check_loading_object();
    }

    // agar tidak banyak bug, biarkan physics untuk diproses sebelum even finish
    yield return new WaitForEndOfFrameUnit();
    yield return new WaitForNextFrameUnit();

    Debug.Log("Game handler scene change finishing.");
    SceneChangedFinishedEvent?.Invoke(_current_metadata.SceneID, _current_metadata.SceneContext);
  }


  public void Awake(){
    foreach(var _metadata in _SceneMetadataList)
      _scene_map[_metadata.Metadata.SceneID] = _metadata;
  }


  private HashSet<ScenarioHandler> _scenario_queue_list = new HashSet<ScenarioHandler>();
  private IEnumerator _scenario_queue_initialize(ScenarioHandler _scenario){
    _scenario_queue_list.Add(_scenario);
    
    _scenario.gameObject.SetActive(true);
    yield return new WaitUntil(() => _scenario.ComponentIntialized);
    _scenario.gameObject.SetActive(false);
    ScenarioStateChangedEvent?.Invoke(_scenario.GetScenarioID());

    _scenario_queue_list.Remove(_scenario);
  }

  private IEnumerator _StartAsCoroutine(){
    Debug.Log("Game Handler init");
    PersistanceContext _persistance_context = FindAnyObjectByType<PersistanceContext>();
    if(_persistance_context == null){
      Debug.LogError("Cannot get Persistance Context Loader.");
      throw new UnityEngine.MissingComponentException();
    }

#if DEBUG
    if(DEBUG_LoadDataOnStart){  
      yield return new WaitUntil(() => _persistance_context.IsInitialized);
      LoadGame(DEBUG_ChangeSceneStartLoad);
    }
#endif
    
    ScenarioHandler[] _list_scenarios = _ScenarioObjectContainer.transform.GetComponentsInChildren<ScenarioHandler>();
    foreach(ScenarioHandler _scenario in _list_scenarios){
      _scenario.ScenarioFinishedevent += _on_scenario_finished;
      _scenario.ScenarioSubscenarioChangedEvent += _on_subscenario_changed;
      _scenario.ScenarioSubscenarioFinishedEvent += _on_subscenario_finished;
      
      Debug.Log(string.Format("Scenario '{0}'", _scenario.GetScenarioID()));
      _scenario_map[_scenario.GetScenarioID()] = _scenario;
      StartCoroutine(_scenario_queue_initialize(_scenario));
    }
    
    if(!_scenario_map.ContainsKey(DefaultScenarioID)){
      Debug.LogError("No default scenario to use.");
      throw new UnityEngine.UnassignedReferenceException();
    }

    yield return new WaitUntil(() => _scenario_queue_list.Count <= 0);
    Debug.Log("All scene initialized."); 
    
#if DEBUG
    SceneMetadata _metadata = new SceneMetadata{
      SceneID = "",
      SceneContext = GameContext.InGame
    };

    DEBUG_SceneMetadata _scene_metadata = FindAnyObjectByType<DEBUG_SceneMetadata>();
    if(_scene_metadata != null)
      _metadata = _scene_metadata.GetSceneMetadata();
    else
      Debug.LogWarning("DEBUGMODE: Cannot get Scene Metadata.");

    _current_scene = _metadata.SceneID;

    yield return _wait_scene_initialize(_metadata);
    yield return StartScenario(DefaultScenarioID);
#else
    ChangeScene(DefaultSceneID);
#endif
  }

  public void Start(){
    StartCoroutine(_StartAsCoroutine());
  }


  public IEnumerator StartScenario(string scenario_id){
    Debug.Log(string.Format("Scenario started '{0}'", scenario_id));
    if(!_scenario_map.ContainsKey(scenario_id)){
      Debug.LogError(string.Format("Cannot get Scenario with ID: '{0}', Scenario will not be triggered", scenario_id));
      yield break;
    }

    ScenarioHandler _scenario = _scenario_map[scenario_id];
    if(_scenario.gameObject.activeSelf)
      Debug.LogWarning(string.Format("Scenario (ID: '{0}') already active.", scenario_id));
    
    _scenario.gameObject.SetActive(true);
    yield return new WaitUntil(() => _scenario.ComponentIntialized);

    ScenarioStateChangedEvent?.Invoke(scenario_id);
    yield return _scenario.SwitchSubScenario(0);
  }

  public void SetEnableScenario(string scenario_id, bool enabled){
    if(!_scenario_map.ContainsKey(scenario_id))
      return;

    ScenarioHandler _scenario_handler = _scenario_map[scenario_id];
    _scenario_handler.gameObject.SetActive(enabled);
    
    ScenarioStateChangedEvent?.Invoke(scenario_id);
  }

  public bool GetEnableScenario(string scenario_id){
    if(!_scenario_map.ContainsKey(scenario_id))
      return false;

    ScenarioHandler _scenario_handler = _scenario_map[scenario_id];
    return _scenario_handler.gameObject.activeSelf;
  }


  #nullable enable
  public ScenarioHandler? GetScenario(string scenario_id){
    Debug.Log(string.Format("Scenario get '{0}'", scenario_id));
    if(!_scenario_map.ContainsKey(scenario_id))
      return null;

    return _scenario_map[scenario_id];
  }
  #nullable disable


  public void GameOverTrigger(){

  }


#if DEBUG
  public void LoadGame(bool change_scene = true){
#else
  public void LoadGame(){
#endif


  }

  public void StartNewGame(){

  }


  public void SaveGame(){
    Debug.Log("Save triggered");
  }


  public void RestartFromLastCheckpoint(){

  }

  public void SetLastCheckpoint(string checkpoint_id){

  }


  public void ChangeScene(string scene_id){
    Debug.Log(string.Format("Chanding scene to: {0}", scene_id));
    SceneRemovingEvent?.Invoke();

    if(!_scene_map.ContainsKey(scene_id)){
      Debug.LogWarning(string.Format("Cannot find Scene ID: '{0}'", scene_id));
      return;
    }

    _last_scene = _current_scene;
    _current_scene = scene_id;
    
    _SceneMetadata _metadata = _scene_map[scene_id];
    SceneManager.LoadScene(_metadata.SceneFile.name);    
    StartCoroutine(_wait_scene_initialize(_metadata.Metadata));
  }

  public string GetLastScene(){
    return _last_scene;
  }


  public void PauseGame(){

  }

  public void ResumeGame(){
    
  }


  public void AddLoadingQueue(ILoadingQueue queue){
    _scene_loading_object_list.Add(queue);
  }
}