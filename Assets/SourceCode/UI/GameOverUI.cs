using UnityEngine;
using System.Collections;
using TMPro;
using System;
using System.Threading;
using System.Runtime.InteropServices;


[RequireComponent(typeof(FadeUI))]
[RequireComponent(typeof(SlideUI))]
/// <summary>
/// UI Component for showing Game Over screen.
/// 
/// This class uses external component(s);
/// - <b>Unity's TMP Text UI</b> for showing different types of game over causes.
/// - <see cref="ButtonBaseUI"/> for giving options to the player after game over state.
/// 
/// This class uses following autoload(s);
/// - <see cref="GameHandler"/> for using Game events and such. 
/// </summary>
public class GameOverUI: MonoBehaviour{
  [SerializeField]
  private TextMeshProUGUI _CauseText;

  [SerializeField]
  private ButtonBaseUI _restart_button;
  [SerializeField]
  private ButtonBaseUI _main_menu_button;
  [SerializeField]
  private ButtonBaseUI _quit_button;

  private GameHandler _game_handler;


  // on released
  private void _on_restart_button(){
    DEBUGModeUtils.Log("restart game");
    _game_handler.RestartFromLastCheckpoint();
  }

  // on released
  private void _on_main_menu_button(){
    _game_handler.ChangeSceneToMainMenu();
  }

  // on released
  private void _on_quit_button(){
    _game_handler.QuitGame();
  }


  public void Start(){
    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingReferenceException();
    }

    _restart_button.OnButtonReleasedEvent += _on_restart_button;
    _main_menu_button.OnButtonReleasedEvent += _on_main_menu_button;
    _quit_button.OnButtonReleasedEvent += _on_quit_button;
  }

  
  /// <summary>
  /// To show what causes the Game Over.
  /// </summary>
  /// <param name="text">Cause(s) in the form of text</param>
  public void SetCauseText(string text){
    _CauseText.text = text;
  }
}