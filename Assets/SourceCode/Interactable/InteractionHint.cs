using UnityEngine;



/// <summary>
/// A class that is used for "activate" or show/hide the target container (<b>GameObject</b>) that is used for Hint that the object can be interacted.
/// </summary>
public class InteractionHint: MonoBehaviour{
  [SerializeField]
  private GameObject _HintContainer;

  private bool _interactable_enter = false;


  public void Start(){
    if(_HintContainer == null)
      Debug.LogWarning("No Hint Container to use.");
    else{
      _HintContainer.SetActive(false);
    }
  }


  /// <summary>
  /// Interface function used by <see cref="InteractableInterface"/> for when an "InteractableEnter" event is triggered, which means the Interaction border is being registered.
  /// </summary>
  public void InteractableInterface_InteractableEnter(){
    if(_HintContainer == null)
      return;

    _interactable_enter = true;
    _HintContainer.SetActive(true);
  }
  
  /// <summary>
  /// Interface function used by <see cref="InteractableInterface"/> for when an "InteractableExit" event is triggered, which means the Interaction border no longer register the entered object.
  /// </summary>
  public void InteractableInterface_InteractableExit(){
    if(_HintContainer == null)
      return;

    _interactable_enter = false;
    _HintContainer.SetActive(false);
  }
}