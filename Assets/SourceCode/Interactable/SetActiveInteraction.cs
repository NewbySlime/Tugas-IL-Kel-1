using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class that is used for "activate" or show/hide the multiple target (<b>GameObject</b>) depending on the interaction.
/// </summary>
public class SetActiveInteraction: MonoBehaviour{
  [SerializeField]
  private List<GameObject> _ListListenerObject;


  private void _set_active_on_list(bool active){
    foreach(GameObject _obj in _ListListenerObject)
      _obj.SetActive(active);
  }


  /// <summary>
  /// Interface function used by <see cref="InteractableInterface"/> for when an "Interact" event is triggered.
  /// This function activates/enables the target objects.
  /// </summary>
  public void InteractableInterface_Interact(){
    _set_active_on_list(true);
  }


  /// <summary>
  /// Interface function used by <see cref="InteractableInterface"/> for when an "InteractableExit" event is triggered, which means the Interaction border no longer register the entered object.
  /// This function disables the target objects.
  /// </summary>
  public void InteractableInterface_InteractableExit(){
    _set_active_on_list(false);
  }
}