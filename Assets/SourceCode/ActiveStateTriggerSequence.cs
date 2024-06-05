using UnityEngine;


public class ActiveStateTriggerSequence: SequenceHandlerVS{
  [SerializeField]
  private bool _TriggerOnlyOnce;


  private GameHandler _game_handler;

  private bool _already_triggered = false;
  private bool _game_ready = false;


  private void _game_handler_scene_changed(string scene_id, GameHandler.GameContext context){
    _game_ready = true;
    OnEnabled();
  }

  private void _game_handler_scene_removed(){
    _game_handler.SceneChangedFinishedEvent -= _game_handler_scene_changed;
    _game_handler.SceneRemovingEvent -= _game_handler_scene_removed;
  }


  ~ActiveStateTriggerSequence(){
    _game_handler_scene_removed();
  }


  public new void Start(){
    base.Start();

    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingReferenceException();
    }

    _game_handler.SceneChangedFinishedEvent += _game_handler_scene_changed;
    _game_handler.SceneRemovingEvent += _game_handler_scene_removed;

    if(_game_handler.SceneInitialized)
      _game_handler_scene_changed(_game_handler.GetCurrentSceneID(), _game_handler.GetCurrentSceneContext());
  }


  public void OnEnabled(){
    if(!_game_ready || (_TriggerOnlyOnce && _already_triggered))
      return;

    StartTriggerAsync();
  }
}