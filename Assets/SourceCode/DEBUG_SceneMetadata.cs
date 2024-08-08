using UnityEngine;


/// <summary>
/// DEBUG purpose class for storing data about the current scene. On debugging phase of the Game, <see cref="GameHandler"/> needs a scene data that would initially supplied at the scene entry point.
/// </summary>
public class DEBUG_SceneMetadata: MonoBehaviour{
  [SerializeField]
  private GameHandler.SceneMetadata _Metadata;

  /// <summary>
  /// Get current scene data.
  /// </summary>
  /// <returns>The scene data</returns>
  public GameHandler.SceneMetadata GetSceneMetadata(){
    return _Metadata;
  }
}