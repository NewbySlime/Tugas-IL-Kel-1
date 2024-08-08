using UnityEngine;



/// <summary>
/// Extended <see cref="AreaTrigger"/> object for triggering save point when an object has entered its area.
/// </summary>
public class CheckpointHandler: AreaTrigger{
  [SerializeField]
  private string _CheckpointID;
  /// <summary>
  /// The ID for this checkpoint object.
  /// </summary>
  public string CheckpointID{get => _CheckpointID;}

  private GameHandler _game_handler;
  /// <summary>
  /// GameHandler used in this class.
  /// </summary>
  protected GameHandler _GameHandler{
    get => _game_handler;
  }

  /// <summary>
  /// Should the checkpoint be triggered when an <see cref="PlayerController"/> has entered its area.
  /// </summary>
  protected bool _TriggerCheckpointOnEnter = false;
  /// <summary>
  /// Should this object trigger "save" event when "CheckpointOnEnter" event. 
  /// </summary>
  protected bool _TriggerSaveOnEnter = true;


  private void _on_checkpoint_entered(Collider2D collider){
    if(_TriggerCheckpointOnEnter){
      PlayerController _player = collider.gameObject.GetComponent<PlayerController>();
      if(_player == null || !_player.TriggerAvailable)
        return;

      _game_handler.SetLastCheckpoint(_CheckpointID);

      if(_TriggerSaveOnEnter)
        _game_handler.SaveGame();
    }
  }

  private void _on_scene_removing(){
    _game_handler.SceneChangedFinishedEvent -= _GameSceneChangedFinished;
    _game_handler.SceneRemovingEvent -= _on_scene_removing;
  }


  /// <summary>
  /// Virtual function that catches event when a game changed scene.
  /// </summary>
  /// <param name="scene_id">The scene to change into</param>
  /// <param name="context">The context of this scene</param>
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
    DEBUGModeUtils.Log("Checkpoint starting");
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


  /// <summary>
  /// Function for teleporting an object to this checkpoint (not handling scene changing. For that see <see cref="LevelTeleportHandler"/>).
  /// </summary>
  /// <param name="game_object">The target object to teleport</param>
  public void TeleportObject(GameObject game_object){
    Debug.Log("Teleporting object...");
    game_object.transform.position = transform.position;
  }


  /// <summary>
  /// Get ID for this checkpoint.
  /// </summary>
  /// <returns>The ID</returns>
  public string GetCheckpointID(){
    return _CheckpointID;
  }
}