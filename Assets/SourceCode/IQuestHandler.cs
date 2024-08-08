using System;


/// <summary>
/// Interface class used for handling custom Quests.
/// </summary>
public interface IQuestHandler{
  /// <summary>
  /// Event used to notify if the quest has updated (progress or another kind of data that has been changed).
  /// </summary>
  public event QuestUpdated QuestUpdatedEvent;
  public delegate void QuestUpdated(IQuestHandler quest_handler);

  /// <summary>
  /// Event used to notify if the quest has been completed.
  /// </summary>
  public event QuestFinished QuestFinishedEvent;
  public delegate void QuestFinished(IQuestHandler quest_handler);

  /// <summary>
  /// To get info about the current state of the quest completion.
  /// </summary>
  /// <returns>Current quest info</returns>
  public QuestHandlerVS.QuestInfo GetQuestInfo();

  /// <summary>
  /// Get quest goal message sentence.
  /// </summary>
  /// <returns>The message</returns>
  public string GetGoalMessage();

  /// <summary>
  /// Get current progress in numerical value.
  /// </summary>
  /// <returns>Current progress</returns>
  public float GetProgress();
}