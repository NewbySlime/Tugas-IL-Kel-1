using System.Collections.Generic;
using UnityEngine;



public class InteractableInterface: MonoBehaviour{
  [SerializeField]
  private List<GameObject> _IncludedListenerObject;

  [SerializeField]
  private bool _InteractOnAvailable = false;


  public void InteractableInterface_InteractableEnter(){
    foreach(GameObject _listener in _IncludedListenerObject)
      _listener.SendMessage("InteractableInterface_InteractableEnter", SendMessageOptions.DontRequireReceiver);
  } 

  public void TriggerInteractionEnter(){
    if(_InteractOnAvailable)
      TriggerInteract();
    else{
      gameObject.SendMessage("InteractableInterface_InteractableEnter", SendMessageOptions.DontRequireReceiver);

      InteractableInterface_InteractableEnter();
    }
  }


  public void InteractableInterface_InteractableExit(){
    foreach(GameObject _listener in _IncludedListenerObject)
      _listener.SendMessage("InteractableInterface_InteractableExit", SendMessageOptions.DontRequireReceiver);
  }

  public void TriggerInteractionExit(){
    gameObject.SendMessage("InteractableInterface_InteractableExit", SendMessageOptions.DontRequireReceiver);

    InteractableInterface_InteractableExit();
  }


  public void InteractableInterface_Interact(){
    foreach(GameObject _listener in _IncludedListenerObject)
      _listener.SendMessage("InteractableInterface_Interact", SendMessageOptions.DontRequireReceiver);
  }

  public void TriggerInteract(){
    if(_InteractOnAvailable)
      return;

    gameObject.SendMessage("InteractableInterface_Interact", SendMessageOptions.DontRequireReceiver);

    InteractableInterface_Interact();
  }
}