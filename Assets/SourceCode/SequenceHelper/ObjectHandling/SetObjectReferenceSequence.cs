using UnityEngine;
using Unity.VisualScripting;
using System;
using JetBrains.Annotations;


namespace SequenceHelper{
  // needed a Sequence Object since the referenced object's lifetime started on runtime, not at scene starting (or changing scene)
  public class SetObjectReferenceSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "set_object_reference";

    public struct SequenceData{
      public ObjectReference.ObjRefID RefID;

      public string NewRefID;
    }

    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      Debug.LogWarning(string.Format("reference id ({0})", _seq_data.NewRefID));
      GameObject _game_object = ObjectReference.GetReferenceObject(_seq_data.RefID);
      if(_game_object == null)
        Debug.LogWarning(string.Format("Referenced Object is null, is this intentional? (NewRefID: {0}) (RefID: {1})", _seq_data.NewRefID, _seq_data.RefID));

      ObjectReference.SetReferenceObject(new(){ID = _seq_data.NewRefID}, _game_object);
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


  [UnitTitle("Set Object Reference")]
  [UnitCategory("Sequence/ObjectHandling")]
  public class SetObjectReferenceSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _obj_ref_input;

    [DoNotSerialize]
    private ValueInput _new_ref_id_input;


    protected override void Definition(){
      base.Definition();

      _obj_ref_input = ValueInput<ObjectReference.ObjRefID>("ObjectRef");
      _new_ref_id_input = ValueInput("NewID", "");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetObjectReferenceSequence.SequenceID,
        SequenceData = new SetObjectReferenceSequence.SequenceData{
          RefID = flow.GetValue<ObjectReference.ObjRefID>(_obj_ref_input),
          NewRefID = flow.GetValue<string>(_new_ref_id_input)
        }
      };
    }
  }
}