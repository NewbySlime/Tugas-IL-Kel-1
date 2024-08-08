using System.Data;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system to set the target object the enemy will attack. 
  /// </summary>
  public class SetEnemyTargetSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "set_enemy_target";

    public struct SequenceData{
      /// <summary>
      /// The object that will be targeted by the enemy.
      /// </summary>
      public ObjectReference.ObjRefID TargetObj;
      
      /// <summary>
      /// The enemy object.
      /// </summary>
      public ObjectReference.ObjRefID EnemyObj;
    }


    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      GameObject _target_obj = ObjectReference.GetReferenceObject(_seq_data.TargetObj);
      if(_target_obj == null){
        Debug.LogError(string.Format("Referenced Target Object is null. (RefID: {0})", _seq_data.TargetObj));
        return;
      }


      GameObject _enemy_obj = ObjectReference.GetReferenceObject(_seq_data.EnemyObj);
      if(_enemy_obj == null){
        Debug.LogError(string.Format("Referenced Enemy Object is null. (RefID: {0})", _seq_data.EnemyObj));
        return;
      }

      InterfaceEnemyBehaviour _enemy_behaviour = _enemy_obj.GetComponent<InterfaceEnemyBehaviour>();
      if(_enemy_behaviour == null){
        Debug.LogError(string.Format("Referenced Enemy Object does not have InterfaceEnemyBehaviour. (RefID: {0})", _seq_data.EnemyObj));
        return;
      } 


      _enemy_behaviour.SetEnemy(_target_obj);
    }

    public bool IsTriggering(){
      return false;
    }


    public string GetSequenceID(){
      return SequenceID;
    }

    public void SetSequenceData(object data){
      if(data is not SequenceData){
        Debug.LogError("Data is not SequenceData.");
        return;
      }

      _seq_data = (SequenceData)data;
    }
  }


  [UnitTitle("Set Enemy Target")]
  [UnitCategory("Sequence/Enemy")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="SetEnemyTargetSequence"/>.
  /// </summary>
  public class SetEnemyTargetSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _target_ref_input;
    [DoNotSerialize]
    private ValueInput _enemy_ref_input;


    protected override void Definition(){
      base.Definition();

      _target_ref_input = ValueInput<ObjectReference.ObjRefID>("TargetRef");
      _enemy_ref_input = ValueInput<ObjectReference.ObjRefID>("EnemyRef");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetEnemyTargetSequence.SequenceID,
        SequenceData = new SetEnemyTargetSequence.SequenceData{
          TargetObj = flow.GetValue<ObjectReference.ObjRefID>(_target_ref_input),
          EnemyObj = flow.GetValue<ObjectReference.ObjRefID>(_enemy_ref_input)
        }
      };
    }
  }
}