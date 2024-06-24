using UnityEngine;
using Unity.VisualScripting;

namespace SequenceHelper{
  public class EnableTriggerActuatorSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "enable_trigger_actuator";

    public struct SequenceData{
      public ObjectReference.ObjRefID TargetRef;

      public bool TriggerEnable;
    }


    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      GameObject _target_obj = ObjectReference.GetReferenceObject(_seq_data.TargetRef);
      if(_target_obj == null){
        Debug.LogError(string.Format("Referenced Object is null. (RefID: {0})", _seq_data.TargetRef));
        return;
      }

      AreaTriggerActuator _actuator = _target_obj.GetComponent<AreaTriggerActuator>();
      if(_actuator == null){
        Debug.LogError(string.Format("Referenced Object does not have AreaTriggerActuator. ({0}, RefID: {1})", _target_obj.name, _seq_data.TargetRef));
        return;
      }

      _actuator.TriggerOnEnter = _seq_data.TriggerEnable;
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


  [UnitTitle("Enable Actuator")]
  [UnitCategory("Sequence/Game/AreaTrigger")]
  public class EnableTriggerActuatorSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _target_obj_input;

    [DoNotSerialize]
    private ValueInput _enable_input;


    protected override void Definition(){
      base.Definition();

      _target_obj_input = ValueInput<ObjectReference.ObjRefID>("TargetObjRef");
      _enable_input = ValueInput("EnableTrigger", true);
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = EnableTriggerActuatorSequence.SequenceID,
        SequenceData = new EnableTriggerActuatorSequence.SequenceData{
          TargetRef = flow.GetValue<ObjectReference.ObjRefID>(_target_obj_input),
          TriggerEnable = flow.GetValue<bool>(_enable_input)
        }
      };
    }
  }
}