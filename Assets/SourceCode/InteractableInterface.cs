using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Interface component that any object can use to receive input/interaction from <see cref="InteractionHandler"/>. This class only acts as a receiver, it does not handle events for when an interaction "view" has entered nor exited.
/// For further explanation, see <b>Reference/Diagrams/InteractionHandling.drawio</b>
/// 
/// Message interface used for interaction;
/// - <see cref="InteractableInterface_InteractableEnter"/>
/// - <see cref="InteractableInterface_Interact"/>
/// - <see cref="InteractableInterface_InteractableExit"/>
/// 
/// This class uses external component(s);
/// - List of <b>GameObject</b>(s) to signalling any <see cref="InteractableInterface"/> or any object that can receive message interface to this class.
/// </summary>
public class InteractableInterface: MonoBehaviour{
  [SerializeField]
  private List<GameObject> _IncludedListenerObject;

  [SerializeField]
  private bool _InteractOnAvailable = false;
  /// <summary>
  /// Can any <see cref="InteractionHandler"/> interact with this object?
  /// </summary>
  public bool InteractOnAvailable{get => _InteractOnAvailable;}

  [SerializeField]
  private bool _PassThrough = false;
  /// <summary>
  /// Should this interface block any interaction "view"?
  /// </summary>
  /// <value></value>
  public bool PassThrough{get => _PassThrough;}


  /// <summary>
  /// Function to be called when this object entered <see cref="InteractionHandler"/> interaction "view". This function does not call on its own, it is being called by <see cref="InteractionHandler"/>.
  /// This is a function to catch interface message from <see cref="InteractionHandler"/>.
  /// </summary>
  public void InteractableInterface_InteractableEnter(){
    if(!gameObject.activeInHierarchy)
      return;
      
    foreach(GameObject _listener in _IncludedListenerObject){
      InteractableInterface _interface = _listener.GetComponent<InteractableInterface>();
      if(_interface != null){
        _interface.TriggerInteractionEnter();
        continue;
      }

      _listener.SendMessage("InteractableInterface_InteractableEnter", SendMessageOptions.DontRequireReceiver);
    }
  } 

  /// <summary>
  /// Trigger for if this object entered the interaction "view".
  /// This will signaling other objects attached to this using message interface <b>InteractableInterface_InteractableEnter</b>.
  /// </summary>
  public void TriggerInteractionEnter(){
    if(!gameObject.activeInHierarchy)
      return;
      
    if(_InteractOnAvailable)
      TriggerInteract();

    gameObject.SendMessage("InteractableInterface_InteractableEnter", SendMessageOptions.DontRequireReceiver);

    InteractableInterface_InteractableEnter();
  }


  /// <summary>
  /// Function to be called when this object exited <see cref="InteractionHandler"/> interaction "view". This function does not call on its own, it is being called by <see cref="InteractionHandler"/>.
  /// This is a function to catch interface message from <see cref="InteractionHandler"/>.
  /// </summary>
  public void InteractableInterface_InteractableExit(){
    if(!gameObject.activeInHierarchy)
      return;
      
    foreach(GameObject _listener in _IncludedListenerObject){
      InteractableInterface _interface = _listener.GetComponent<InteractableInterface>();
      if(_interface != null){
        _interface.TriggerInteractionExit();
        continue;
      }

      _listener.SendMessage("InteractableInterface_InteractableExit", SendMessageOptions.DontRequireReceiver);
    }
  }

  /// <summary>
  /// Trigger for if this object exited the interaction "view".
  /// This will signalling other objects attached to this using message interface <b>InteractableInterface_InteractableExit</b>.
  /// </summary>
  public void TriggerInteractionExit(){
    if(!gameObject.activeInHierarchy)
      return;
      
    gameObject.SendMessage("InteractableInterface_InteractableExit", SendMessageOptions.DontRequireReceiver);

    InteractableInterface_InteractableExit();
  }


  /// <summary>
  /// Function to be called when an <see cref="InteractionHandler"/> has prompted to interact with objects near it (in its interaction "view"). This function does not call on its own, it is being called by <see cref="InteractionHandler"/>.
  /// This is a function to catch interface message from <see cref="InteractionHandler"/>.
  /// </summary>
  public void InteractableInterface_Interact(){
    if(!gameObject.activeInHierarchy)
      return;
      
    foreach(GameObject _listener in _IncludedListenerObject){
      InteractableInterface _interface = _listener.GetComponent<InteractableInterface>();
      if(_interface != null){
        _interface.TriggerInteract();
        continue;
      }

      _listener.SendMessage("InteractableInterface_Interact", SendMessageOptions.DontRequireReceiver);
    }
  }

  /// <summary>
  /// Trigger for if this object is being interacted by <see cref="InteractionHandler"/>.
  /// This will signalling other objects attached to this using message interface <b>InteractableInterface_Interact</b>.
  /// </summary>
  public void TriggerInteract(){
    if(!gameObject.activeInHierarchy)
      return;
      
    gameObject.SendMessage("InteractableInterface_Interact", SendMessageOptions.DontRequireReceiver);

    InteractableInterface_Interact();
  }
}