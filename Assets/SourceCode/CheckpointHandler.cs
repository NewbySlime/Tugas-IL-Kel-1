using UnityEngine;



// TODO try to get filter if player that enters it
public class CheckpointHandler: AreaTrigger{
  [SerializeField]
  private string _CheckpointID;

  private GameHandler _game_handler;
  protected GameHandler _GameHandler{
    get => _game_handler;
  }

  protected bool _TriggerSaveOnEnter = false;


  private void _on_checkpoint_entered(Collider2D collider){
    PlayerController _player = collider.gameObject.GetComponent<PlayerController>();
    if(_player == null)
      return;

    _game_handler.SetLastCheckpoint(_CheckpointID);

    if(_TriggerSaveOnEnter)
      _game_handler.SaveGame();
  }


  protected virtual void _GameSceneChangedFinished(string scene_id, GameHandler.GameContext context){
    TriggerOnEnter = true;
    _TriggerSaveOnEnter = true;
  }


  protected override void _OnObjectEnter(Collider2D collider){
    if(TriggerOnEnter)
      _on_checkpoint_entered(collider);

    base._OnObjectEnter(collider);
  }


  public new void Start(){
    Debug.Log("Checkpoint starting");
    base.Start();

    TriggerOnEnter = false;
    _TriggerSaveOnEnter = false;

    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot get Game Handler.");
      throw new UnityEngine.MissingComponentException();
    }

    _game_handler.SceneChangedFinishedEvent += _GameSceneChangedFinished;
  }


  public void TeleportObject(GameObject game_object){
    game_object.transform.position = transform.position;
  }


  public string GetCheckpointID(){
    return _CheckpointID;
  }
}