using Unity.VisualScripting;
using System;
using System.Collections;


namespace PatrolActions{
  public class WaitForSeconds: PatrolAction{
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