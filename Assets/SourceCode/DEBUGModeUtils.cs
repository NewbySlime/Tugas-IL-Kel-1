using UnityEngine;


public class DEBUGModeUtils: MonoBehaviour{
  public static void Log(object data){
#if DEBUG
    Debug.Log(data);
#endif
  }

  public static void LogWarning(object data){
#if DEBUG
    Debug.LogWarning(data);
#endif
  }

  public static void LogError(object data){
#if DEBUG
    Debug.LogError(data);
#endif
  }
}