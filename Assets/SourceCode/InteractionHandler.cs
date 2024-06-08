using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;



// Ini adalah handler untuk interaksi antara player dan objek lainnya.
// Cara kerjanya,
//    Player mempunyai salah satu InteractionHandler yang akan dipakai ketika player memencet tombol "Interaksi" (tombol F contohnya).
//    Untuk penerimanya ada InteractableInterface yang menerima arahan dari Handler ini yang dimana akan digunakan sebagai interaksi player.
//    Secara teknis, ini bekerja dengan handler menggunakan Trigger dari komponen Collision. Setiap ada object yang masuk, Handler akan menyimpan objek yang datang sebagai objek yang akan diberikan arahan ketika player berinteraksi.
//
// Untuk developer, fokus kepada ketiga fungsi utama yang dipakai ketika ada interaksi antara player dan objek.
// Yaitu: Interaction_OnEnter, Interaction_OnExit, TriggerInteraction   
public class InteractionHandler: MonoBehaviour{
  // Objek Relay yang dimana Handler ini akan menerima event\ "Collision" dari Rigidbody2D ataupun Collision2D
  [SerializeField]
  private RigidbodyMessageRelay _InteractionTrigger;


  private HashSet<InteractableInterface> _interactable_list = new();


  public void Start(){
    if(_InteractionTrigger == null) 
      Debug.LogWarning("No Trigger for Interaction can be used.");
    else{
      _InteractionTrigger.OnTriggerEntered2DEvent += Interaction_OnEnter;
      _InteractionTrigger.OnTriggerExited2DEvent += Interaction_OnExit;
    }
  }


  // Fungsi ini dipakai untuk menyimpan objek yang diberikan saat Collider masuk ke Trigger Collider.
  // Kemudian fungsi ini dilempar ke objek yang masuk dengan fungsi InteractableInterface.TriggerInteractionEnter() 
  public void Interaction_OnEnter(Collider2D collider){
    InteractableInterface _interface = collider.gameObject.GetComponent<InteractableInterface>();
    if(_interface == null)
      return;

    _interactable_list.Add(_interface);
    _interface.TriggerInteractionEnter();
  }

  // Fungsi ini dipakai untuk melepas objek yang diberikan saat Collider masuk ke Trigger Collider.
  // Kemudian fungsi ini dilempar ke objek yang masuk dengan fungsi InteractableInterface.TriggerInteractionExit() 
  public void Interaction_OnExit(Collider2D collider){
    InteractableInterface _interface = collider.gameObject.GetComponent<InteractableInterface>();
    if(_interface == null)
      return;

    _interactable_list.Remove(_interface);
    _interface.TriggerInteractionExit();
  }

  // Fungsi ini dipakai oleh Player untuk melakukan fungsi "Interaction" ke objek objek target player.
  // Kemudian fungsi ini dilempar ke objek yang masuk dengan fungsi InteractableInterface.TriggerInteract()
  public bool TriggerInteraction(){
    if(_interactable_list.Count <= 0)
      return false;

    InteractableInterface _nearest_obj = null;
    float _nearest_dist = float.PositiveInfinity;
    foreach(InteractableInterface _interface in _interactable_list){
      float _dist = (transform.position - _interface.transform.position).magnitude;
      if(_dist < _nearest_dist){
        _nearest_obj = _interface;
        _nearest_dist = _dist;
      }
    }

    _nearest_obj.TriggerInteract();
    Debug.Log("interaction do");

    return true;
  }
}