
/// <summary>
/// Interface class used to handle data for the inheriting Sequence system.
/// </summary>
public interface ISequenceData{
  /// <summary>
  /// Get the ID about inheriting Sequence system.
  /// </summary>
  /// <returns>The Sequence ID</returns>
  public string GetSequenceID();

  /// <summary>
  /// Function to set appropriate data used by the inheriting Sequence system.
  /// </summary>
  /// <param name="data">The data used by inheriting Sequence system.</param>
  public void SetSequenceData(object data);
}