using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;


[RequireComponent(typeof(VideoPlayer))]
public class SplashScreenUI: MonoBehaviour{
  [SerializeField]
  private GameObject _TargetVisualContainer;

  private bool _already_triggered = false;


  private GameHandler _game_handler;
  private VideoPlayer _video_player;

  private Coroutine _splash_screen_coroutine = null;


  private IEnumerator _trigger_splash_screen(){
    StartCoroutine(UIUtility.SetHideUI(_TargetVisualContainer, false, true));

    _video_player.Play();

    yield return new WaitUntil(() => _video_player.frame > 0);
    yield return new WaitUntil(() => (ulong)_video_player.frame >= (_video_player.frameCount-1));

    _splash_screen_finished();
  }

  private void _splash_screen_finished(){
    _video_player.Stop();
    StartCoroutine(UIUtility.SetHideUI(_TargetVisualContainer, true));

    _splash_screen_coroutine = null;
  }


  private void _on_scene_changed(string scene_id, GameHandler.GameContext context){
    if(_already_triggered || _splash_screen_coroutine != null)
      return;

    _already_triggered = true;
    if(context != GameHandler.GameContext.MainMenu){
      StartCoroutine(UIUtility.SetHideUI(_TargetVisualContainer, true, true));
      return;
    }

    _splash_screen_coroutine = StartCoroutine(_trigger_splash_screen());
  }


  public void Start(){
    _video_player = GetComponent<VideoPlayer>();

    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingReferenceException();
    }

    _game_handler.SceneChangedFinishedEvent += _on_scene_changed;
  }

  
  public void CancelSplashScreen(){
    if(_splash_screen_coroutine == null)
      return;

    StopCoroutine(_splash_screen_coroutine);
    _splash_screen_finished();
  }


  public void OnUIAccept(InputValue value){
    if(value.isPressed)
      CancelSplashScreen();
  }

  public void OnUIAccept_Mouse(InputValue value){
    if(value.isPressed)
      CancelSplashScreen();
  }
}