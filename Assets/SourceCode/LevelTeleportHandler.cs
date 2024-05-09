using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;


public class LevelTeleportHandler: CheckpointHandler{
  [SerializeField]
  private string _ChangeSceneID;

  [SerializeField]
  private string _ArriveSceneID;


  protected bool _TriggerChangeSceneOnEnter = false;


  private void _on_teleport_enter(Collider2D collider){
    PlayerController _player = collider.gameObject.GetComponent<PlayerController>();
    if(_player == null)
      return;

    if(_TriggerChangeSceneOnEnter)
      _GameHandler.ChangeScene(_ChangeSceneID);
  }


  protected override void _GameSceneChangedFinished(string scene_id, GameHandler.GameContext context){
    base._GameSceneChangedFinished(scene_id, context);

    _TriggerChangeSceneOnEnter = true;
    _TriggerSaveOnEnter = false;
  }

  protected override void _OnObjectEnter(Collider2D collider){
    Debug.LogWarning("teleport triggered");
    if(TriggerOnEnter)
      _on_teleport_enter(collider);

    base._OnObjectEnter(collider);
  }


  public new void Start(){
    base.Start();

    TriggerOnEnter = false;
    _TriggerChangeSceneOnEnter = false;
    _TriggerSaveOnEnter = true;

    LevelCheckpointDatabase _database = FindAnyObjectByType<LevelCheckpointDatabase>();
    if(_database == null){
      Debug.LogWarning("Cannot get database for Level Checkpoints.");
      return;
    }

    PlayerController _player = FindAnyObjectByType<PlayerController>();
    if(_player != null){
      string _last_scene = _GameHandler.GetLastScene();
      Debug.Log(string.Format("Last scene {0}", _last_scene));
      if(_last_scene == _ArriveSceneID)
        TeleportObject(_player.gameObject);
    }
    else{
      Debug.LogWarning("Cannot find Player object.");
    }
  }


  public string GetChangeSceneID(){
    return _ChangeSceneID;
  }

  public string GetArriveSceneID(){
    return _ArriveSceneID;
  }
}