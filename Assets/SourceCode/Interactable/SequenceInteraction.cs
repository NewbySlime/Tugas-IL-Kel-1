using UnityEngine;
using Unity.VisualScripting;


[RequireComponent(typeof(InteractableInterface))]
/// <summary>
/// A class that used for trigger sequencing created with Unity Visual Script.
/// 
/// This class uses Component(s);
/// - <see cref="SequenceHandlerVS"/> for the handling of the sequencing.
/// </summary>
public class SequenceInteraction: MonoBehaviour{
  [SerializeField]
  private SequenceHandlerVS _SequenceHandler;

  
  /// <summary>
  /// Interface function used by <see cref="InteractableInterface"/> for when an "Interact" event is triggered. 
  /// </summary>
  public void InteractableInterface_Interact(){
    _SequenceHandler.StartTriggerAsync();
  }
}