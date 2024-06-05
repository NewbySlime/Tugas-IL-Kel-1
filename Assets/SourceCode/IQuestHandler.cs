using System;


public interface IQuestHandler{
  public delegate void QuestUpdated(IQuestHandler quest_handler);
  public event QuestUpdated QuestUpdatedEvent;

  public delegate void QuestFinished(IQuestHandler quest_handler);
  public event QuestFinished QuestFinishedEvent;


  public QuestHandlerVS.QuestInfo GetQuestInfo();
  public string GetGoalMessage();
  public float GetProgress();
}