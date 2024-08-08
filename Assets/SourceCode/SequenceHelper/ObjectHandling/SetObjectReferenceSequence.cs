using UnityEngine;
using Unity.VisualScripting;
using System;
using JetBrains.Annotations;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system to set object within certain <see cref="ObjectReference.ObjRefID"/> to a new ID.
  /// NOTE: needed a Sequence Object since the referenced object's lifetime started on runtime, not at scene starting (or changing scene).
  /// </summary>
  public class SetObjectReferenceSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "set_object_reference";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// The target object to be referenced using the new ID.
      /// </summary>
      public ObjectReference.ObjRefID RefID;

      /// <summary>
      /// The new ID for the reference.
      /// </summary>
      public string NewRefID;
    }

    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      DEBUGModeUtils.LogWarning(string.Format("reference id ({0})", _seq_data.NewRefID));
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
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="SetObjectReferenceSequence"/>.
  /// </summary>
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