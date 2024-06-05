using UnityEngine;
using Unity.VisualScripting;
using System.Collections;


namespace PatrolActions{
  public class LookAtDirection: PatrolAction{
    public Vector2 Direction;

    public System.Type GetPatrolType(){
      return typeof(LookAtDirection);
    }

    public IEnumerator DoAction(PatrolBehaviour behaviour){
      MovementController _movement = behaviour._MovementController;
      _movement.LookAt(Direction);

      yield break;
    }
  }


  [UnitTitle("LookAtDirection")]
  [UnitCategory("Patrol")]
  public class LookAtDirectionUnit: PatrolNodeBase{
    [DoNotSerialize]
    private ValueInput _direction_input;

    protected override void Definition(){
      base.Definition();

      _direction_input = ValueInput("Direction", Vector2.zero);
    }

    protected override void AddData(Flow flow, out PatrolAction action){
      action = new LookAtDirection{
        Direction = flow.GetValue<Vector2>(_direction_input)
      };
    }
  }
}