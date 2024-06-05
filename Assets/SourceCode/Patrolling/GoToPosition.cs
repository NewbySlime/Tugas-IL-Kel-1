using UnityEngine;
using Unity.VisualScripting;
using System.Collections;
using System;


namespace PatrolActions{
  public class GoToPosition: PatrolAction{
    public Vector3 TargetPosition;

    public Type GetPatrolType(){
      return typeof(GoToPosition);
    }

    public IEnumerator DoAction(PatrolBehaviour behaviour){
      PathFollower _follower = behaviour._PathFollower;
      _follower.CancelMoving();

      _follower.FollowPathAsync(TargetPosition);
      yield return new WaitUntil(() => !_follower.IsMoving());
    }
  }


  [UnitTitle("GoToPosition")]
  [UnitCategory("Patrol")]
  public class GoToPositionUnit: PatrolNodeBase{
    [DoNotSerialize]
    private ValueInput _position_input;

    protected override void Definition(){
      base.Definition();

      _position_input = ValueInput("Position", Vector3.zero);
    }

    protected override void AddData(Flow flow, out PatrolAction action){
      action = new GoToPosition{
        TargetPosition = flow.GetValue<Vector3>(_position_input)
      };
    }
  }
}