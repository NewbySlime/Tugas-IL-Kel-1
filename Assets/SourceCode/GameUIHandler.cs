using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using JetBrains.Annotations;
using Unity.VisualScripting;
using Unity.VisualScripting.Generated.PropertyProviders;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.Video;


public class GameUIHandler: MonoBehaviour{
  public enum PlayerUIMode{
    MainHUD,
    Pausing,
    Setting,
    GameOver
  }

  public enum MainHUDUIEnum{
    PlayerHUD,
    DialogueUI,
    BossHealthBarUI,
    QTEUI,
    VideoUI,
    CinematicBarUI,
    RecipeBookUI
  }

  public enum UtilityHUDUIEnum{
    SaveHintUI
  }

  public class ModeContext<T>{
    public Dictionary<T, bool> ContextShowList = new();
  }


  [SerializeField]
  private PlayerHUDUI _PlayerHUD;
  [SerializeField]
  private DialogueCharacterUI _DialogueHUD;
  [SerializeField]
  private BossHealthBarUI _BossHealthBarUI;
  [SerializeField]
  private VideoPlayer _VideoUI;
  [SerializeField]
  private QuickTimeEventUI _QTEUI;
  [SerializeField]
  private GameObject _CinematicBarUI;
  [SerializeField]
  private RecipeBookUI _RecipeBookUI;

  [SerializeField]
  private GameObject _UIContainer;

  [SerializeField]
  private GameObject _MainHUDParent;
  [SerializeField]
  private PauseGameUI _PausingUI;
  [SerializeField]
  private SettingsUI _SettingsUI;
  [SerializeField]
  private GameOverUI _GameOverUI;

  [SerializeField]
  private LoadingUI _LevelLoadingUI;

  [SerializeField]
  private FadeUI _GeneralUsageFadeUI;
  [SerializeField]
  private FadeUI _UnscaledFadeUI;

  [SerializeField]
  private GameObject _SavingHintUI;

  private PlayerUIMode _current_player_mode;

  private GameHandler _game_handler;
  private InputFocusContext _input_context;

  private Dictionary<MainHUDUIEnum, bool> _main_ui_context = new();
  private Dictionary<PlayerUIMode, bool> _player_ui_context = new();


  private void _game_handler_scene_changed(string scene_id, GameHandler.GameContext context){
    switch(context){
      case GameHandler.GameContext.InGame:{
        ResetPlayerUIMode(true, new(){PlayerUIMode.MainHUD});
        SetPlayerUIMode(PlayerUIMode.MainHUD, true);

        ResetMainUIMode(true, new(){MainHUDUIEnum.PlayerHUD});
        SetMainHUDUIMode(MainHUDUIEnum.PlayerHUD, true);
      }break;

      case GameHandler.GameContext.MainMenu:{
        ResetMainUIMode(true);
        ResetPlayerUIMode(true);
      }break;
    }
  }

  private void _game_handler_scene_removed(){
    // ResetPlayerUIMode();
    // ResetMainUIMode();
  }


  private IEnumerator _StartAsCoroutine(){
    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot get GameHandler.");
      throw new MissingReferenceException();
    }

    _game_handler.SceneChangedFinishedEvent += _game_handler_scene_changed;
    _game_handler.SceneRemovingEvent += _game_handler_scene_removed;


    _input_context = FindAnyObjectByType<InputFocusContext>();
    if(_input_context == null){
      Debug.LogError("Cannot get InputFocusContext.");
      throw new MissingReferenceException();
    }


    yield return null;
    yield return new WaitForEndOfFrame();

    yield return UIUtility.SetHideUI(_GeneralUsageFadeUI.gameObject, true, true);
    // UnscaledFadeUI used by GameHandler
    yield return UIUtility.SetHideUI(_LevelLoadingUI.gameObject, true, true);

    ResetMainUIMode(true);
    ResetPlayerUIMode(true);
    SetPlayerUIMode(PlayerUIMode.MainHUD, true);

    if(_game_handler.SceneInitialized)
      _game_handler_scene_changed(_game_handler.GetCurrentSceneID(), _game_handler.GetCurrentSceneContext());
  }


  public void Start(){
    StartCoroutine(_StartAsCoroutine());
  }


  public LoadingUI GetLevelLoadingUI(){
    return _LevelLoadingUI;
  }

  public FadeUI GetGeneralFadeUI(){
    return _GeneralUsageFadeUI;
  }

  public FadeUI GetUnscaledFadeUI(){
    return _UnscaledFadeUI;
  }

  public GameOverUI GetGameOverUI(){
    return _GameOverUI;
  }

  public DialogueCharacterUI GetDialogueHUDUI(){
    return _DialogueHUD;
  }

  public PlayerHUDUI GetPlayerHUDUI(){
    return _PlayerHUD;
  }

  public BossHealthBarUI GetBossHealthBarUI(){
    return _BossHealthBarUI;
  }

  public VideoPlayer GetVideoPlayerUI(){
    return _VideoUI;
  }

  public QuickTimeEventUI GetQTEUI(){
    return _QTEUI;
  }

  public RecipeBookUI GetRecipeBookUI(){
    return _RecipeBookUI;
  }


  public void HideAllUI(){
    StartCoroutine(UIUtility.SetHideUI(_UIContainer, true));
  }

  public void ShowAllUI(){
    StartCoroutine(UIUtility.SetHideUI(_UIContainer, false));
  }


  public void SetPlayerUIMode(PlayerUIMode mode, bool ui_show, bool skip_animation = false){
    _player_ui_context[mode] = ui_show;

    GameObject _ui_obj = null;
    switch(mode){
      case PlayerUIMode.MainHUD:{
        _ui_obj = _MainHUDParent;
      }break;

      case PlayerUIMode.Pausing:{
        _ui_obj = _PausingUI.gameObject;
      }break;

      case PlayerUIMode.Setting:{
        _ui_obj = _SettingsUI.gameObject;
      }break;

      case PlayerUIMode.GameOver:{
        _ui_obj = _GameOverUI.gameObject;
      }break;
    }

    StartCoroutine(UIUtility.SetHideUI(_ui_obj, !ui_show));
  }

  public void ResetPlayerUIMode(bool skip_animation = false, HashSet<PlayerUIMode> filter = null){
    filter = filter == null? new(): filter;

    var _player_ui_list = new List<KeyValuePair<GameObject, PlayerUIMode>>{
      new (_MainHUDParent, PlayerUIMode.MainHUD),
      new (_PausingUI.gameObject, PlayerUIMode.Pausing),
      new (_SettingsUI.gameObject, PlayerUIMode.Setting),
      new (_GameOverUI.gameObject, PlayerUIMode.GameOver)
    };

    foreach(var _player_pair in _player_ui_list){
      if(filter.Contains(_player_pair.Value))
        continue;

      SetPlayerUIMode(_player_pair.Value, false, skip_animation);
    }
  }


  public ModeContext<PlayerUIMode> GetPlayerModeContext(){
    ModeContext<PlayerUIMode> _result = new();
    foreach(PlayerUIMode mode in _player_ui_context.Keys)
      _result.ContextShowList[mode] = _player_ui_context[mode];

    return _result;
  }

  public void SetPlayerModeContext(ModeContext<PlayerUIMode> context, bool skip_animation = false){
    foreach(var _context in context.ContextShowList)
      SetPlayerUIMode(_context.Key, _context.Value, skip_animation);
  }


  public void SetMainHUDUIMode(MainHUDUIEnum mode, bool ui_show, bool skip_animation = false){
    _main_ui_context[mode] = ui_show;

    GameObject _ui_obj = null;
    switch(mode){
      case MainHUDUIEnum.PlayerHUD:{
        _ui_obj = _PlayerHUD.GetVisualContainer();
      }break;

      case MainHUDUIEnum.DialogueUI:{
        _ui_obj = _DialogueHUD.gameObject;
      }break;

      case MainHUDUIEnum.BossHealthBarUI:{
        _ui_obj = _BossHealthBarUI.gameObject;
      }break;

      case MainHUDUIEnum.QTEUI:{
        _ui_obj = _QTEUI.gameObject;
      }break;

      case MainHUDUIEnum.VideoUI:{
        _ui_obj = _VideoUI.gameObject;
      }break;

      case MainHUDUIEnum.CinematicBarUI:{
        _ui_obj = _CinematicBarUI;
      }break;

      case MainHUDUIEnum.RecipeBookUI:{
        _ui_obj = _RecipeBookUI.gameObject;
      }break;
    }

    StartCoroutine(UIUtility.SetHideUI(_ui_obj, !ui_show, skip_animation));
  }

  public void ResetMainUIMode(bool skip_animation = false, HashSet<MainHUDUIEnum> filter = null){
    filter = filter == null? new(): filter;

    List<MainHUDUIEnum> _list_reset = new(){
      MainHUDUIEnum.PlayerHUD,
      MainHUDUIEnum.DialogueUI,
      MainHUDUIEnum.BossHealthBarUI,
      MainHUDUIEnum.QTEUI,
      MainHUDUIEnum.VideoUI,
      MainHUDUIEnum.CinematicBarUI,
      MainHUDUIEnum.RecipeBookUI
    };

    foreach(MainHUDUIEnum _reset in _list_reset){
      if(filter.Contains(_reset))
        continue;

      SetMainHUDUIMode(_reset, false, skip_animation);
    }
  }


  public ModeContext<MainHUDUIEnum> GetMainUIContext(){
    ModeContext<MainHUDUIEnum> _result = new();
    foreach(MainHUDUIEnum _enum in _main_ui_context.Keys)
      _result.ContextShowList[_enum] = _main_ui_context[_enum];

    return _result; 
  }

  public void SetMainUIContext(ModeContext<MainHUDUIEnum> context, bool skip_animation = false){
    foreach(MainHUDUIEnum _enum in context.ContextShowList.Keys)
      SetMainHUDUIMode(_enum, context.ContextShowList[_enum], skip_animation);
  }


  public void SetUtilityHUDUIMode(UtilityHUDUIEnum mode, bool ui_show, bool skip_animation = false){
    GameObject _ui_obj = null;
    switch(mode){
      case UtilityHUDUIEnum.SaveHintUI:{
        _ui_obj = _SavingHintUI;
      }break;
    }

    StartCoroutine(UIUtility.SetHideUI(_ui_obj, !ui_show, skip_animation));
  }


  public PlayerUIMode GetPlayerUIMode(){
    return _current_player_mode;
  }
}