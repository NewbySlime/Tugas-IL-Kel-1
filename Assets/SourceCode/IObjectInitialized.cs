
/// <summary>
/// Interface class that any object can use to know the readyness state of the object.
/// </summary>
public interface IObjectInitialized{
  /// <summary>
  /// Check if this object has been initialized or not.
  /// </summary>
  /// <returns>Is this object ready or not</returns>
  public bool GetIsInitialized();
}