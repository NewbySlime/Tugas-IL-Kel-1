using UnityEngine;
using System.Collections;
using TMPro;
using System;
using System.Threading;
using System.Runtime.InteropServices;


[RequireComponent(typeof(FadeUI))]
[RequireComponent(typeof(SlideUI))]
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


  private void _on_restart_button(){
    Debug.Log("restart game");
    _game_handler.RestartFromLastCheckpoint();
  }

  private void _on_main_menu_button(){
    _game_handler.ChangeSceneToMainMenu();
  }

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

  
  public void SetCauseText(string text){
    _CauseText.text = text;
  }
}