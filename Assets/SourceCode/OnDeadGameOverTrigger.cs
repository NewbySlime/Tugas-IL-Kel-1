using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;


[RequireComponent(typeof(HealthComponent))]
/// <summary>
/// Trigger class for triggering a Game Over event to <see cref="GameHandler"/>.
/// 
/// This class uses following component(s);
/// - <see cref="HealthComponent"/> for listening vital events of this object.
///
/// This class uses external component(s);
/// - <see cref="GameHandler"/> for game events and such.
/// </summary>
public class OnDeadGameOverTrigger: MonoBehaviour{
  [SerializeField]
  private float _DeadDelay = 0;

  [SerializeField]
  private string _GameOverMessage;


  private GameHandler _game_handler;

  private HealthComponent _health_component;

  private Coroutine _current_dead_coroutine = null;

  /// <summary>
  /// Flag to enable triggering the Game Over event whenever this object is dead.
  /// </summary>
  public bool TriggerEnable = true;


  private IEnumerator _on_dead_co_func(){
    yield return new WaitForSeconds(_DeadDelay);
    _game_handler.TriggerGameOver(_GameOverMessage);

    _current_dead_coroutine = null;
  }

  private void _trigger_on_dead(){
    if(_current_dead_coroutine != null || !TriggerEnable) 
      return;

    _current_dead_coroutine = StartCoroutine(_on_dead_co_func());
  }

  private void _on_health_changed(int new_health){
    if(new_health <= 0)
      _trigger_on_dead();
    else
      CancelTrigger();
  }


  public void Start(){
    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      return;
    }

    _health_component = GetComponent<HealthComponent>();
    // set HealthComponent not to destroy when dead, since that will interrupt the trigger
    _health_component.DestroyOnDead = false;
    _health_component.OnHealthChangedEvent += _on_health_changed;
  }


  /// <summary>
  /// Cancel the triggering to prevent Game Over event (after trigger, it will be delayed first before Game Over event).
  /// </summary>
  public void CancelTrigger(){
    if(_current_dead_coroutine == null)
      return;

    StopCoroutine(_current_dead_coroutine);
    _current_dead_coroutine = null;
  }
}