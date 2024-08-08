using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// UI for game pause and handles the option in the game pause.
/// 
/// This class uses external component(s);
/// - <see cref="ButtonBaseUI"/> for handling option's interactions.
/// 
/// This class uses autoload(s);
/// - <see cref="GameHandler"/> for game events and such.
/// - <see cref="InputFocusContext"/> for asking focus for using input.
/// </summary>
public class PauseGameUI: MonoBehaviour{
  [SerializeField]
  private ButtonBaseUI _ResumeGameButton;
  [SerializeField]
  private ButtonBaseUI _OpenSettingsButton;
  [SerializeField]
  private ButtonBaseUI _MainMenuButton;
  [SerializeField]
  private ButtonBaseUI _ExitGameButton;


  private GameHandler _game_handler;
  private InputFocusContext _focus_context;

  // spam prevention flag for "cancel" input.
  // this will prevent any input coming after trigger for resuming the game.
  private bool _cancel_spam_prevention_flag = true;


  private IEnumerator _cancel_spam_co_func(){
    yield return null;
    yield return new WaitForEndOfFrame();

    _cancel_spam_prevention_flag = false;
  }


  private void _on_resume_button(){
    _game_handler.ResumeGame();
  }

  private void _on_open_settings_button(){
    _game_handler.OpenSettingsUI();
  }

  private void _on_main_menu_button(){
    _game_handler.ChangeSceneToMainMenu();
  }

  private void _on_exit_game_button(){
    _game_handler.QuitGame();
  }


  public void Start(){
    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingReferenceException();
    }

    _focus_context = FindAnyObjectByType<InputFocusContext>();
    if(_focus_context == null){
      Debug.LogError("Cannot find InputFocusContext.");
      throw new MissingReferenceException();
    }

    _ResumeGameButton.OnButtonReleasedEvent += _on_resume_button;
    _OpenSettingsButton.OnButtonReleasedEvent += _on_open_settings_button;
    _MainMenuButton.OnButtonReleasedEvent += _on_main_menu_button;
    _ExitGameButton.OnButtonReleasedEvent += _on_exit_game_button;
  }


  /// <summary>
  /// Catch Unity's input event for "UICancel".
  /// Used for resuming the Game, and only available when the focus are given by <see cref="GameHandler"/> when the game is pausing.
  /// </summary>
  /// <param name="value">Unity's input data</param>
  public void OnUICancel(InputValue value){
    if(!_focus_context.InputAvailable(this))
      return;

    if(_cancel_spam_prevention_flag){
      StartCoroutine(_cancel_spam_co_func());
      return;
    }

    if(value.isPressed){
      _game_handler.ResumeGame();
    }

    _cancel_spam_prevention_flag = true;
  }
}