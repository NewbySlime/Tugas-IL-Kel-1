using System.Collections.Generic;
using UnityEngine;



public class InteractableInterface: MonoBehaviour{
  [SerializeField]
  private List<GameObject> _IncludedListenerObject;


  public void TriggerInteractionEnter(){
    gameObject.SendMessage("InteractableInterface_InteractableEnter", SendMessageOptions.DontRequireReceiver);

    foreach(GameObject _listener in _IncludedListenerObject)
      _listener.SendMessage("InteractableInterface_InteractableEnter", SendMessageOptions.DontRequireReceiver);
  }

  public void TriggerInteractionExit(){
    gameObject.SendMessage("InteractableInterface_InteractableExit", SendMessageOptions.DontRequireReceiver);

    foreach(GameObject _listener in _IncludedListenerObject)
      _listener.SendMessage("InteractableInterface_InteractableExit", SendMessageOptions.DontRequireReceiver);
  }


  public void TriggerInteract(){
    gameObject.SendMessage("InteractableInterface_Interact", SendMessageOptions.DontRequireReceiver);

    foreach(GameObject _listener in _IncludedListenerObject)
      _listener.SendMessage("InteractableInterface_Interact", SendMessageOptions.DontRequireReceiver);
  }
}