using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;



/// <summary>
/// Class for handling interaction with other objects that has <see cref="InteractableInterface"/>. This class acts as the actor, all events surrounding interactions are handled here.
/// For further explanation, see <b>Reference/Diagrams/InteractionHandling.drawio</b>
/// 
/// This class uses external component(s);
/// - <see cref="RigidbodyMessageRelay"/> for getting physics events of the target body used.
/// - <see cref="InteractableInterface"/> for signalling the interface for interaction events.
/// </summary>
public class InteractionHandler: MonoBehaviour{
  [SerializeField]
  private RigidbodyMessageRelay _InteractionTrigger;


  private HashSet<InteractableInterface> _interactable_list = new();

  private InteractableInterface _current_interactable = null;

  
  // NOTE: this function calculates nearest object by the <b>Transfrom</b> component's position, this does not use collide point for the calculation.
  // This function returns a list of interface objects not a single interface due to some interactable are pass through, but still accounted. Last object in the list is blocking (meaning pass through flag is not enabled).
  private List<InteractableInterface> _get_nearest_objects(){
    List<InteractableInterface> _interface_list = _interactable_list.ToList();
    _interface_list.Sort((InteractableInterface var1, InteractableInterface var2) => {
      float _dist_from1 = (transform.position - var1.transform.position).magnitude;
      float _dist_from2 = (transform.position - var2.transform.position).magnitude;

      return _dist_from1 > _dist_from2? 1: -1;
    });

    List<InteractableInterface> _result = new();
    foreach(InteractableInterface _interface in _interface_list){
      _result.Add(_interface);

      if(!_interface.PassThrough)
        break;
    }

    return _result;
  }

  // Trigger events for any entered objects, and trigger exit to interactable behind the first object.
  // The parameter is a list of interactables due to a possibility that an interactable might be pass through.
  private void _trigger_enter_objects(List<InteractableInterface> list_interface){
    HashSet<InteractableInterface> _exit_set = new(_interactable_list);
    foreach(InteractableInterface _interface_obj in list_interface){
      _exit_set.Remove(_interface_obj);
      if(_interface_obj.InteractOnAvailable)
        continue;

      _interface_obj.TriggerInteractionEnter();
    }

    // trigger exit to not account any objs
    foreach(InteractableInterface _interface_obj in _exit_set)
      _interface_obj.TriggerInteractionExit();
  }


  // Change the current focus of the interactable object.
  private void _change_current(InteractableInterface interactable){
    if(interactable == _current_interactable)
      return;

    if(_current_interactable != null){
      _current_interactable.TriggerInteractionExit();
    }

    _current_interactable = interactable;
    if(interactable != null){
      interactable.TriggerInteractionEnter();
    }
  }


  public void Start(){
    if(_InteractionTrigger == null) 
      Debug.LogWarning("No Trigger for Interaction can be used.");
    else{
      _InteractionTrigger.OnTriggerEntered2DEvent += Interaction_OnEnter;
      _InteractionTrigger.OnTriggerExited2DEvent += Interaction_OnExit;
    }
  }


  /// <summary>
  /// Function to catch <see cref="RigidbodyMessageRelay.OnTriggerEntered2DEvent"/> event from the target body.
  /// It will trigger an "interactable enter" event to the entered object and readjust the interactable focus to a valid one.
  /// </summary>
  /// <param name="collider">The entered object</param>
  public void Interaction_OnEnter(Collider2D collider){
    InteractableInterface _interface = collider.gameObject.GetComponent<InteractableInterface>();
    if(_interface == null)
      return;

    _interactable_list.Add(_interface);

    List<InteractableInterface> _nearest_objs = _get_nearest_objects();
    _trigger_enter_objects(_nearest_objs);

    _change_current(_nearest_objs.Count > 0? _nearest_objs[_nearest_objs.Count-1]: null);
  }

  /// <summary>
  /// Function to catch <see cref="RigidbodyMessageRelay.OnTriggerExited2DEvent"/> event from the target body.
  /// It will trigger an "interactable exit" event to the exited object and readjust the interactable focus to a valid one. 
  /// </summary>
  /// <param name="collider">The exited object</param>
  public void Interaction_OnExit(Collider2D collider){
    InteractableInterface _interface = collider.gameObject.GetComponent<InteractableInterface>();
    if(_interface == null)
      return;

    _interactable_list.Remove(_interface);
    _interface.TriggerInteractionExit();

    List<InteractableInterface> _nearest_objs = _get_nearest_objects();
    _trigger_enter_objects(_nearest_objs);

    _change_current(_nearest_objs.Count > 0? _nearest_objs[_nearest_objs.Count-1]: null);
  }

  /// <summary>
  /// Function to trigger an interaction event to a nearest and valid interactable object.
  /// The trigger event will be skipped when there are no current valid focus to an interactable.
  /// </summary>
  /// <returns>Is the trigger event succeeded or not</returns>
  public bool TriggerInteraction(){
    if(_current_interactable == null)
      return false;

    _current_interactable.TriggerInteract();
    return true;
  }
}