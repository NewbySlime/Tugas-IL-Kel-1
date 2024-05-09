using System.Collections.Generic;
using UnityEngine;



public class LevelCheckpointDatabase: MonoBehaviour{
  private Dictionary<string, CheckpointHandler> _checkpoint_map = new Dictionary<string, CheckpointHandler>();

  private GameHandler _game_handler;


  private void _scene_changed_initializing(string scene_id, GameHandler.GameContext context){
    Debug.Log("refresh database");
    _checkpoint_map.Clear();

    CheckpointHandler[] _checkpoint_list = FindObjectsOfType<CheckpointHandler>();
    foreach(CheckpointHandler _checkpoint in _checkpoint_list)
      _checkpoint_map[_checkpoint.GetCheckpointID()] = _checkpoint;
  }


  public void Start(){
    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot get Game Handler.");
      throw new UnityEngine.MissingComponentException();
    }

    _game_handler.SceneChangedInitializingEvent += _scene_changed_initializing; 
  }

  
  #nullable enable
  public CheckpointHandler? GetCheckpoint(string checkpoint_id){
    if(!_checkpoint_map.ContainsKey(checkpoint_id))
      return null;

    return _checkpoint_map[checkpoint_id];
  }
  #nullable disable
}