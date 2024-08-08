using UnityEngine;


/// <summary>
/// Static class containing functions for debugging functionalities.
/// Functions in this class will not work outside debugging mode.
/// This class will still retain its structure and functions even outside debug mode.
/// </summary>
public static class DEBUGModeUtils{
  /// <summary>
  /// Logs a message/data in normal log.
  /// </summary>
  /// <param name="data">The data to log</param>
  public static void Log(object data){
#if DEBUG
    Debug.Log(data);
#endif
  }

  /// <summary>
  /// Logs a message/data in warning log.
  /// </summary>
  /// <param name="data">The data to log</param>
  public static void LogWarning(object data){
#if DEBUG
    Debug.LogWarning(data);
#endif
  }

  /// <summary>
  /// Logs a message/data in error log.
  /// </summary>
  /// <param name="data">The data to log</param>
  public static void LogError(object data){
#if DEBUG
    Debug.LogError(data);
#endif
  }
}