using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Database for storing data about <see cref="CheckpointHandler"/> in a certain scene. This database have to be updated whenever a scene has been changed due to this class does not use nor store any data related to a scene. Instead, it will get all <see cref="CheckpointHandler"/> object existed in the scene.
/// This database should exist only once throughout the Game instance. 
/// </summary>
public class LevelCheckpointDatabase: MonoBehaviour{
  private Dictionary<string, CheckpointHandler> _checkpoint_map = new Dictionary<string, CheckpointHandler>();


  /// <summary>
  /// Update and get all <see cref="CheckpointHandler"/>.
  /// </summary>
  public void UpdateDatabase(){
    Debug.Log("[LevelCheckpointDatabase] Refreshing database...");
    _checkpoint_map.Clear();

    CheckpointHandler[] _checkpoint_list = FindObjectsOfType<CheckpointHandler>();
    foreach(CheckpointHandler _checkpoint in _checkpoint_list)
      _checkpoint_map[_checkpoint.GetCheckpointID()] = _checkpoint;
  }

  
  #nullable enable
  /// <summary>
  /// Get checkpoint object based on the ID.
  /// Returns null if no such checkpoint existed.
  /// </summary>
  /// <param name="checkpoint_id">The target checkpoint</param>
  /// <returns>The checkpoint object</returns>
  public CheckpointHandler? GetCheckpoint(string checkpoint_id){
    if(!_checkpoint_map.ContainsKey(checkpoint_id))
      return null;

    return _checkpoint_map[checkpoint_id];
  }
  #nullable disable
}