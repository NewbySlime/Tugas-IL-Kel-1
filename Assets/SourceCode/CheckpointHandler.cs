using UnityEngine;



public class CheckpointHandler: AreaTrigger{
  [SerializeField]
  private string _CheckpointID;
  public string CheckpointID{get => _CheckpointID;}

  private GameHandler _game_handler;
  protected GameHandler _GameHandler{
    get => _game_handler;
  }

  protected bool _TriggerCheckpointOnEnter = false;
  protected bool _TriggerSaveOnEnter = true;


  private void _on_checkpoint_entered(Collider2D collider){
    if(_TriggerCheckpointOnEnter){
      PlayerController _player = collider.gameObject.GetComponent<PlayerController>();
      if(_player == null || !_player.TriggerAvailable)
        return;

      _game_handler.SetLastCheckpoint(_CheckpointID);
    }

    if(_TriggerSaveOnEnter)
      _game_handler.SaveGame();
  }

  private void _on_scene_removing(){
    _game_handler.SceneChangedFinishedEvent -= _GameSceneChangedFinished;
    _game_handler.SceneRemovingEvent -= _on_scene_removing;
  }


  protected virtual void _GameSceneChangedFinished(string scene_id, GameHandler.GameContext context){
    TriggerOnEnter = true;
    _TriggerCheckpointOnEnter = true;
    _TriggerSaveOnEnter = true;
  }


  protected override void _OnObjectEnter(Collider2D collider){
    _on_checkpoint_entered(collider);

    base._OnObjectEnter(collider);
  }


  ~CheckpointHandler(){
    _on_scene_removing();
  }


  public new void Start(){
    Debug.Log("Checkpoint starting");
    base.Start();

    TriggerOnEnter = false;
    _TriggerCheckpointOnEnter = false;
    _TriggerSaveOnEnter = false;

    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot get Game Handler.");
      throw new UnityEngine.MissingComponentException();
    }

    _game_handler.SceneChangedFinishedEvent += _GameSceneChangedFinished;
    _game_handler.SceneRemovingEvent += _on_scene_removing;

    if(_game_handler.SceneInitialized)
      _GameSceneChangedFinished(_game_handler.GetCurrentSceneID(), _game_handler.GetCurrentSceneContext());
  }


  public void TeleportObject(GameObject game_object){
    Debug.Log("Teleporting object...");
    game_object.transform.position = transform.position;
  }


  public string GetCheckpointID(){
    return _CheckpointID;
  }
}