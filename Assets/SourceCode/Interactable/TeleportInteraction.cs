using System.Collections;
using UnityEngine;


/// <summary>
/// A class that handles player level changing based on the interaction of the object.
/// <seealso cref="LevelTeleportHandler"/> 
/// </summary>
public class TeleportInteraction: LevelTeleportHandler{

  protected override void _GameSceneChangedFinished(string scene_id, GameHandler.GameContext context){
    base._GameSceneChangedFinished(scene_id, context);

    _TriggerChangeSceneOnEnter = false;
  }


  /// <summary>
  /// Interface function used by <see cref="InteractableInterface"/> for when an "Interact" event is triggered. 
  /// </summary>
  public void InteractableInterface_Interact(){
    TriggerTeleport();
  }
}