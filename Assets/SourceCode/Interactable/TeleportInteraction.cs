using System.Collections;
using UnityEngine;


public class TeleportInteraction: LevelTeleportHandler{


  protected override void _GameSceneChangedFinished(string scene_id, GameHandler.GameContext context){
    base._GameSceneChangedFinished(scene_id, context);

    _TriggerChangeSceneOnEnter = false;
  }


  public void InteractableInterface_Interact(){
    TriggerTeleport();
  }
}