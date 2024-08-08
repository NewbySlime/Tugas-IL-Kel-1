
/// <summary>
/// Interface class used for handling data about the quest.
/// </summary>
public interface IQuestData{
  /// <summary>
  /// Get current Quest ID based on the inheriting class.
  /// </summary>
  /// <returns></returns>
  public string GetQuestID();

  /// <summary>
  /// Function for setting the appropriate quest data containing the data used for inheriting class.
  /// </summary>
  /// <param name="quest_data">The quest data</param>
  public void SetQuestData(object quest_data);
}