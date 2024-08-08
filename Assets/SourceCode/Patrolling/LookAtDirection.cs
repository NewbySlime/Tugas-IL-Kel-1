using UnityEngine;
using Unity.VisualScripting;
using System.Collections;


namespace PatrolActions{
  /// <summary>
  /// An extension of <see cref="PatrolAction"/> that handles the object to force look at certain direction.
  /// </summary>
  public class LookAtDirection: PatrolAction{
    /// <summary>
    /// Target direction to look at.
    /// </summary>
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
  /// <summary>
  /// An extension of <see cref="PatrolNodeBase"/> for <see cref="LookAtDirection"/>.
  /// </summary>
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