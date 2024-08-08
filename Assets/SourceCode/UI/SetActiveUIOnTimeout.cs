using UnityEngine;
using Unity.VisualScripting;
using System.Collections;


/// <summary>
/// <see cref="TimingBaseUI"/> extension effect that lets the component enable/disable the target object when the timer is running out. 
/// </summary>
public class SetActiveUIOnTimeout: TimingBaseUI{
  [SerializeField]
  private GameObject _TargetObject;

  [DoNotSerialize]
  /// <summary>
  /// Should the target object be enabled or not after timeout.
  /// </summary>
  public bool SetActiveTarget = true;


  // Coroutine function to wait until the timer is running out.
  private IEnumerator _start_timer(){
    DEBUGModeUtils.Log("starting timer");
    yield return new WaitUntil(() => AllTimerFinished(this));

    DEBUGModeUtils.Log(string.Format("timer finished, {0}", SetActiveTarget));
    _TargetObject.SetActive(SetActiveTarget);
  }


  protected override void _on_timer_started(){
    _TargetObject.SetActive(true);

    StartCoroutine(_start_timer());
  }
}