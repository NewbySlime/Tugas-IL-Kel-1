using System.Collections.Generic;
using UnityEngine;



// Komponen ini bertugas untuk menerima arahan-arahan dari InteractionHandler. Untuk lebih detail, baca komen di InteractionHandler.
// 
// Untuk developer, fokus kepada ketiga fungsi utama yang akan dipakai saat penerimaan arahan. (NOTE: baca dokumentasi dari Unity, https://docs.unity3d.com/ScriptReference/GameObject.SendMessage.html)
// Yaitu;
//    InteractableInterface_InteractableEnter
//      Message yang diberikan ketika ada objek yang masuk ke area Interactable.
//    InteractableInterface_InteractableExit
//      Message yang diberikan ketika ada objek yang keluar dari area Interactable.
//    InteractableInterface_Interact
//      Message yang diberikan ketika Player men-trigger interaksi.
public class InteractableInterface: MonoBehaviour{
  // Ini adalah list Objek yang akan diberikan lebih lanjut arahan-arahan dari Handler.
  [SerializeField]
  private List<GameObject> _IncludedListenerObject;

  // Ini adalah flag ketika suatu objek disarankan untuk langsung "trigger" saat memasuki Area "Interactable".
  [SerializeField]
  private bool _InteractOnAvailable = false;


  public void InteractableInterface_InteractableEnter(){
    if(!gameObject.activeInHierarchy)
      return;
      
    foreach(GameObject _listener in _IncludedListenerObject)
      _listener.SendMessage("InteractableInterface_InteractableEnter", SendMessageOptions.DontRequireReceiver);
  } 

  public void TriggerInteractionEnter(){
    if(!gameObject.activeInHierarchy)
      return;
      
    if(_InteractOnAvailable)
      TriggerInteract();

    gameObject.SendMessage("InteractableInterface_InteractableEnter", SendMessageOptions.DontRequireReceiver);

    InteractableInterface_InteractableEnter();
  }


  public void InteractableInterface_InteractableExit(){
    if(!gameObject.activeInHierarchy)
      return;
      
    foreach(GameObject _listener in _IncludedListenerObject)
      _listener.SendMessage("InteractableInterface_InteractableExit", SendMessageOptions.DontRequireReceiver);
  }

  public void TriggerInteractionExit(){
    if(!gameObject.activeInHierarchy)
      return;
      
    gameObject.SendMessage("InteractableInterface_InteractableExit", SendMessageOptions.DontRequireReceiver);

    InteractableInterface_InteractableExit();
  }


  public void InteractableInterface_Interact(){
    if(!gameObject.activeInHierarchy)
      return;
      
    foreach(GameObject _listener in _IncludedListenerObject)
      _listener.SendMessage("InteractableInterface_Interact", SendMessageOptions.DontRequireReceiver);
  }

  public void TriggerInteract(){
    if(!gameObject.activeInHierarchy)
      return;
      
    gameObject.SendMessage("InteractableInterface_Interact", SendMessageOptions.DontRequireReceiver);

    InteractableInterface_Interact();
  }
}