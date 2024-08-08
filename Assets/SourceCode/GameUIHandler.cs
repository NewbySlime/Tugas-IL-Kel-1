using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.Video;


/// <summary>
/// Helper class for storing all the UI objects used in the Game. All UI objects are added by hardcoding this class.
/// To see which UI are added to this class, see the variables with attribute <b>SerializeField</b>.
/// For other developer who wanted to add a UI, add a variable of this class, and modify some of the functions used in this class. To know which function to modify, see the context of the UI to add and the flow of the code to configure the UI.
/// For example, if a developer wanted to change or add to the Game's HUD, modify <see cref="SetMainHUDUIMode"/> and <see cref="ResetMainUIMode"/> and change the enumerator.
/// A hint, see "Dev Notes" in some comments for modify this class.
/// </summary>
public class GameUIHandler: MonoBehaviour{
  /// <summary>
  /// List of types in Game's general UI.
  /// Dev Note: Add an enum to add a UI.
  /// </summary>
  public enum PlayerUIMode{
    MainHUD,
    Pausing,
    Setting,
    GameOver
  }

  /// <summary>
  /// List of types in Game's HUD UI.
  /// Dev Note: Add an enum to add a UI.
  /// </summary>
  public enum MainHUDUIEnum{
    PlayerHUD,
    FadeOutUI,
    DialogueUI,
    BossHealthBarUI,
    QTEUI,
    VideoUI,
    CinematicBarUI,
    RecipeBookUI
  }

  /// <summary>
  /// List of types in Game's Utility or Miscellaneous UI.
  /// Dev Note: Add an enum to add a UI.
  /// </summary>
  public enum UtilityHUDUIEnum{
    SaveHintUI
  }

  /// <summary>
  /// Claaa for storing a list of context of the mode (is enabled or not) for certain generic type.
  /// </summary>
  /// <typeparam name="T">The generic type as the key/ID</typeparam>
  public class ModeContext<T>{
    public Dictionary<T, bool> ContextShowList = new();
  }


  [SerializeField]
  private PlayerHUDUI _PlayerHUD;
  [SerializeField]
  private FadeUI _FadeOutPlayerHUDUI;
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
    _VideoUI.Stop();
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


  // MARK: Get UI objects
  // Dev Note: Add a getter function to get an added UI object.

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


  /// <summary>
  /// Hide all UI.
  /// NOTE: this will only hide the target UI Container object, not actually the independent UI object.
  /// </summary>
  public void HideAllUI(){
    StartCoroutine(UIUtility.SetHideUI(_UIContainer, true));
  }

  /// <summary>
  /// Show all UI.
  /// NOTE: this will only show the target UI Container object, not actually the independent UI object.
  /// </summary>
  public void ShowAllUI(){
    StartCoroutine(UIUtility.SetHideUI(_UIContainer, false));
  }


  /// <summary>
  /// Set the mode (show or hide) of the UI of the Game's general UI.
  /// Dev Note: modify this function to also include the added UI object.
  /// </summary>
  /// <param name="mode">UI type to change</param>
  /// <param name="ui_show">Should it be shown or hidden</param>
  /// <param name="skip_animation">Should the animation used be skipped</param>
  public void SetPlayerUIMode(PlayerUIMode mode, bool ui_show, bool skip_animation = false){
    _player_ui_context[mode] = ui_show;

    GameObject _ui_obj = null;
    // Dev Note: modify this switch case to include the added UI object.
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

  /// <summary>
  /// Reset the mode (hide the object) of a UI of the Game's general UI.
  /// Dev Note: modify this function to also include the added UI object.
  /// </summary>
  /// <param name="skip_animation">Should the hidden animation be skipped</param>
  /// <param name="filter">Ignore filter to skip UI reset</param>
  public void ResetPlayerUIMode(bool skip_animation = false, HashSet<PlayerUIMode> filter = null){
    filter = filter == null? new(): filter;

    // Dev Note: modify this list to include the added UI object.
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


  /// <summary>
  /// To get mode context state for all UI of the Game's general UI.
  /// This function used alongside <see cref="SetPlayerModeContext"/> for restoring the configuration the UI in the stored state.
  /// </summary>
  /// <returns>The context data</returns>
  public ModeContext<PlayerUIMode> GetPlayerModeContext(){
    ModeContext<PlayerUIMode> _result = new();
    foreach(PlayerUIMode mode in _player_ui_context.Keys)
      _result.ContextShowList[mode] = _player_ui_context[mode];

    return _result;
  }

  /// <summary>
  /// Set the state of mode context of all UI of the Game's general UI.
  /// This function used alongside <see cref="GetPlayerModeContext"/> for memorizing the state of the UI used.
  /// </summary>
  /// <param name="context">The stored state of the related UI</param>
  /// <param name="skip_animation">Should the animation used when setting the configuration be skipped</param>
  public void SetPlayerModeContext(ModeContext<PlayerUIMode> context, bool skip_animation = false){
    foreach(var _context in context.ContextShowList)
      SetPlayerUIMode(_context.Key, _context.Value, skip_animation);
  }


  /// <summary>
  /// Set the mode (show or hide) of the UI of the Game's HUD UI.
  /// Dev Note: modify this function to also include the added UI object.
  /// </summary>
  /// <param name="mode">UI type to change</param>
  /// <param name="ui_show">Should it be shown or hidden</param>
  /// <param name="skip_animation">Should the animation used be skipped</param>
  public void SetMainHUDUIMode(MainHUDUIEnum mode, bool ui_show, bool skip_animation = false){
    _main_ui_context[mode] = ui_show;

    GameObject _ui_obj = null;
    // Dev Note: modify this switch case to include the added UI object.
    switch(mode){
      case MainHUDUIEnum.PlayerHUD:{
        _ui_obj = _PlayerHUD.GetVisualContainer();
      }break;

      case MainHUDUIEnum.FadeOutUI:{
        _ui_obj = _FadeOutPlayerHUDUI.gameObject;
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

  /// <summary>
  /// Reset the mode (hide the object) of a UI of the Game's HUD UI.
  /// Deb Note: modify this function to also include the added UI object.
  /// </summary>
  /// <param name="skip_animation">Should the hidden animation be skipped</param>
  /// <param name="filter">Ignore filter to skip UI reset</param>
  public void ResetMainUIMode(bool skip_animation = false, HashSet<MainHUDUIEnum> filter = null){
    filter = filter == null? new(): filter;

    // Dev Note: modify this list to include the added UI object.
    List<MainHUDUIEnum> _list_reset = new(){
      MainHUDUIEnum.PlayerHUD,
      MainHUDUIEnum.FadeOutUI,
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


  /// <summary>
  /// To get mode context state for all UI of the Game's HUD UI.
  /// This function used alongside <see cref="SetMainUIContext"/> for restoring the configuration of the UI in the stored state.
  /// </summary>
  /// <returns>The context data</returns>
  public ModeContext<MainHUDUIEnum> GetMainUIContext(){
    ModeContext<MainHUDUIEnum> _result = new();
    foreach(MainHUDUIEnum _enum in _main_ui_context.Keys)
      _result.ContextShowList[_enum] = _main_ui_context[_enum];

    return _result; 
  }

  /// <summary>
  /// Set the state of mode context of all UI of the Game's HUD UI.
  /// This funciton used alongside <see cref="GetPlayerModeContext"/> for memorizing the state of the UI used.
  /// </summary>
  /// <param name="context">The stored state of the related UI</param>
  /// <param name="skip_animation">Should the animation used when setting the configuration be skipped</param>
  public void SetMainUIContext(ModeContext<MainHUDUIEnum> context, bool skip_animation = false){
    foreach(MainHUDUIEnum _enum in context.ContextShowList.Keys)
      SetMainHUDUIMode(_enum, context.ContextShowList[_enum], skip_animation);
  }


  /// <summary>
  /// Set the mode (show or hide) of the UI of the Game's Utility/Miscellaneous UI
  /// Deb Note: modify this function to also include the added UI object.
  /// </summary>
  /// <param name="mode">UI type to change</param>
  /// <param name="ui_show">Should it be shown or hidden</param>
  /// <param name="skip_animation">Should the animation used be skipped</param>
  public void SetUtilityHUDUIMode(UtilityHUDUIEnum mode, bool ui_show, bool skip_animation = false){
    GameObject _ui_obj = null;
    // Deb Note: modify this switch case to include the added UI object.
    switch(mode){
      case UtilityHUDUIEnum.SaveHintUI:{
        _ui_obj = _SavingHintUI;
      }break;
    }

    StartCoroutine(UIUtility.SetHideUI(_ui_obj, !ui_show, skip_animation));
  }
}