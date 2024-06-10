using System.Collections.Generic;
using UnityEngine;


public class SetActiveInteraction: MonoBehaviour{
  [SerializeField]
  private List<GameObject> _ListListenerObject;


  private void _set_active_on_list(bool active){
    foreach(GameObject _obj in _ListListenerObject)
      _obj.SetActive(active);
  }


  public void InteractableInterface_Interact(){
    _set_active_on_list(true);
  }


  public void InteractableInterface_InteractableExit(){
    _set_active_on_list(false);
  }
}