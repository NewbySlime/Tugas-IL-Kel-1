using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system for setting the "enabled" flag to the target object.
  /// </summary>
  public class SetObjectActiveSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "set_object_active";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// The target object to be "enabled".
      /// </summary>
      public ObjectReference.ObjRefID RefID;

      /// <summary>
      /// Enable flag.
      /// </summary>
      public bool Active;
    }


    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      GameObject _ref_obj = ObjectReference.GetReferenceObject(_seq_data.RefID);
      if(_ref_obj == null){
        Debug.LogError(string.Format("Reference Object is not found. (RefID: {0})", _seq_data.RefID));
        return;
      }

      _ref_obj.SetActive(_seq_data.Active);
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


  [UnitTitle("Set Object Active")]
  [UnitCategory("Sequence/Object")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="SetObjectActiveSequence"/>.
  /// </summary>
  public class SetObjectActiveSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _ref_id_input;
    [DoNotSerialize]
    private ValueInput _active_input;


    protected override void Definition(){
      base.Definition();

      _ref_id_input = ValueInput<ObjectReference.ObjRefID>("ObjectRef");
      _active_input = ValueInput("SetActive", true);
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetObjectActiveSequence.SequenceID,
        SequenceData = new SetObjectActiveSequence.SequenceData{
          RefID = flow.GetValue<ObjectReference.ObjRefID>(_ref_id_input),
          Active = flow.GetValue<bool>(_active_input)
        }
      };
    }
  }
}