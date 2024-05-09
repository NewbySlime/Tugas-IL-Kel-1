using UnityEngine;



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


  public void InteractableInterface_InteractableEnter(){
    if(_HintContainer == null)
      return;

    _interactable_enter = true;
    _HintContainer.SetActive(true);
  }
  
  public void InteractableInterface_InteractableExit(){
    if(_HintContainer == null)
      return;

    _interactable_enter = false;
    _HintContainer.SetActive(false);
  }
}