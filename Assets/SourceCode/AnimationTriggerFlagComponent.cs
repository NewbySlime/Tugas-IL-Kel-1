using UnityEngine;


[RequireComponent(typeof(Animator))]
public class AnimationTriggerFlagComponent: MonoBehaviour{
  public delegate void AnimationTrigger(string trigger_name);
  public event AnimationTrigger AnimationTriggerEvent;

  public void TriggerFlag(string trigger_name){
    AnimationTriggerEvent?.Invoke(trigger_name);
  }
}