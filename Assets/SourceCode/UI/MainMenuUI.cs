using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MainMenuUI: MonoBehaviour{
  [Serializable]
  public struct PromptData{
    public PromptContext Context;
    public string PromptText;
  }

  public enum PromptContext{
    StartNewGame
  }

  [SerializeField]
  private ButtonBaseUI _LoadGameButton;
  [SerializeField]
  private GameObject _LoadGameButtonActiveTarget;

  [SerializeField]
  private PromptUI _PromptUI;

  [SerializeField]
  private ButtonBaseUI _StartNewGameButton;
  [SerializeField]
  private ButtonBaseUI _OpenSettingsButton;
  [SerializeField]
  private ButtonBaseUI _ExitGameButton;

  [SerializeField]
  private List<PromptData> _PromptDataList;

  private GameHandler _game_handler;

  private Dictionary<PromptContext, PromptData> _prompt_data_map = new();
  private PromptContext _current_prompt_context;



  private void _on_prompt_accept(){
    switch(_current_prompt_context){
      case PromptContext.StartNewGame:{
        _game_handler.StartNewGame();
      }break;

      default:{
        Debug.LogError(string.Format("PromptContext ({0}) is not yet supported.", _current_prompt_context));
        StartCoroutine(UIUtility.SetHideUI(_PromptUI.gameObject, true));
      }break;
    }
  }

  private void _on_prompt_cancel(){
    StartCoroutine(UIUtility.SetHideUI(_PromptUI.gameObject, true));
  }


  private void _trigger_prompt(PromptContext context){
    PromptData _data = new(){
      PromptText = "Error: Empty Prompt"
    };

    if(_prompt_data_map.ContainsKey(context))
      _data = _prompt_data_map[context];

    _current_prompt_context = context;
    _PromptUI.SetPromptText(_data.PromptText);

    StartCoroutine(UIUtility.SetHideUI(_PromptUI.gameObject, false));
  }


  private void _on_load_game_button(){
    _game_handler.LoadGame();
  }

  private void _on_start_game_button(){
    if(_game_handler.PersistanceHandler.IsSaveValid()){
      _trigger_prompt(PromptContext.StartNewGame);
      return;
    }

    _game_handler.StartNewGame();
  }

  private void _on_open_settings_button(){
    _game_handler.OpenSettingsUI();
  }

  private void _on_quit_button(){
    _game_handler.QuitGame();
  }


  private IEnumerator _start_as_coroutine(){
    yield return null;
    yield return new WaitForEndOfFrame();

    yield return UIUtility.SetHideUI(_LoadGameButtonActiveTarget, !_game_handler.PersistanceHandler.IsSaveValid(), true);
  }

  public void Start(){
    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingReferenceException();
    }

    foreach(PromptData _data in _PromptDataList)
      _prompt_data_map[_data.Context] = _data;

    _PromptUI.OnPromptAcceptEvent += _on_prompt_accept;
    _PromptUI.OnPromptCancelEvent += _on_prompt_cancel;

    _StartNewGameButton.OnButtonReleasedEvent += _on_start_game_button;
    _LoadGameButton.OnButtonReleasedEvent += _on_load_game_button;
    _OpenSettingsButton.OnButtonReleasedEvent += _on_open_settings_button;
    _ExitGameButton.OnButtonReleasedEvent += _on_quit_button;

    StartCoroutine(UIUtility.SetHideUI(_PromptUI.gameObject, true, true));
    StartCoroutine(UIUtility.SetHideUI(_LoadGameButtonActiveTarget, true, true));
    StartCoroutine(_start_as_coroutine());
  }
}