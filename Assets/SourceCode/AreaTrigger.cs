using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;



/// <summary>
/// Class that triggers a sequence contained in this class by using event from <b>Collider2D</b> (as trigger) for when an object entered its physics body.
/// Uses <see cref="AreaTriggerActuator"/> to differentiate which object that can be use as a source of triggering.
/// 
/// This class uses external component(s);
/// - <see cref="SequenceHandlerVS"/> for sequencing system used in this class.
/// - <see cref="RigidbodyMessageRelay"/> for handling <b>Collider2D</b>'s Unity event.
/// 
/// This class uses autoload(s);
/// - <see cref="GameHandler"/> for game events and such.
/// </summary>
public class AreaTrigger: MonoBehaviour{
  [SerializeField]
  private SequenceHandlerVS _SequenceHandler;

  [SerializeField]
  private RigidbodyMessageRelay _Rigidbody;

  [DoNotSerialize]
  private GameHandler _game_handler = null;

  /// <summary>
  /// Should the class trigger the sequence when an object entered its body?
  /// </summary>
  public bool TriggerOnEnter = true;


  /// <summary>
  /// Virtual function for handling event for when an object entered its body.
  /// This function will still be triggered even by an object that does not have <see cref="AreaTriggerActuator"/>, so inheriting class can handle the trigger with its own filter.
  /// </summary>
  /// <param name="collider"></param>
  protected virtual void _OnObjectEnter(Collider2D collider){
    AreaTriggerActuator _trigger_actuator = collider.gameObject.GetComponent<AreaTriggerActuator>();
    if(_trigger_actuator == null || !_trigger_actuator.TriggerOnEnter || !TriggerOnEnter || _SequenceHandler == null || _SequenceHandler.IsTriggering())
      return;

    DEBUGModeUtils.Log(string.Format("Object entered {0}", collider.gameObject.name));
    _SequenceHandler.StartTriggerAsync();
  }


  public void Start(){
    if(_Rigidbody != null){
      _Rigidbody.OnTriggerEntered2DEvent += _OnObjectEnter;
    }

    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingReferenceException();
    }
  }
}