using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;



public class InteractionHandler: MonoBehaviour{
  [SerializeField]
  private RigidbodyMessageRelay _InteractionTrigger;


  private HashSet<InteractableInterface> _interactable_list = new();


  public void Start(){
    if(_InteractionTrigger == null) 
      Debug.LogWarning("No Trigger for Interaction can be used.");
    else{
      _InteractionTrigger.OnTriggerEntered2DEvent += Interaction_OnEnter;
      _InteractionTrigger.OnTriggerExited2DEvent += Interaction_OnExit;
    }
  }


  public void Interaction_OnEnter(Collider2D collider){
    InteractableInterface _interface = collider.gameObject.GetComponent<InteractableInterface>();
    if(_interface == null)
      return;

    _interactable_list.Add(_interface);
    _interface.TriggerInteractionEnter();
  }

  public void Interaction_OnExit(Collider2D collider){
    InteractableInterface _interface = collider.gameObject.GetComponent<InteractableInterface>();
    if(_interface == null)
      return;

    _interactable_list.Remove(_interface);
    _interface.TriggerInteractionExit();
  }

  public bool TriggerInteraction(){
    if(_interactable_list.Count <= 0)
      return false;

    InteractableInterface _nearest_obj = null;
    float _nearest_dist = float.PositiveInfinity;
    foreach(InteractableInterface _interface in _interactable_list){
      float _dist = (transform.position - _interface.transform.position).magnitude;
      if(_dist < _nearest_dist){
        _nearest_obj = _interface;
        _nearest_dist = _dist;
      }
    }

    _nearest_obj.TriggerInteract();
    Debug.Log("interaction do");

    return true;
  }
}