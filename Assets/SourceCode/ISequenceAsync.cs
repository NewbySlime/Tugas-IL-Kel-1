using System;
using System.Collections;


/// <summary>
/// Interface class used for triggering the inheriting Sequence system.
/// </summary>
public interface ISequenceAsync{
  /// <summary>
  /// Function to trigger the Sequence asynchronously.
  /// </summary>
  public void StartTriggerAsync();

  /// <summary>
  /// Function to check if the Sequence is still playing or not.
  /// </summary>
  /// <returns></returns>
  public bool IsTriggering();
}