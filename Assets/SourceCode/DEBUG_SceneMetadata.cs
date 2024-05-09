using UnityEngine;


public class DEBUG_SceneMetadata: MonoBehaviour{
  [SerializeField]
  private GameHandler.SceneMetadata _Metadata;

  public GameHandler.SceneMetadata GetSceneMetadata(){
    return _Metadata;
  }
}