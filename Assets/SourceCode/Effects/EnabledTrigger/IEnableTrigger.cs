
/// <summary>
/// An interface class to set some options of the behaviour of the class. This interface linked to Unity's Enable event.
/// </summary>
public interface IEnableTrigger{
  /// <summary>
  /// Set a member that determines to process a trigger when the object is being enabled.
  /// </summary>
  /// <param name="flag">The process trigger flag.</param>
  public void TriggerSetOnEnable(bool flag);
}