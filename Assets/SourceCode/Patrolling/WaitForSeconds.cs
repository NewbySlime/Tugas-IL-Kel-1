using Unity.VisualScripting;
using System;
using System.Collections;


namespace PatrolActions{
  /// <summary>
  /// An extension of <see cref="PatrolAction"/> that makes the target patrol object to wait for certain second(s).
  /// </summary>
  public class WaitForSeconds: PatrolAction{
    /// <summary>
    /// Second(s) to wait.
    /// </summary>
    public float WaitSeconds;

    public Type GetPatrolType(){
      return typeof(WaitForSeconds);
    }

    public IEnumerator DoAction(PatrolBehaviour behaviour){
      yield return new UnityEngine.WaitForSeconds(WaitSeconds);
    }
  }


  [UnitTitle("WaitForSeconds")]
  [UnitCategory("Patrol")]
  /// <summary>
  /// An extension of <see cref="PatrolNodeBase"/> for <see cref="WaitForSeconds"/>.  
  /// </summary>
  public class WaitForSecondsUnit: PatrolNodeBase{
    [DoNotSerialize]
    private ValueInput _seconds_input;

    protected override void Definition(){
      base.Definition();

      _seconds_input = ValueInput("Time (s)", 1f);
    }

    protected override void AddData(Flow flow, out PatrolAction action){
      action = new WaitForSeconds{
        WaitSeconds = flow.GetValue<float>(_seconds_input)
      };
    }
  }
}