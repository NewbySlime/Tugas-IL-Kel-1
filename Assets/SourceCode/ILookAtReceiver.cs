using UnityEngine;


/// <summary>
/// Interface class used as a way to prompt the inherited object to look at certain direction.
/// </summary>
public interface ILookAtReceiver{
  /// <summary>
  /// Prompt this object to look at a direction.
  /// </summary>
  /// <param name="direction">The direction to look at</param>
  public void LookAt(Vector2 direction);
}