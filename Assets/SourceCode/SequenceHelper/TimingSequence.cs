using System.Threading;
using TMPro;
using UnityEngine;



namespace SequenceHelper{
  public class TimerSequence: MonoBehaviour, ISequenceAsync{
    [SerializeField]
    private float _Timer;

    [SerializeField]
    private bool _UseUnscaled = false;

    private float _current_timer = 0;

    public void FixedUpdate(){
      if(_current_timer > 0)
        _current_timer -= _UseUnscaled?
          Time.fixedUnscaledDeltaTime:
          Time.fixedDeltaTime;
    }

    
    public void StartTriggerAsync(){
      _current_timer = _Timer;
    }

    public bool IsTriggering(){
      return _current_timer > 0;
    }
  }


  public class PauseGameSequence: MonoBehaviour, ISequenceAsync{
    [SerializeField]
    private bool _AsResume = false;

    private GameHandler _game_handler;

    public void Start(){
      _game_handler = FindAnyObjectByType<GameHandler>();
      if(_game_handler == null){
        Debug.LogError("Game Handler cannot be fetch.");
        throw new UnityEngine.MissingComponentException();
      }
    }


    public void StartTriggerAsync(){
      if(_AsResume)
        _game_handler.ResumeGame();
      else
        _game_handler.PauseGame();
    }

    public bool IsTriggering(){
      return false;
    }
  }
}