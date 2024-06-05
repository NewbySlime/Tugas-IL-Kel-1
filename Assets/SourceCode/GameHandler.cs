using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameHandler: MonoBehaviour{

  public const string DefaultSceneID = "main_menu_scene";
  public const string DefaultGameSceneID = "main_game_scene"; 
  public const string DefaultScenarioID = "default_scenario";

  public delegate void SceneChangedInitializing(string scene_id, GameContext context);
  public event SceneChangedInitializing SceneChangedInitializingEvent;

  public delegate void SceneChangedFinished(string scene_id, GameContext context);
  public event SceneChangedFinished SceneChangedFinishedEvent;

  public delegate void SceneRemoving();
  public event SceneRemoving SceneRemovingEvent;

  public delegate void LoadDataFromPersistance(PersistanceContext context);
  public event LoadDataFromPersistance LoadDataFromPersistanceEvent;


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

  [Serializable]
  private class SceneContext: PersistanceContext.IPersistance{
    public string SceneID;
    public string LastCheckpointID;

    public string GetDataID(){
      return "SceneContext";
    }


    public string GetData(){
      return ConvertExt.ToBase64String(JsonUtility.ToJson(this));
    }

    public void SetData(string data){
      JsonUtility.FromJsonOverwrite(ConvertExt.FromBase64String(data), this);
    }
  }

  [Serializable]
  private class SceneContextCollection: PersistanceContext.IPersistance{
    public SceneContext[] SceneContexts = new SceneContext[0];

    public string GetDataID(){
      return "SceneContext.Collections";
    }


    public string GetData(){
      return ConvertExt.ToBase64String(JsonUtility.ToJson(this));
    }

    public void SetData(string data){
      JsonUtility.FromJsonOverwrite(ConvertExt.FromBase64String(data), this);
    }
  }


  [Serializable]
  private class GameLevelContext: PersistanceContext.IPersistance{
    public string CurrentSceneID;


    public string GetDataID(){
      return "GameLevelContext";
    }


    public string GetData(){
      return ConvertExt.ToBase64String(JsonUtility.ToJson(this));
    }

    public void SetData(string data){
      JsonUtility.FromJsonOverwrite(ConvertExt.FromBase64String(data), this);
    }
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
  private LevelCheckpointDatabase _checkpoint_database;

  private GameTimeHandler _time_handler;

  private Dictionary<string, _SceneMetadata> _scene_map = new Dictionary<string, _SceneMetadata>();

  private HashSet<ILoadingQueue> _scene_loading_object_list = new HashSet<ILoadingQueue>();

  private ScenarioDiagramVS _scenario_diagram;
  internal ScenarioDiagramVS _ScenarioDiagram{get => _scenario_diagram;}

  private GameUIHandler _ui_handler;
  private InputFocusContext _input_context;

  private GameOverUI _game_over_ui;
  private FadeUI _game_over_fadeui;

  private LoadingUI _level_loading_ui;
  
  private FadeUI _fade_gu_ui;


  private Dictionary<string, SceneContext> _scene_context_map = new();


  private string _current_scene;
  private string _last_scene;

  private bool _scene_initialized = false;
  public bool SceneInitialized{get => _scene_initialized;}

  private bool _obj_initialized = false;
  public bool Initialized{get => _obj_initialized;}

  public bool AreaTriggerEnable = true;


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
    yield return null;
    while(_scene_loading_object_list.Count > 0){
      yield return null;
      _check_loading_object();
    }

    // agar tidak banyak bug, biarkan physics untuk diproses sebelum even finish
    yield return null;
    yield return new WaitForEndOfFrame();

    Debug.Log("Game handler scene change finishing.");
    SceneChangedFinishedEvent?.Invoke(_current_metadata.SceneID, _current_metadata.SceneContext);
    _scene_initialized = true;
  }


  private IEnumerator _change_scene(string scene_id, string teleport_to = "", bool do_save = true){
    if(!_scene_map.ContainsKey(scene_id)){
      Debug.LogWarning(string.Format("Cannot find Scene ID: '{0}'", scene_id));
      yield break;
    }

    _scene_initialized = false;
    _time_handler.StopTime();

    Debug.Log(string.Format("Chanding scene to: {0}", scene_id));
    _level_loading_ui.SetLoadingProgress(0);

    _level_loading_ui.FadeToCover = true;
    yield return TimingBaseUI.StartAllTimer(_level_loading_ui);

    SceneRemovingEvent?.Invoke();

    ObjectReference.ClearReference();
    _input_context.ClearRegisters();

    _last_scene = _current_scene;
    _current_scene = scene_id;
    
    _SceneMetadata _metadata = _scene_map[scene_id];
    AsyncOperation _async_op = SceneManager.LoadSceneAsync(_metadata.SceneFile.name);
    _level_loading_ui.BindAsyncOperation(_async_op);
    yield return new WaitUntil(() => _async_op.isDone);

    _checkpoint_database.UpdateDatabase();

    _time_handler.ResumeTime();
    
    Debug.Log(string.Format("teleport to {0}", teleport_to));
    if(_metadata.Metadata.SceneContext == GameContext.InGame && teleport_to.Length > 0){
      Debug.Log("check teleporting");
      bool _do_teleport = true;

      CheckpointHandler _handler = null;
      LevelCheckpointDatabase _checkpoint_db = FindAnyObjectByType<LevelCheckpointDatabase>();
      if(_checkpoint_db == null){
        Debug.LogWarning("No Checkpoint database found. Game will not teleport Player as needed.");
        _do_teleport = false;
      }else{
        _handler = _checkpoint_db.GetCheckpoint(teleport_to);
        if(_handler == null){
          Debug.LogWarning(string.Format("No Checkpoint found for ID:'{0}'", teleport_to));
          _do_teleport = false;
        }
      }
      
      PlayerController _player = FindAnyObjectByType<PlayerController>();
      if(_player == null){
        Debug.LogError("Player isn't found in the level?");
        _do_teleport = false;
      }

      Debug.Log(string.Format("do teleport {0}", _do_teleport));
      if(_do_teleport){
        _handler.TeleportObject(_player.gameObject);

        yield return new WaitForNextFrameUnit();
        yield return new WaitForEndOfFrame();
      }
    }

    yield return _wait_scene_initialize(_metadata.Metadata);

    yield return new WaitUntil(() => _level_loading_ui.UIAnimationFinished);

    _level_loading_ui.FadeToCover = false;
    yield return TimingBaseUI.StartAllTimer(_level_loading_ui);

    _level_loading_ui.UnbindAsyncOperation();

    if(_metadata.Metadata.SceneContext == GameContext.InGame && do_save)
      SaveGame();
  }


#if DEBUG
  private IEnumerator _load_game_first_time(){
    yield return new WaitUntil(() => Initialized && SceneInitialized);
    LoadGame(DEBUG_ChangeSceneStartLoad);
  }
#endif

  private IEnumerator _load_game(bool change_scene = true){
    _fade_gu_ui.FadeToCover = true;
    yield return TimingBaseUI.StartAllTimer(_fade_gu_ui);

    _persistance_context.ReadSave();

    GameLevelContext _game_context = new();
    _persistance_context.OverwriteData(_game_context);

    SceneContextCollection _scene_contexts = new();
    _persistance_context.OverwriteData(_scene_contexts);

    _scene_context_map.Clear();
    foreach(SceneContext _context in _scene_contexts.SceneContexts){
      Debug.Log(string.Format("scene context {0}", _context.SceneID));
      _scene_context_map[_context.SceneID] = _context;
    }

    ScenarioDiagramVS.PersistanceData _scenario_data = new();
    _persistance_context.OverwriteData(_scenario_data);
    _scenario_diagram.SetPersistanceData(_scenario_data);

    // load scene
    if(change_scene){
      string _teleport_id = "";
      if(_scene_context_map.ContainsKey(_game_context.CurrentSceneID)){
        SceneContext _scene_context = _scene_context_map[_game_context.CurrentSceneID]; 
        _teleport_id = _scene_context.LastCheckpointID;
      }

      Debug.Log(string.Format("scene {0}, teleport {1}", _game_context.CurrentSceneID, _teleport_id));
      yield return _change_scene(_game_context.CurrentSceneID, _teleport_id, false);
    }
    
    LoadDataFromPersistanceEvent?.Invoke(_persistance_context);

    _fade_gu_ui.FadeToCover = false;
    yield return TimingBaseUI.StartAllTimer(_fade_gu_ui);
  }


  private IEnumerator _quit_game(){
    _fade_gu_ui.FadeToCover = true;
    yield return TimingBaseUI.StartAllTimer(_fade_gu_ui);

    Application.Quit();
  }

  
  private IEnumerator _save_game_co_func(){
    Debug.Log("Saving data...");
    
    _ui_handler.SetUtilityHUDUIMode(GameUIHandler.UtilityHUDUIEnum.SaveHintUI, true);
    
    yield return _persistance_context.WriteSaveAsync();
    yield return new WaitForSeconds(1);

    _ui_handler.SetUtilityHUDUIMode(GameUIHandler.UtilityHUDUIEnum.SaveHintUI, false);
  }

  private void _persistance_saving(PersistanceContext context){
    GameLevelContext _game_context = new GameLevelContext{
      CurrentSceneID = _current_scene
    };

    SceneContextCollection _scene_context_collection = new SceneContextCollection{
      SceneContexts = new SceneContext[_scene_context_map.Count]
    };

    int idx = 0;
    foreach(SceneContext _context in _scene_context_map.Values){
      _scene_context_collection.SceneContexts[idx] = _context;

      idx++;
    }

    context.ParseData(_game_context);
    context.ParseData(_scene_context_collection);
    context.ParseData(_scenario_diagram.GetPersistanceData());
  }


  public void Awake(){
    foreach(var _metadata in _SceneMetadataList)
      _scene_map[_metadata.Metadata.SceneID] = _metadata;
  }

  private IEnumerator _StartAsCoroutine(){
    Debug.Log("Game Handler init");

    _time_handler = FindAnyObjectByType<GameTimeHandler>();
    if(_time_handler == null){
      Debug.LogError("Cannot find GameTimeHandler.");
      throw new MissingReferenceException();
    }

    _ui_handler = FindAnyObjectByType<GameUIHandler>();
    if(_ui_handler == null){
      Debug.LogError("Cannot find GameUIHandler.");
      throw new MissingReferenceException();
    }
    
    _input_context = FindAnyObjectByType<InputFocusContext>();
    if(_input_context == null){
      Debug.LogError("Cannot find InputFocusContext.");
      throw new MissingReferenceException();
    }


    _persistance_context = FindAnyObjectByType<PersistanceContext>();
    if(_persistance_context == null){
      Debug.LogError("Cannot get Persistance Context Loader.");
      throw new MissingComponentException();
    }

    _persistance_context.PersistanceSavingEvent += _persistance_saving;

    _scenario_diagram = FindAnyObjectByType<ScenarioDiagramVS>();
    if(_scenario_diagram == null){
      Debug.LogError("Cannot get Scenario Diagram.");
      throw new MissingComponentException();
    }

    _checkpoint_database = FindAnyObjectByType<LevelCheckpointDatabase>();
    if(_checkpoint_database == null){
      Debug.LogError("Cannot get database for Checkpoints.");
      throw new MissingComponentException();
    }


    _level_loading_ui = _ui_handler.GetLevelLoadingUI();
    if(_level_loading_ui == null){
      Debug.LogError("Level Loading UI Object does not have LoadingUI.");
      throw new MissingComponentException();
    }

    _fade_gu_ui = _ui_handler.GetGeneralFadeUI();
    if(_fade_gu_ui == null){
      Debug.LogError("FadeGU UI Object does not have FadeUI.");
      throw new MissingComponentException();
    }

    
    // tunggu sampai fade general usage UI sudah inisialisasi
    yield return new WaitForNextFrameUnit();
    yield return new WaitForEndOfFrame();

    _fade_gu_ui.FadeToCover = true;
    yield return TimingBaseUI.StartAllTimer(_fade_gu_ui, true);

    _game_over_ui = _ui_handler.GetGameOverUI();
    if(_game_over_ui == null){
      Debug.LogError("GameOverUI Object does not have GameOverUI.");
      throw new MissingComponentException();
    }

    _game_over_fadeui = _game_over_ui.GetComponent<FadeUI>();
    if(_game_over_fadeui == null){
      Debug.LogError("GameOverUI Object does not have FadeUI.");
      throw new MissingComponentException();
    }

#if DEBUG
    // tunggu sampai start selanjutnya / menunggu semua objek inisialisasi
    yield return new WaitForNextFrameUnit();
    yield return new WaitForEndOfFrame();

    if(DEBUG_LoadDataOnStart)
      StartCoroutine(_load_game_first_time());

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
    yield return _scenario_diagram.StartScenario(DefaultScenarioID);

#else
    ChangeScene(DefaultSceneID);
#endif

    _fade_gu_ui.FadeToCover = false;
    yield return TimingBaseUI.StartAllTimer(_fade_gu_ui);

    _obj_initialized = true;
  }

  public void Start(){
    StartCoroutine(_StartAsCoroutine());
  }


#if DEBUG
  public void LoadGame(bool change_scene = true){
    StartCoroutine(_load_game(change_scene));
#else
  public void LoadGame(){
    StartCoroutine(_load_game(true));
#endif
  }

  public void StartNewGame(){
    _persistance_context.ClearData();

    _scene_context_map.Clear();
    foreach(_SceneMetadata _metadata in _SceneMetadataList)
      _scene_context_map[_metadata.Metadata.SceneID] = new SceneContext{
        SceneID = _metadata.Metadata.SceneID,
        LastCheckpointID = ""
      };

    ChangeScene(DefaultGameSceneID);
  }


  public void SaveGame(){
    StartCoroutine(_save_game_co_func());
  }


  public void RestartFromLastCheckpoint(){
    LoadGame();
  }

  public void SetLastCheckpoint(string checkpoint_id){
    if(!_scene_context_map.ContainsKey(_current_scene))
      _scene_context_map[_current_scene] = new SceneContext{
        SceneID = _current_scene
      };

    SceneContext _current_context = _scene_context_map[_current_scene];
    _current_context.LastCheckpointID = checkpoint_id;
  }


  public void ChangeScene(string scene_id, string teleport_to = ""){
    StartCoroutine(_change_scene(scene_id, teleport_to));
  }

  public void ChangeSceneToMainMenu(){
    Debug.LogWarning("Main menu is not implemented yet.");
  }

  public string GetLastScene(){
    return _last_scene;
  }


  public void QuitGame(){
    StartCoroutine(_quit_game());
  }


  public void PauseGame(){
    _time_handler.StopTime();
  }

  public void ResumeGame(){
    _time_handler.ResumeTime();
  }


  public void AddLoadingQueue(ILoadingQueue queue){
    _scene_loading_object_list.Add(queue);
  }


  public string GetCurrentSceneID(){
    return _current_scene;
  }

  public GameContext GetCurrentSceneContext(){
    if(!_scene_map.ContainsKey(_current_scene)){
      Debug.LogWarning(string.Format("No Metadata found for Scene: '{0}'", _current_scene));
      return GameContext.InGame;
    }

    return _scene_map[_current_scene].Metadata.SceneContext;
  }


  public void TriggerPlayerSpotted(){
    TriggerGameOver("Kamu Ketahuan!");
  }

  public void TriggerPlayerDied(){
    TriggerGameOver("Yadi Pingsan!");
  }

  public void TriggerGameOver(string cause_text){
    _ui_handler.ResetMainUIMode();
    _ui_handler.SetPlayerUIMode(GameUIHandler.PlayerUIMode.GameOver);

    _time_handler.StopTime();

    _input_context.RegisterInputObject(_game_over_ui, InputFocusContext.ContextEnum.UI);
    _game_over_ui.SetCauseText(cause_text);
  }
}