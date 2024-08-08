using UnityEngine;


[RequireComponent(typeof(Animator))]
/// <summary>
/// Component for distributing Unity's Animation Trigger Event into C# event for evenly distribution to subscribers.
/// NOTE: Unity's Animation Trigger Event only uses string as the event parameter.
/// </summary>
public class AnimationTriggerFlagComponent: MonoBehaviour{
  /// <summary>
  /// Event when a trigger happens from currently played animation.
  /// </summary>
  public event AnimationTrigger AnimationTriggerEvent;
  public delegate void AnimationTrigger(string trigger_name);

  /// <summary>
  /// Function that can be used by Unity's animation system to trigger an event. This event will be redistributed using C# event.
  /// </summary>
  /// <param name="trigger_name">Data related to the trigger event</param>
  public void TriggerFlag(string trigger_name){
    AnimationTriggerEvent?.Invoke(trigger_name);
  }
}