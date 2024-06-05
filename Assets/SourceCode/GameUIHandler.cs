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
    GameOver
  }

  public enum MainHUDUIEnum{
    PlayerHUD,
    DialogueUI,
    QTEUI,
    VideoUI
  }

  public enum UtilityHUDUIEnum{
    SaveHintUI
  }


  [SerializeField]
  private PlayerHUDUI _PlayerHUD;
  [SerializeField]
  private DialogueCharacterUI _DialogueHUD;
  [SerializeField]
  private VideoPlayer _VideoUI;
  [SerializeField]
  private QuickTimeEventUI _QTEUI;

  [SerializeField]
  private GameObject _SavingHintUI;

  [SerializeField]
  private GameObject _MainHUDParent;
  [SerializeField]
  private GameObject _PausingHUDParent;
  [SerializeField]
  private GameObject _GameOverHUDParent;
  [SerializeField]
  private GameObject _UIContainer;

  [SerializeField]
  private LoadingUI _LevelLoadingUI;

  [SerializeField]
  private GameOverUI _GameOverUI;

  [SerializeField]
  private FadeUI _GeneralUsageFadeUI;


  private PlayerUIMode _current_player_mode;

  private GameHandler _game_handler;
  private InputFocusContext _input_context;


  private IEnumerator _set_hide_ui(GameObject ui_obj, bool ui_hide, bool skip_animation = false){
    FadeUI _fadeui = ui_obj.GetComponent<FadeUI>();
    if(_fadeui != null){
      _fadeui.FadeToCover = !ui_hide;
    }

    SlideUI _slideui = ui_obj.GetComponent<SlideUI>();
    if(_slideui != null){
      _slideui.ShowAnimation = !ui_hide;
    }

    SetActiveUIOnTimeout _set_active_ui = ui_obj.GetComponent<SetActiveUIOnTimeout>();
    if(_set_active_ui != null){
      _set_active_ui.SetActiveTarget = !ui_hide;
    }

    TimingBaseUI.SkipAllTimer(ui_obj);
    yield return TimingBaseUI.StartAllTimer(ui_obj, skip_animation);
  }


  private void _game_handler_scene_changed(string scene_id, GameHandler.GameContext context){
    switch(context){
      case GameHandler.GameContext.InGame:{
        SetPlayerUIMode(PlayerUIMode.MainHUD);
        SetMainHUDUIMode(MainHUDUIEnum.PlayerHUD, true);
      }break;
    }
  }

  private void _game_handler_scene_removed(){
    ResetPlayerUIMode();
    ResetMainUIMode();
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


    yield return new WaitForNextFrameUnit();
    yield return new WaitForEndOfFrame();

    SetPlayerUIMode(PlayerUIMode.MainHUD);

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

  public GameOverUI GetGameOverUI(){
    return _GameOverUI;
  }

  public DialogueCharacterUI GetDialogueHUDUI(){
    return _DialogueHUD;
  }

  public PlayerHUDUI GetPlayerHUDUI(){
    return _PlayerHUD;
  }

  public VideoPlayer GetVideoPlayerUI(){
    return _VideoUI;
  }

  public QuickTimeEventUI GetQTEUI(){
    return _QTEUI;
  }


  public void HideAllUI(){
    StartCoroutine(_set_hide_ui(_UIContainer, true));
  }

  public void ShowAllUI(){
    StartCoroutine(_set_hide_ui(_UIContainer, false));
  }


  public void SetPlayerUIMode(PlayerUIMode mode, bool skip_animation = false){
    ResetPlayerUIMode(false, new HashSet<PlayerUIMode>{mode});

    GameObject _ui_obj = null;
    switch(mode){
      case PlayerUIMode.MainHUD:{
        _ui_obj = _MainHUDParent;
      }break;

      case PlayerUIMode.Pausing:{
        _ui_obj = _PausingHUDParent;
      }break;

      case PlayerUIMode.GameOver:{
        _ui_obj = _GameOverHUDParent;
      }break;
    }

    StartCoroutine(_set_hide_ui(_ui_obj, false));
  }

  public void ResetPlayerUIMode(bool skip_animation = false, HashSet<PlayerUIMode> filter = null){
    filter = filter == null? new(): filter;

    var _player_ui_list = new List<KeyValuePair<GameObject, PlayerUIMode>>{
      new (_MainHUDParent, PlayerUIMode.MainHUD),
      new (_PausingHUDParent, PlayerUIMode.Pausing),
      new (_GameOverHUDParent, PlayerUIMode.GameOver)
    };

    foreach(var _player_pair in _player_ui_list){
      if(filter.Contains(_player_pair.Value))
        continue;

      StartCoroutine(_set_hide_ui(_player_pair.Key, true, skip_animation));
    }
  }


  public void SetMainHUDUIMode(MainHUDUIEnum mode, bool ui_show, bool skip_animation = false){
    GameObject _ui_obj = null;
    switch(mode){
      case MainHUDUIEnum.PlayerHUD:{
        _ui_obj = _PlayerHUD.GetVisualContainer();
      }break;

      case MainHUDUIEnum.DialogueUI:{
        _ui_obj = _DialogueHUD.gameObject;
      }break;

      case MainHUDUIEnum.QTEUI:{
        _ui_obj = _QTEUI.gameObject;
      }break;

      case MainHUDUIEnum.VideoUI:{
        _ui_obj = _VideoUI.gameObject;
      }break;
    }

    StartCoroutine(_set_hide_ui(_ui_obj, !ui_show, skip_animation));
  }

  public void ResetMainUIMode(bool skip_animation = false, HashSet<MainHUDUIEnum> filter = null){
    filter = filter == null? new(): filter;

    List<MainHUDUIEnum> _list_reset = new(){
      MainHUDUIEnum.PlayerHUD,
      MainHUDUIEnum.DialogueUI,
      MainHUDUIEnum.QTEUI,
      MainHUDUIEnum.VideoUI
    };

    foreach(MainHUDUIEnum _reset in _list_reset){
      if(filter.Contains(_reset))
        continue;

      SetMainHUDUIMode(_reset, false, skip_animation);
    }
  }


  public void SetUtilityHUDUIMode(UtilityHUDUIEnum mode, bool ui_show, bool skip_animation = false){
    GameObject _ui_obj = null;
    switch(mode){
      case UtilityHUDUIEnum.SaveHintUI:{
        _ui_obj = _SavingHintUI;
      }break;
    }

    StartCoroutine(_set_hide_ui(_ui_obj, !ui_show, skip_animation));
  }


  public PlayerUIMode GetPlayerUIMode(){
    return _current_player_mode;
  }
}