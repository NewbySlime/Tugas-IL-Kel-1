using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


[RequireComponent(typeof(CircleCollider2D))]
/// <summary>
/// Class to "transmit sound" that can be received by <see cref="SoundAlertReceiver"/>. This class is used to create an "imaginary sound" using an area of effect to accomodate Unity's Sound system for a Game object's ability to "hear" a sound that can be used to trigger a flag in the object.
/// For further explanation, see <b>Reference/Diagrams/SoundAlert.drawio</b>
/// 
/// This class uses following component(s);
/// - Unity's <b>CircleCollider2D</b> physics body (as trigger) for the area of effect to capture nearby <see cref="SoundAlertReceiver"/> objects.
/// </summary>
public class SoundAlertTransceiver: MonoBehaviour{
  [Serializable]
  /// <summary>
  /// Configuration for using <see cref="SoundAlertTransceiver"/>'s "sound" transmitting function.
  /// </summary>
  public struct AlertConfig{
    public float SoundRangeMax;

    /// <summary>
    /// Speed reference for area of effect's range. This variable is calculated alongside <see cref="SoundRangeMax"/> to get the resulting range.
    /// </summary>
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

    DEBUGModeUtils.Log("receiver triggered");

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


  /// <summary>
  /// Sets the configuration for transmitting "sound".
  /// </summary>
  /// <param name="config">The new configuration</param>
  public void SetAlertConfig(AlertConfig config){
    _this_config = config;
  }

  /// <summary>
  /// Trigger transmitting "sound" to nearby <see cref="SoundAlertReceiver"/>.
  /// </summary>
  public void TriggerSound(){
    StartCoroutine(_trigger_sound());
  }


  /// <summary>
  /// Function to catch <b>CircleCollder2D</b> event for when an object has entered its body.
  /// This function is to add an object (if it is <see cref="SoundAlertReceiver"/>) to be added to "nearby" list.
  /// </summary>
  /// <param name="collider">The body that entered</param>
  public void OnTriggerEnter2D(Collider2D collider){
    DEBUGModeUtils.Log(string.Format("receiver added {0}", collider.gameObject.name));
    SoundAlertReceiver _receiver = collider.gameObject.GetComponent<SoundAlertReceiver>();
    if(_receiver == null)
      return;

    DEBUGModeUtils.Log("receiver actually added");
    _alert_receiver_list.Add(_receiver);
  }

  /// <summary>
  /// Function to catch <b>CircleCollider2D</b> event for when an object has exited its body.
  /// This function si to remove an object from "nearby" list.
  /// </summary>
  /// <param name="collider">The body that exited</param>
  public void OnTriggerExit2D(Collider2D collider){
    SoundAlertReceiver _receiver = collider.gameObject.GetComponent<SoundAlertReceiver>();
    if(_receiver == null || !_alert_receiver_list.Contains(_receiver))
      return;

    _alert_receiver_list.Remove(_receiver);
  }
}