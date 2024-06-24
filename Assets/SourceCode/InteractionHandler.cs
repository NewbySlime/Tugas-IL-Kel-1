using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;



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

  private InteractableInterface _current_interactable = null;


  private List<InteractableInterface> _get_nearest_objects(){
    List<InteractableInterface> _interface_list = _interactable_list.ToList();
    _interface_list.Sort((InteractableInterface var1, InteractableInterface var2) => {
      float _dist_from1 = (transform.position - var1.transform.position).magnitude;
      float _dist_from2 = (transform.position - var2.transform.position).magnitude;

      return _dist_from1 > _dist_from2? 1: -1;
    });

    List<InteractableInterface> _result = new();
    foreach(InteractableInterface _interface in _interface_list){
      _result.Add(_interface);

      if(!_interface.PassThrough)
        break;
    }

    return _result;
  }

  private void _trigger_enter_objects(List<InteractableInterface> list_interface){
    HashSet<InteractableInterface> _exit_set = new(_interactable_list);
    foreach(InteractableInterface _interface_obj in list_interface){
      _exit_set.Remove(_interface_obj);
      if(_interface_obj.InteractOnAvailable)
        continue;

      _interface_obj.TriggerInteractionEnter();
    }

    // trigger exit to not accounted objs
    foreach(InteractableInterface _interface_obj in _exit_set)
      _interface_obj.TriggerInteractionExit();
  }


  private void _change_current(InteractableInterface interactable){
    if(interactable == _current_interactable)
      return;

    if(_current_interactable != null){
      _current_interactable.TriggerInteractionExit();
    }

    _current_interactable = interactable;
    if(interactable != null){
      interactable.TriggerInteractionEnter();
    }
  }


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

    List<InteractableInterface> _nearest_objs = _get_nearest_objects();
    _trigger_enter_objects(_nearest_objs);

    _change_current(_nearest_objs.Count > 0? _nearest_objs[_nearest_objs.Count-1]: null);
  }

  // Fungsi ini dipakai untuk melepas objek yang diberikan saat Collider masuk ke Trigger Collider.
  // Kemudian fungsi ini dilempar ke objek yang masuk dengan fungsi InteractableInterface.TriggerInteractionExit() 
  public void Interaction_OnExit(Collider2D collider){
    InteractableInterface _interface = collider.gameObject.GetComponent<InteractableInterface>();
    if(_interface == null)
      return;

    _interactable_list.Remove(_interface);
    _interface.TriggerInteractionExit();

    List<InteractableInterface> _nearest_objs = _get_nearest_objects();
    _trigger_enter_objects(_nearest_objs);

    _change_current(_nearest_objs.Count > 0? _nearest_objs[_nearest_objs.Count-1]: null);
  }

  // Fungsi ini dipakai oleh Player untuk melakukan fungsi "Interaction" ke objek objek target player.
  // Kemudian fungsi ini dilempar ke objek yang masuk dengan fungsi InteractableInterface.TriggerInteract()
  public bool TriggerInteraction(){
    if(_current_interactable == null)
      return false;

    _current_interactable.TriggerInteract();
    return true;
  }
}