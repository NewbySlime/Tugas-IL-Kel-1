using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;



public class AreaTrigger: MonoBehaviour{
  [SerializeField]
  private SequenceHandlerVS _SequenceHandler;

  [SerializeField]
  private RigidbodyMessageRelay _Rigidbody;

  private GameHandler _game_handler;

  public bool TriggerOnEnter = true;


  protected virtual void _OnObjectEnter(Collider2D collider){
    if(!_game_handler.AreaTriggerEnable || !TriggerOnEnter || _SequenceHandler == null || _SequenceHandler.IsTriggering())
      return;

    Debug.Log("done");
    _SequenceHandler.StartTriggerAsync();
  }


  public void Start(){
    _Rigidbody.OnTriggerEntered2DEvent += _OnObjectEnter;

    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingReferenceException();
    }
  }
}