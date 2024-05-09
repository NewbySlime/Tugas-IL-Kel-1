

public interface IQuestObserver{
  public delegate void QuestUpdated(string quest_id);
  public event QuestUpdated QuestUpdatedEvent;

  public delegate void QuestFinished(string quest_id);
  public event QuestFinished QuestFinishedEvent;

 
  public QuestObserver.QuestInfo GetQuestInfo();
  public string GetQuestID();
  public string GetQuestName();

  public float GetQuestProgress();

  public void BindObject(QuestObserver quest_observer, QuestObserver.BindIQuestObserverCallback bind_func);
}