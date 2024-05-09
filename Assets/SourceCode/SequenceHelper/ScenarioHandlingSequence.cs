using UnityEngine;


namespace SequenceHelper{
  public class StartScenarioSequence: MonoBehaviour, ISequenceAsync{
    [SerializeField]
    private string _ScenarioID;

    private GameHandler _game_handler;


    public void Start(){
      _game_handler = FindAnyObjectByType<GameHandler>();
      if(_game_handler == null){
        Debug.LogError("Cannot get Game Handler.");
        throw new UnityEngine.MissingComponentException();
      }
    }


    public void StartTriggerAsync(){
      StartCoroutine(_game_handler.StartScenario(_ScenarioID));
    }

    public bool IsTriggering(){
      return false;
    }
  }
}