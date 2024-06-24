using System.Collections.Generic;
using UnityEngine;



public class LevelCheckpointDatabase: MonoBehaviour{
  private Dictionary<string, CheckpointHandler> _checkpoint_map = new Dictionary<string, CheckpointHandler>();



  public void UpdateDatabase(){
    Debug.Log("[LevelCheckpointDatabase] Refreshing database...");
    _checkpoint_map.Clear();

    CheckpointHandler[] _checkpoint_list = FindObjectsOfType<CheckpointHandler>();
    foreach(CheckpointHandler _checkpoint in _checkpoint_list)
      _checkpoint_map[_checkpoint.GetCheckpointID()] = _checkpoint;
  }

  
  #nullable enable
  public CheckpointHandler? GetCheckpoint(string checkpoint_id){
    if(!_checkpoint_map.ContainsKey(checkpoint_id))
      return null;

    return _checkpoint_map[checkpoint_id];
  }
  #nullable disable
}