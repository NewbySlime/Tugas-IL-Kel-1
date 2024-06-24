using UnityEngine;
using Unity.VisualScripting;
using System.Collections;


public class SetActiveUIOnTimeout: TimingBaseUI{
  [SerializeField]
  private GameObject _TargetObject;

  [DoNotSerialize]
  public bool SetActiveTarget = true;

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