using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


[RequireComponent(typeof(CircleCollider2D))]
public class SoundAlertTransceiver: MonoBehaviour{
  [Serializable]
  public struct AlertConfig{
    public float SoundRangeMax;
    public float MaxCollisionSpeedReference;
  }


  private CircleCollider2D _collider2d;
  private AlertConfig _this_config;

  private HashSet<SoundAlertReceiver> _alert_receiver_list = new();

  private Vector3 _last_pos;
  private float _current_speed;


  private IEnumerator _trigger_sound(){
    _collider2d.radius = Mathf.Clamp(
      _this_config.SoundRangeMax,
      0,
      _current_speed/_this_config.MaxCollisionSpeedReference * _this_config.SoundRangeMax
    );

    yield return new WaitForFixedUpdate();
    yield return new WaitForEndOfFrame();

    foreach(SoundAlertReceiver _receiver in _alert_receiver_list)
      _receiver.TriggerHear(gameObject);

    Debug.Log("receiver triggered");

    // test
    yield return new WaitForSeconds(1);
    _collider2d.radius = 0;
  }


  public void Start(){
    _collider2d = GetComponent<CircleCollider2D>();
    _collider2d.radius = 0;
  }

  public void FixedUpdate(){
    _current_speed = (transform.position-_last_pos).magnitude/Time.fixedDeltaTime;

    _last_pos = transform.position;
  }


  public void SetAlertConfig(AlertConfig config){
    _this_config = config;
  }

  public void TriggerSound(){
    StartCoroutine(_trigger_sound());
  }


  public void OnTriggerEnter2D(Collider2D collider){
    Debug.Log(string.Format("receiver added {0}", collider.gameObject.name));
    SoundAlertReceiver _receiver = collider.gameObject.GetComponent<SoundAlertReceiver>();
    if(_receiver == null)
      return;

    Debug.Log("receiver actually added");
    _alert_receiver_list.Add(_receiver);
  }

  public void OnTriggerExit2D(Collider2D collider){
    SoundAlertReceiver _receiver = collider.gameObject.GetComponent<SoundAlertReceiver>();
    if(_receiver == null || !_alert_receiver_list.Contains(_receiver))
      return;

    _alert_receiver_list.Remove(_receiver);
  }
}