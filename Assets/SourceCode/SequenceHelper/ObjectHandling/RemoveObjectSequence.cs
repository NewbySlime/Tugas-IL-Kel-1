using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system for removing target object based on <see cref="ObjectReference.ObjRefID"/>.
  /// </summary>
  public class RemoveObjectSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for reistering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "remove_object";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// The target object to be removed for the Game.
      /// </summary>
      public ObjectReference.ObjRefID RefID;
    }


    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      GameObject _ref_obj = ObjectReference.GetReferenceObject(_seq_data.RefID);
      if(_ref_obj == null){
        Debug.LogWarning(string.Format("Reference object is null. (RefID: {0})", _seq_data.RefID));
        return;
      }

      Destroy(_ref_obj);
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


  [UnitTitle("Remove Object")]
  [UnitCategory("Sequence/Object")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="RemoveObjectSequence"/>.
  /// </summary>
  public class RemoveObjectSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _ref_id_input;


    protected override void Definition(){
      base.Definition();

      _ref_id_input = ValueInput<ObjectReference.ObjRefID>("ObjectRef");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = RemoveObjectSequence.SequenceID,
        SequenceData = new RemoveObjectSequence.SequenceData{
          RefID = flow.GetValue<ObjectReference.ObjRefID>(_ref_id_input)
        }
      };
    }
  }
}