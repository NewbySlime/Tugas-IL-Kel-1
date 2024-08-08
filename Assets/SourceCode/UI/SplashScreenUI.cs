using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;


[RequireComponent(typeof(VideoPlayer))]
/// <summary>
/// UI Class for showing splash screen in the start of the game. The usage of this class instead of using Unity's configuration is to show the splash screen (the studio logo) using video.
/// 
/// This class uses following component(s);
/// - <b>VideoPlayer</b> to show the splash screen video. The VideoClip in <b>VideoPlayer</b> should already been assigned with the appropriate video.
/// 
/// This class uses autoload(s);
/// - <see cref="GameHandler"/> for game events and such.
/// </summary>
public class SplashScreenUI: MonoBehaviour{
  [SerializeField]
  // The target object to show/hide the UI element.
  private GameObject _TargetVisualContainer;

  private bool _already_triggered = false;


  private GameHandler _game_handler;
  private VideoPlayer _video_player;

  private Coroutine _splash_screen_coroutine = null;


  // Trigger function to start the splash screen video.
  private IEnumerator _trigger_splash_screen(){
    StartCoroutine(UIUtility.SetHideUI(_TargetVisualContainer, false, true));

    _video_player.Play();

    yield return new WaitUntil(() => _video_player.frame > 0);
    yield return new WaitUntil(() => (ulong)_video_player.frame >= (_video_player.frameCount-1));

    _splash_screen_finished();
  }

  // Function for when the coroutine is finished or resetting/stopping the coroutine.
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
    _video_player.Prepare();

    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingReferenceException();
    }

    _game_handler.SceneChangedFinishedEvent += _on_scene_changed;
  }

  
  /// <summary>
  /// Function to stop the splash screen video.
  /// </summary>
  public void CancelSplashScreen(){
    if(_splash_screen_coroutine == null)
      return;

    StopCoroutine(_splash_screen_coroutine);
    _splash_screen_finished();
  }


  /// <summary>
  /// Function to catch "UIAccept" input event.
  /// This function will stop the splash screen.
  /// </summary>
  /// <param name="value"></param>
  public void OnUIAccept(InputValue value){
    if(value.isPressed)
      CancelSplashScreen();
  }

  /// <summary>
  /// Function to catch "UIAccept_Mouse" input event.
  /// This function will stop the splash screen.
  /// </summary>
  /// <param name="value"></param>
  public void OnUIAccept_Mouse(InputValue value){
    if(value.isPressed)
      CancelSplashScreen();
  }
}