using UnityEngine;


[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasRecursiveScaleCalibrator))]
/// <summary>
/// Class for rescaling UI elements' <b>RectTransform</b> when the game changes level/scene.
/// Explanation: A bug that happen in Unity's Canvas (with RenderMode put to "Screen Space - Camera") when the Game changes scene and then the Canvas has to do some anticipation when the camera is gone and then reapplied with new camera in the new scene, the Canvas would resized to bigger size than it shouldn't be (say, around 100 from 1).
/// 
/// This class uses following component(s);
/// - <b>Canvas</b> base component used in this class.
/// - <see cref="CanvasRecursiveScaleCalibrator"/> for recalibrating function.
/// 
/// This class uses following autoload(s);
/// - <see cref="GameHandler"/> for using Game events and such.
/// </summary>
public class CanvasAutoCameraSetter: MonoBehaviour{
  private GameHandler _game_handler;

  private Canvas _canvas;
  private CanvasRecursiveScaleCalibrator _scale_calibrator;


  private void _game_handler_scene_initialized(string scene_id, GameHandler.GameContext context){
    DEBUGModeUtils.Log("game handler scene initialized");
    _canvas.worldCamera = FindAnyObjectByType<Camera>();
    if(_canvas.worldCamera == null){
      Debug.LogWarning("No Camera found.");
    }

    _scale_calibrator.TriggerCalibrate();
  }

  private void _game_handler_scene_removed(){
    _canvas.worldCamera = null;

    // check if in DontDestroyOnLoad
    if(gameObject.scene.buildIndex == -1)
      return;

    _game_handler.SceneChangedFinishedEvent -= _game_handler_scene_initialized;
    _game_handler.SceneRemovingEvent -= _game_handler_scene_removed;
  }


  public void Start(){
    _canvas = GetComponent<Canvas>(); _canvas.worldCamera = null;
    _scale_calibrator = GetComponent<CanvasRecursiveScaleCalibrator>();
    
    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingReferenceException();
    }

    _game_handler.SceneChangedFinishedEvent += _game_handler_scene_initialized;
    _game_handler.SceneRemovingEvent += _game_handler_scene_removed;
    
    if(_game_handler.SceneInitialized)
      _game_handler_scene_initialized(_game_handler.GetCurrentSceneID(), _game_handler.GetCurrentSceneContext());
  }
}