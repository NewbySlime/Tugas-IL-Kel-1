using UnityEngine;
using Unity.VisualScripting;
using System;
using System.Collections;


namespace PatrolActions{
  /// <summary>
  /// An extension of <see cref="PatrolAction"/> that handles moving to a target <b>Tranform</b>.
  /// </summary>
  public class GoToObject: PatrolAction{
    /// <summary>
    /// Target object to travel to.
    /// </summary>
    public Transform TargetObject;

    public Type GetPatrolType(){
      return typeof(GoToObject);
    }

    public IEnumerator DoAction(PatrolBehaviour behaviour){
      PathFollower _follower = behaviour._PathFollower;

      while(true){
        _follower.CancelMoving();
        _follower.FollowPathAsync(TargetObject.position);
        yield return new UnityEngine.WaitForSeconds(behaviour._PathUpdateInterval);

        if(!_follower.IsMoving())
          break;
      }
    }
  }


  [UnitTitle("GoToObject")]
  [UnitCategory("Patrol")]
  /// <summary>
  /// An extension of <see cref="PatrolNodeBase"/> for <see cref="GoToObject"/>.
  /// </summary>
  public class GoToObjectUnit: PatrolNodeBase{
    [DoNotSerialize]
    private ValueInput _transform_input;

    protected override void Definition(){
      base.Definition();

      _transform_input = ValueInput<Transform>("Target");
    }

    protected override void AddData(Flow flow, out PatrolAction action){
      action = new GoToObject{
        TargetObject = flow.GetValue<Transform>(_transform_input)
      };
    }
  }
}