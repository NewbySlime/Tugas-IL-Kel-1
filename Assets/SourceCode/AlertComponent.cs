using System.Collections.Generic;
using UnityEngine;


public class AlertComponent: MonoBehaviour{
  [SerializeField]
  private float _AlertTimeout;

  [SerializeField]
  private GameObject _AlertRigidbody;


  private float _alert_timer;
  private bool _alerted = false;

  private HashSet<GameObject> _object_contained;

  
  private void Alert_OnTriggerEnter2D(Collider2D collider){
    if(_object_contained.Count <= 0)
      _alert_timer = _AlertTimeout;

    _object_contained.Add(collider.gameObject);
  }

  private void Alert_OnTriggerExit2D(Collider2D collider){
    _object_contained.Remove(collider.gameObject);
  }


  private void _DoAlert(){
    _alerted = true;
    gameObject.SendMessage("AlertComponent_OnAlerted");
  }


  public void Start(){
    RigidbodyMessageRelay _alert_messsge_relay = _AlertRigidbody.GetComponent<RigidbodyMessageRelay>();
    if(_alert_messsge_relay != null){
      _alert_messsge_relay.OnTriggerEntered2DEvent += Alert_OnTriggerEnter2D;
      _alert_messsge_relay.OnTriggerExited2DEvent += Alert_OnTriggerExit2D;
    }
    else
      Debug.LogError("Cannot get AlertComponent's RigidbodyMessageRelay.");
  }


  public void FixedUpdate(){
    if(_object_contained.Count > 0){
      if(!_alerted){
        _alert_timer -= Time.fixedDeltaTime;
        if(_alert_timer <= 0)
          _DoAlert();
      }
    }
  }


  public void ResetAlerted(){
    _alerted = false;
  }

  public bool GetIsAlerted(){
    return _alerted;
  }
}