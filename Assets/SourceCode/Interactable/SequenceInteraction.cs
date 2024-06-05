using UnityEngine;
using Unity.VisualScripting;


[RequireComponent(typeof(InteractableInterface))]
public class SequenceInteraction: MonoBehaviour{
  [SerializeField]
  private SequenceHandlerVS _SequenceHandler;

  
  public void InteractableInterface_Interact(){
    _SequenceHandler.StartTriggerAsync();
  }
}