using UnityEngine;


/// <summary>
/// An actuator component for triggering <see cref="AreaTrigger"/>.
/// </summary>
public class AreaTriggerActuator: MonoBehaviour{
  /// <summary>
  /// Flag for telling <see cref="AreaTrigger"/> or any component that this object can be use as a trigger source or not.
  /// </summary>
  public bool TriggerOnEnter;
}