using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(PersistanceContext))]
public class GameHandler: MonoBehaviour{
  public const string RuntimeSceneDataFile = "GameConfigs/RuntimeSceneData";

  public const string DefaultSceneID = "main_menu_scene";
  public const string DefaultGameSceneID = "intro_game_scene"; 
  public const string DefaultScenarioID = "intro_scenario";

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
  public class SceneData{
    public SceneMetadata Metadata;

    public UnityEngine.Object SceneFile;
  }


  [Serializable]
  public class RuntimeSceneData{
    public SceneMetadata Metadata;

    public string SceneName;
  }

  [Serializable]
  public class RuntimeSceneDataWrapper{
    public RuntimeSceneData[] SceneDataList = new RuntimeSceneData[0];
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


#if DEBUG
  [Serializable]
  private struct SetScenarioData{
    public string ScenarioID;
    public int SubScenarioIdx;

    public bool TriggerEntrySequence;
  }
#endif

  


  [SerializeField]
  private GameObject _ScenarioObjectContainer;

  [SerializeField]
  private List<SceneData> _SceneMetadataList;


#if DEBUG
  [SerializeField]
  private bool DEBUG_LoadDataOnStart = false;

  [SerializeField]
  private bool DEBUG_ChangeSceneStartLoad = true;

  [SerializeField]
  private bool DEBUG_UseCustomInitScenarios = false;

  [SerializeField]
  private List<SetScenarioData> DEBUG_ListInitializeScenario;

  [SerializeField]
  private GameTimeHandler.GameTimePeriod DEBUG_InitTimePeriod = GameTimeHandler.GameTimePeriod.Daytime;
#endif

  private LevelCheckpointDatabase _checkpoint_database;

  private GameTimeHandler _time_handler;

  private Dictionary<string, RuntimeSceneData> _scene_map = new();

  private HashSet<ILoadingQueue> _scene_loading_object_list = new HashSet<ILoadingQueue>();

  private ScenarioDiagramVS _scenario_diagram;
  internal ScenarioDiagramVS _ScenarioDiagram{get => _scenario_diagram;}

  private GameUIHandler _ui_handler;
  private InputFocusContext _input_context;

  private GameRuntimeData _runtime_data;

  private GameOverUI _game_over_ui;
  private FadeUI _game_over_fadeui;

  private LoadingUI _level_loading_ui;
  
  private FadeUI _fade_ui;


  private Dictionary<string, SceneContext> _scene_context_map = new();


  private string _current_scene;
  private GameContext _current_context;

  private string _last_scene;

  private Coroutine _trigger_pause_coroutine = null;
  private bool _trigger_pause_hide = false;

  private Coroutine _trigger_settings_coroutine = null;
  private bool _trigger_settings_hide = false;

  private bool _scene_initialized = false;
  public bool SceneInitialized{get => _scene_initialized;}

  private bool _scene_initializing = false;
  public bool SceneInitializing{get => _scene_initializing;}

  private bool _obj_initialized = false;
  public bool Initialized{get => _obj_initialized;}
  
  public PersistanceContext PersistanceHandler{private set; get;}


#if DEBUG
  private IEnumerator _set_custom_scenario(){
    yield return _scenario_diagram.ResetAllScenario();

    foreach(SetScenarioData _data in DEBUG_ListInitializeScenario){
      ScenarioHandlerVS _handler = _scenario_diagram.GetScenario(_data.ScenarioID);
      if(_handler == null){
        Debug.LogWarning(string.Format("DEBUG: Cannot get Scenario. (ID: {0})", _data.ScenarioID));
        continue;
      }

      _scenario_diagram.SetEnableScenario(_data.ScenarioID, true);

      StartCoroutine(_handler.SwitchSubScenario(_data.SubScenarioIdx, _data.TriggerEntrySequence));
    }
  }


  private IEnumerator _use_custom_scenario(){
    BaseLoadingQueue _load_queue = new(){
      LoadFlag = false
    };

    AddLoadingQueue(_load_queue);

    yield return new WaitUntil(() => _scene_initializing);

    if(DEBUG_UseCustomInitScenarios)
      yield return _set_custom_scenario();
    else
      yield return _reset_game_scenario();

    _time_handler.SetTimePeriod(DEBUG_InitTimePeriod);

    _load_queue.LoadFlag = true;
  }
#endif


  private IEnumerator _reset_game_scenario(){
    yield return _scenario_diagram.ResetAllScenario();

    yield return _scenario_diagram.StartScenario(DefaultScenarioID);
    _time_handler.SetTimePeriod(GameTimeHandler.GameTimePeriod.Daytime);
  }


  private IEnumerator _pause_co_func(){
    _trigger_pause_hide = false;

    var _player_ui_context = _ui_handler.GetPlayerModeContext();
    _ui_handler.SetPlayerUIMode(GameUIHandler.PlayerUIMode.Pausing, true);

    _time_handler.StopTime();
    yield return new WaitUntil(() => _trigger_pause_hide || !_scene_initialized);
    _time_handler.ResumeTime();

    _ui_handler.SetPlayerModeContext(_player_ui_context);
    _trigger_pause_coroutine = null;
  }


  private IEnumerator _settings_co_func(){
    _trigger_settings_hide = false;

    var _player_ui_context = _ui_handler.GetPlayerModeContext();
    _ui_handler.SetPlayerUIMode(GameUIHandler.PlayerUIMode.Setting, true);

    yield return new WaitUntil(() => _trigger_settings_hide || !_scene_initialized);

    _ui_handler.SetPlayerModeContext(_player_ui_context);
    _trigger_settings_coroutine = null;
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
    _scene_initializing = true;

    Debug.Log("[GameHandler] waiting for loading queue...");

    // tunggu sampai semua komponen sudah ter-inisialisasi
    yield return null;
    while(_scene_loading_object_list.Count > 0){
      yield return null;
      _check_loading_object();
    }

    Debug.Log("[GameHandler] loading queue empty.");
    
    // agar tidak banyak bug, biarkan physics untuk diproses sebelum even finish
    yield return null;
    yield return new WaitForEndOfFrame();

    if(!SplashScreen.isFinished){
      Debug.Log("Waiting for splashcreen");
      yield return new WaitUntil(() => SplashScreen.isFinished);
      Debug.Log("Waiting for splashscreen done.");
    }

    Debug.Log("[GameHandler] scene change finished.");
    _scene_initialized = true;

    SceneChangedFinishedEvent?.Invoke(_current_metadata.SceneID, _current_metadata.SceneContext);
  }


  private IEnumerator _change_scene(string scene_id, string teleport_to = "", bool do_save = true, bool clear_runtime_data = false){
    if(!_scene_initialized)
      yield break;

    if(!_scene_map.ContainsKey(scene_id)){
      Debug.LogWarning(string.Format("Cannot find Scene ID: '{0}'", scene_id));
      yield break;
    }

    _scene_initializing = false;
    _scene_initialized = false;
    SceneRemovingEvent?.Invoke();

    _time_handler.StopTime();

    Debug.Log(string.Format("[GameHandler] changing scene to: {0}", scene_id));
    _level_loading_ui.SetLoadingProgress(0);

    yield return UIUtility.SetHideUI(_level_loading_ui.gameObject, false, true);

    Debug.Log("[GameHandler] Input cleared");
    if(clear_runtime_data)
      _runtime_data.ClearData();

    ObjectReference.ClearReference();
    _input_context.ClearRegisters();

    RuntimeSceneData _metadata = _scene_map[scene_id];

    _last_scene = _current_scene;
    _current_scene = scene_id;
    _current_context = _metadata.Metadata.SceneContext;
    
    AsyncOperation _async_op = SceneManager.LoadSceneAsync(_metadata.SceneName);
    _level_loading_ui.BindAsyncOperation(_async_op);
    yield return new WaitUntil(() => _async_op.isDone);

    _checkpoint_database.UpdateDatabase();

    _time_handler.ResumeTime();

    Debug.Log(string.Format("[GameHandler] Teleporting to {0}", teleport_to));
    if(_metadata.Metadata.SceneContext == GameContext.InGame && teleport_to.Length > 0){
      DEBUGModeUtils.Log("check teleporting");
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

      Debug.Log(string.Format("[GameHandler] do teleport {0}", _do_teleport));
      if(_do_teleport){
        _handler.TeleportObject(_player.gameObject);

        yield return new WaitForNextFrameUnit();
        yield return new WaitForEndOfFrame();
      }
    }

    yield return _wait_scene_initialize(_metadata.Metadata);

    yield return new WaitUntil(() => _level_loading_ui.UIAnimationFinished);

    yield return UIUtility.SetHideUI(_level_loading_ui.gameObject, true);
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
    _time_handler.ResumeTime();

    yield return UIUtility.SetHideUI(_fade_ui.gameObject, false);

    PersistanceHandler.ReadSave();

    GameLevelContext _game_context = new();
    PersistanceHandler.OverwriteData(_game_context);

    SceneContextCollection _scene_contexts = new();
    PersistanceHandler.OverwriteData(_scene_contexts);

    _scene_context_map.Clear();
    foreach(SceneContext _context in _scene_contexts.SceneContexts){
      DEBUGModeUtils.Log(string.Format("scene context {0}", _context.SceneID));
      _scene_context_map[_context.SceneID] = _context;
    }

    ScenarioDiagramVS.PersistanceData _scenario_data = new();
    PersistanceHandler.OverwriteData(_scenario_data);
    _scenario_diagram.SetPersistanceData(_scenario_data);

    // load scene
    if(change_scene){
      string _teleport_id = "";
      if(_scene_context_map.ContainsKey(_game_context.CurrentSceneID)){
        SceneContext _scene_context = _scene_context_map[_game_context.CurrentSceneID]; 
        _teleport_id = _scene_context.LastCheckpointID;
      }

      Debug.Log(string.Format("[GameHandler] SceneID: {0}, TeleportID: {1}", _game_context.CurrentSceneID, _teleport_id));
      yield return _change_scene(_game_context.CurrentSceneID, _teleport_id, false, true);
    }
    
    LoadDataFromPersistanceEvent?.Invoke(PersistanceHandler);

    yield return UIUtility.SetHideUI(_fade_ui.gameObject, true);
  }


  private IEnumerator _quit_game(){
    yield return UIUtility.SetHideUI(_fade_ui.gameObject, false);

    Application.Quit();
  }

  
  private IEnumerator _save_game_co_func(){
    Debug.Log("Saving data...");
    
    _ui_handler.SetUtilityHUDUIMode(GameUIHandler.UtilityHUDUIEnum.SaveHintUI, true);
    
    PersistanceHandler.WriteSave();
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


  private IEnumerator _trigger_recipe_added(string recipe_item_id){
    RecipeBookUI _recipe_book_ui = _ui_handler.GetRecipeBookUI();
    bool _current_trigger = _recipe_book_ui.IsEffectTriggering;

    _recipe_book_ui.TriggerRecipeDiscoveryEffect(recipe_item_id);
    if(!_current_trigger){
      _input_context.RegisterInputObject(this, InputFocusContext.ContextEnum.Pause);

      var _context_data = _ui_handler.GetMainUIContext();
      _ui_handler.ResetMainUIMode();
      _ui_handler.SetMainHUDUIMode(GameUIHandler.MainHUDUIEnum.RecipeBookUI, true);

      yield return new WaitUntil(() => !_recipe_book_ui.IsEffectTriggering);

      _time_handler.ResumeTime();
      _ui_handler.SetMainUIContext(_context_data);

      _input_context.RemoveInputObject(this, InputFocusContext.ContextEnum.Pause);
    }
  }


  private void _create_runtime_scene_data(){
    string _path = "Assets/Resources/" + RuntimeSceneDataFile + ".json";
    FileStream _writer = File.Open(_path, FileMode.Create);

    RuntimeSceneDataWrapper _wrapper = new(){
      SceneDataList = _scene_map.Values.ToArray()
    };

    _writer.Write(Encoding.UTF8.GetBytes(JsonUtility.ToJson(_wrapper)));
    _writer.Close();
  }

  private void _load_runtime_scene_data(){
    TextAsset _runtime_scene = Resources.Load<TextAsset>(RuntimeSceneDataFile);

    RuntimeSceneDataWrapper _wrapper = new();
    JsonUtility.FromJsonOverwrite(_runtime_scene.text, _wrapper);

    _scene_map.Clear();
    foreach(RuntimeSceneData _scene_data in _wrapper.SceneDataList)
      _scene_map[_scene_data.Metadata.SceneID] = _scene_data;
  }


  public void Awake(){
#if UNITY_EDITOR
    foreach(var _metadata in _SceneMetadataList){
      _scene_map[_metadata.Metadata.SceneID] = new(){
        Metadata = _metadata.Metadata,
        SceneName = _metadata.SceneFile.name
      };
    }

    _create_runtime_scene_data();
#else

    _load_runtime_scene_data();
#endif
  }

  private IEnumerator _StartAsCoroutine(){
    Debug.Log("GameHandler init");

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

    // block any input while GameHandler initiating
    _input_context.RegisterInputObject(this, InputFocusContext.ContextEnum.Pause);

    _runtime_data = FindAnyObjectByType<GameRuntimeData>();
    if(_runtime_data == null){
      Debug.LogError("Cannot find GameRuntimeData.");
      throw new MissingReferenceException();
    }


    PersistanceHandler = GetComponent<PersistanceContext>();
    PersistanceHandler.PersistanceSavingEvent += _persistance_saving;

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

    _fade_ui = _ui_handler.GetUnscaledFadeUI();
    if(_fade_ui == null){
      Debug.LogError("FadeGU UI Object does not have FadeUI.");
      throw new MissingComponentException();
    }

    
    // tunggu sampai fade general usage UI sudah inisialisasi
    yield return new WaitForNextFrameUnit();
    yield return new WaitForEndOfFrame();

    yield return UIUtility.SetHideUI(_fade_ui.gameObject, false, true);

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

    // tunggu sampai start selanjutnya / menunggu semua objek inisialisasi
    yield return new WaitForNextFrameUnit();
    yield return new WaitForEndOfFrame();

    yield return new WaitUntil(() => _scenario_diagram.IsInitialized);


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
    _current_context = _metadata.SceneContext;

#if DEBUG
    if(DEBUG_LoadDataOnStart)
      StartCoroutine(_load_game_first_time());
    else
      StartCoroutine(_use_custom_scenario());
#else
    yield return _reset_game_scenario();
#endif

    yield return _wait_scene_initialize(_metadata);
    yield return UIUtility.SetHideUI(_fade_ui.gameObject, true);

    _obj_initialized = true;

    // unblock input
    _input_context.RemoveInputObject(this, InputFocusContext.ContextEnum.Pause);
  }

  public void Start(){
    UnityEngine.Random.InitState((int)DateTime.Now.ToFileTimeUtc());
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
    PersistanceHandler.ClearData();
    _runtime_data.ClearData();
    ResetGameScenario();

    _scene_context_map.Clear();
    foreach(SceneData _metadata in _SceneMetadataList)
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


  public void ChangeScene(string scene_id, string teleport_to = "", bool do_save = true){
    StartCoroutine(_change_scene(scene_id, teleport_to, do_save));
  }

  public void ChangeSceneToMainMenu(){
    ChangeScene(DefaultSceneID);
  }

  public string GetLastScene(){
    return _last_scene;
  }


  public void QuitGame(){
    StartCoroutine(_quit_game());
  }


  public void PauseGame(){
    if(_trigger_pause_coroutine != null || !_scene_initialized || _current_context != GameContext.InGame)
      return;
    
    _trigger_pause_coroutine = StartCoroutine(_pause_co_func());
  }

  public void ResumeGame(){
    _trigger_pause_hide = true;
  }


  public void OpenSettingsUI(){
    if(_trigger_settings_coroutine != null)
      return;

    _trigger_settings_coroutine = StartCoroutine(_settings_co_func());
  }

  public void CloseSettingsUI(){
    _trigger_settings_hide = true;
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



  // MARK: Game Triggers
  public void TriggerPlayerSpotted(){
    TriggerGameOver("Kamu Ketahuan!");
  }

  public void TriggerGameOver(string cause_text){
    _ui_handler.ResetMainUIMode();

    _ui_handler.ResetPlayerUIMode(true, new(){GameUIHandler.PlayerUIMode.GameOver});
    _ui_handler.SetPlayerUIMode(GameUIHandler.PlayerUIMode.GameOver, true);

    _time_handler.StopTime();

    _input_context.RegisterInputObject(_game_over_ui, InputFocusContext.ContextEnum.UI);
    _game_over_ui.SetCauseText(cause_text);
  }


  public void TriggerRecipeAdded(string recipe_item_id){
    StartCoroutine(_trigger_recipe_added(recipe_item_id));
  }


  // MARK: Input Handlings
  public void OnPauseGame(InputValue value){
    if(value.isPressed){
      // Unpause will be handled by PauseGameUI
      PauseGame();
    }
  }


  public void ResetGameScenario(){
    StartCoroutine(_reset_game_scenario());
  }
}