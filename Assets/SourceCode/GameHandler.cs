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
/// <summary>
/// Class for handling Game's functions, events, and other objects/components that serves as the internal functions.
/// 
/// This class uses following component(s);
/// - <see cref="PersistanceContext"/> for handling file save for a game state in certain time.
/// 
/// This class uses external component(s);
/// - Object for containing <see cref="ScenarioHandlerVS"/>.
/// - <see cref="LevelCheckpointDatabase"/> for storing data about all checkpoint in a scene.
/// - <see cref="GameTimeHandler"/> for setting up the Game's time.
/// - <see cref="ScenarioDiagramVS"/> for handling Game's scenario or parts of the Game's story.
/// - <see cref="GameUIHandler"/> for getting all UIs in the Game.
/// - <see cref="InputFocusContext"/> for handling the Game's input.
/// - <see cref="GameRuntimeData"/> for storing data about the Game in runtime for use inbetween scenes.
/// - <see cref="GameOverUI"/>
/// - <see cref="FadeUI"/> UI for covering up the screen.
/// - <see cref="LoadingUI"/>
/// 
/// <seealso cref="GameUIHandler"/>
/// <seealso cref="GameTimeHandler"/>
/// <seealso cref="GameRuntimeData"/>
/// </summary>
public class GameHandler: MonoBehaviour{
  /// <summary>
  /// Path to file containing the scene data for build.
  /// Data contained in here are automatically created in function <see cref="_load_runtime_scene_data"/> whenever this class are initiated in debug mode.
  /// </summary>
  public const string RuntimeSceneDataFile = "GameConfigs/RuntimeSceneData";


  /// <summary>
  /// ID for entry point scene that also serves as a main menu.
  /// </summary>
  public const string DefaultSceneID = "main_menu_scene";

  /// <summary>
  /// ID for entry point scene for starting a new game.
  /// </summary>
  public const string DefaultGameSceneID = "intro_game_scene";

  /// <summary>
  /// ID for entry point scenario.
  /// </summary>
  public const string DefaultScenarioID = "intro_scenario";


  /// <summary>
  /// Event for when a scene is initializing.
  /// </summary>
  public event SceneChangedInitializing SceneChangedInitializingEvent;
  public delegate void SceneChangedInitializing(string scene_id, GameContext context);

  /// <summary>
  /// Event for when a scene has changed and finished initializing. 
  /// </summary>
  public event SceneChangedFinished SceneChangedFinishedEvent;
  public delegate void SceneChangedFinished(string scene_id, GameContext context);

  /// <summary>
  /// Event for when prompted to change scene but still preparing (not yet changed).
  /// </summary>
  public event SceneRemoving SceneRemovingEvent;
  public delegate void SceneRemoving();

  /// <summary>
  /// Event for when a save file is being loaded from attached persistance in this object.
  /// </summary>
  public event LoadDataFromPersistance LoadDataFromPersistanceEvent;
  public delegate void LoadDataFromPersistance(PersistanceContext context);


  /// <summary>
  /// Tells the current context of the scene.
  /// </summary>
  public enum GameContext{
    InGame,
    MainMenu
  }


  [Serializable]
  /// <summary>
  /// Metadata about a scene attached.
  /// </summary>
  public class SceneMetadata{
    public string SceneID;
    public GameContext SceneContext; 
  }

  [Serializable]
  /// <summary>
  /// (ONLY USE IN DEBUG MODE) Data about a certain scene. For build mode counterpart of this see <see cref="RuntimeSceneData"/>.
  /// </summary>
  public class SceneData{
    /// <summary>
    /// Scene's metadata, stuff like identifier and the context of it.
    /// </summary>
    public SceneMetadata Metadata;

    /// <summary>
    /// The Unity's Scene file.
    /// </summary>
    public UnityEngine.Object SceneFile;
  }


  [Serializable]
  /// <summary>
  /// Scene data for build mode. Loaded in <see cref="_load_runtime_scene_data"/> function.
  /// </summary>
  public class RuntimeSceneData{
    /// <inheritdoc cref="SceneData.Metadata"/>
    public SceneMetadata Metadata;

    /// <summary>
    /// Scene identifier used by Unity's Scene handling system.
    /// </summary>
    public string SceneName;
  }

  [Serializable]
  /// <summary>
  /// Wrapper to store multiple <see cref="RuntimeSceneData"/> with <b>Serializeable</b> attribute.
  /// </summary>
  public class RuntimeSceneDataWrapper{
    public RuntimeSceneData[] SceneDataList = new RuntimeSceneData[0];
  }


  [Serializable]
  /// <summary>
  /// Current context/state of certain scene. This class can also be used for saving data.
  /// </summary>
  private class SceneContext: PersistanceContext.IPersistance{
    public string SceneID;

    /// <summary>
    /// Which checkpoint the Player pass through in a scene.
    /// </summary>
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
  /// <summary>
  /// Wrapper to store multiple <see cref="SceneContext"/> with <see cref="Serializeable"/> attribute. This class can also be used for saving data.
  /// </summary>
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
  /// <summary>
  /// Data about the current state of the Game.
  /// </summary>
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
  /// <summary>
  /// Data used for instantly configure a scenario. Only for debugging mode.
  /// </summary>
  private struct SetScenarioData{
    public string ScenarioID;
    public int SubScenarioIdx;

    public bool TriggerEntrySequence;
  }
#endif


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
  // Only useable if DEBUG_UseCustomInitScenarios is enabled.
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
  /// <summary>
  /// Flag if current scene already initialized or not yet.
  /// </summary>
  public bool SceneInitialized{get => _scene_initialized;}

  private bool _scene_initializing = false;
  /// <summary>
  /// Flag if current scene is still initilizing.
  /// </summary>
  public bool SceneInitializing{get => _scene_initializing;}

  private bool _obj_initialized = false;
  /// <summary>
  /// Flag if this object has been initialized or not.
  /// </summary>
  public bool Initialized{get => _obj_initialized;}
  
  /// <summary>
  /// Persistance handler used by this <see cref="GameHandler"/>.
  /// </summary>
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


  // Function for blocking scene initializing process to embed the custom scenario. Which then uses _set_custom_scenario() function to actually configure the custom scenario.
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

  // Resets Game's scenario to default state.
  private IEnumerator _reset_game_scenario(){
    yield return _scenario_diagram.ResetAllScenario();

    yield return _scenario_diagram.StartScenario(DefaultScenarioID);
    _time_handler.SetTimePeriod(GameTimeHandler.GameTimePeriod.Daytime);
  }


  // Function for handling to show and hide Pause UI based on the interaction.
  // This coroutine function will be finished (when Pause UI hidden) when _trigger_pause_hide flag is enabled.
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


  // Function for handling to show and hide Settings UI based on the interaction.
  // This coroutine function will be finished (when Settings UI hidden) when _trigger_settings_hide flag is enabled.
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


  // Coroutine for triggering save file in PersistanceContext  
  private IEnumerator _save_game_co_func(){
    Debug.Log("Saving data...");
    
    _ui_handler.SetUtilityHUDUIMode(GameUIHandler.UtilityHUDUIEnum.SaveHintUI, true);
    
    PersistanceHandler.WriteSave();
    yield return new WaitForSeconds(1);

    _ui_handler.SetUtilityHUDUIMode(GameUIHandler.UtilityHUDUIEnum.SaveHintUI, false);
  }

  // Event for catching saving event from PersistanceContext
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


  // Function to trigger animation for a recipe has been added to Player's catalogue
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
  /// <summary>
  /// Load a save file that saved previous state of the Game.
  /// </summary>
  /// <param name="change_scene">Should loading only load the data or also change the scene</param>
  public void LoadGame(bool change_scene = true){
    StartCoroutine(_load_game(change_scene));
#else
  /// <summary>
  /// Load a save file that saved previous state of the Game.
  /// </summary>
  public void LoadGame(){
    StartCoroutine(_load_game(true));
#endif
  }

  /// <summary>
  /// To reset Game's data and variables and start a new Game.
  /// </summary>
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


  /// <summary>
  /// Trigger a Game Save event.
  /// If an object wants to store a data to save file, subscribe to <see cref="PersistanceContext.PersistanceSavingEvent"/> in <see cref="PersistanceContext"/> attached to this object.
  /// </summary>
  public void SaveGame(){
    StartCoroutine(_save_game_co_func());
  }


  /// <summary>
  /// Restart the Game from the last saved Game state.
  /// </summary>
  public void RestartFromLastCheckpoint(){
    LoadGame();
  }

  /// <summary>
  /// Set the last checkpoint that the player entered last time.
  /// </summary>
  /// <param name="checkpoint_id">The target checkpoint ID</param>
  public void SetLastCheckpoint(string checkpoint_id){
    if(!_scene_context_map.ContainsKey(_current_scene))
      _scene_context_map[_current_scene] = new SceneContext{
        SceneID = _current_scene
      };

    SceneContext _current_context = _scene_context_map[_current_scene];
    _current_context.LastCheckpointID = checkpoint_id;
  }


  /// <summary>
  /// Change Game's scene to a new scene.
  /// This does not automatically teleport the Player, instead it uses checkpoint (teleport) ID from the previous scene to determine where to teleport to.
  /// <seealso cref="LevelTeleportHandler"/>
  /// </summary>
  /// <param name="scene_id">The scene to change to</param>
  /// <param name="teleport_to">Force teleport to a checkpoint (teleport) by ID</param>
  /// <param name="do_save">Force save after changing scene</param>
  public void ChangeScene(string scene_id, string teleport_to = "", bool do_save = true){
    StartCoroutine(_change_scene(scene_id, teleport_to, do_save));
  }

  /// <summary>
  /// Change Game's scene to main menu scene. This uses <see cref="ChangeScene"/> function with hardcoded scene ID. 
  /// </summary>
  public void ChangeSceneToMainMenu(){
    ChangeScene(DefaultSceneID);
  }

  /// <summary>
  /// Get scene ID of the previous scene.
  /// </summary>
  /// <returns>The scene ID</returns>
  public string GetLastScene(){
    return _last_scene;
  }


  /// <summary>
  /// Prompt the Game to quit the Game program.
  /// </summary>
  public void QuitGame(){
    StartCoroutine(_quit_game());
  }


  /// <summary>
  /// Prompt to pause the Game. It will show the UI for pausing, and then when closed by this object or by the UI presented, the Game will be resumed. 
  /// </summary>
  public void PauseGame(){
    if(_trigger_pause_coroutine != null || !_scene_initialized || _current_context != GameContext.InGame)
      return;
    
    _trigger_pause_coroutine = StartCoroutine(_pause_co_func());
  }

  /// <summary>
  /// Resume the paused Game that have been paused by <see cref="PauseGame"/>. This will hide the UI for pausing.
  /// </summary>
  public void ResumeGame(){
    _trigger_pause_hide = true;
  }


  /// <summary>
  /// Show the UI for settings. Hide the UI by using <see cref="CloseSettingsUI"/>.
  /// </summary>
  public void OpenSettingsUI(){
    if(_trigger_settings_coroutine != null)
      return;

    _trigger_settings_coroutine = StartCoroutine(_settings_co_func());
  }

  /// <summary>
  /// Hide the settings UI. To show the UI, see <see cref="OpenSettingsUI"/>.
  /// </summary>
  public void CloseSettingsUI(){
    _trigger_settings_hide = true;
  }



  /// <summary>
  /// Add queue for blocking the scene initializing progress. This is used for signaling this object to wait until all objects in queue is loaded.
  /// </summary>
  /// <param name="queue">Loading interface object</param>
  public void AddLoadingQueue(ILoadingQueue queue){
    _scene_loading_object_list.Add(queue);
  }


  /// <summary>
  /// Get scene ID for current scene.
  /// </summary>
  /// <returns>The current scene ID</returns>
  public string GetCurrentSceneID(){
    return _current_scene;
  }

  /// <summary>
  /// Get scene context for current scene.
  /// </summary>
  /// <returns>The scene context</returns>
  public GameContext GetCurrentSceneContext(){
    if(!_scene_map.ContainsKey(_current_scene)){
      Debug.LogWarning(string.Format("No Metadata found for Scene: '{0}'", _current_scene));
      return GameContext.InGame;
    }

    return _scene_map[_current_scene].Metadata.SceneContext;
  }


  // MARK: Game Triggers

  /// <summary>
  /// Trigger Game Over when player is spotted. This uses <see cref="TriggerGameOver"/> using hardcoded parameters.
  /// </summary>
  public void TriggerPlayerSpotted(){
    TriggerGameOver("Kamu Ketahuan!");
  }

  /// <summary>
  /// Trigger a Game Over event.
  /// </summary>
  /// <param name="cause_text">The cause of the Game Over event</param>
  public void TriggerGameOver(string cause_text){
    _ui_handler.ResetMainUIMode();

    _ui_handler.ResetPlayerUIMode(true, new(){GameUIHandler.PlayerUIMode.GameOver});
    _ui_handler.SetPlayerUIMode(GameUIHandler.PlayerUIMode.GameOver, true);

    _time_handler.StopTime();

    _input_context.RegisterInputObject(_game_over_ui, InputFocusContext.ContextEnum.UI);
    _game_over_ui.SetCauseText(cause_text);
  }


  /// <summary>
  /// Start an animation for when a recipe is added to player catalogue. Even with the animation is playing, this function can still be used but it will be queued for the next animation.
  /// </summary>
  /// <param name="recipe_item_id">The recipe ID added to player</param>
  public void TriggerRecipeAdded(string recipe_item_id){
    StartCoroutine(_trigger_recipe_added(recipe_item_id));
  }


  // MARK: Input Handlings

  /// <summary>
  /// Function to catch "PauseGame" input event. Used for redirecting it to <see cref="PauseGame"/>.
  /// </summary>
  /// <param name="value">Unity's input data</param>
  public void OnPauseGame(InputValue value){
    if(value.isPressed){
      // Unpause will be handled by PauseGameUI
      PauseGame();
    }
  }


  /// <summary>
  /// Resetting the Game's scenario to default scenario.
  /// </summary>
  public void ResetGameScenario(){
    StartCoroutine(_reset_game_scenario());
  }
}