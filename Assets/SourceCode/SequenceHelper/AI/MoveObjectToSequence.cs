using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  public class MoveObjectToSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "move_object_to";

    public struct SequenceData{
      public Vector3 Position;
      public ObjectReference.ObjRefID RefID;
    }

    private SequenceData _seq_data;

    private bool _sequence_triggering = false;


    private IEnumerator _start_trigger(){
      GameObject _obj = ObjectReference.GetReferenceObject(_seq_data.RefID);
      if(_obj == null){
        Debug.LogError(string.Format("Cannot get Object. (Ref: {0})", _seq_data.RefID));
        yield break;
      }

      PathFollower _path_follower = _obj.GetComponent<PathFollower>();
      if(_path_follower == null){
        Debug.LogError(string.Format("Object does not have PathFollower. (Ref: {0})", _seq_data.RefID));
        yield break;
      }

      _sequence_triggering = true;

      _path_follower.FollowPathAsync(_seq_data.Position);
      yield return new WaitUntil(() => {
        if(_path_follower.IsStuck()){
          _obj.transform.position = _seq_data.Position;
          return true;
        }
        else if(!_path_follower.IsMoving())
          return true;

        return false;
      });

      _sequence_triggering = false;
    }


    public void StartTriggerAsync(){
      StartCoroutine(_start_trigger());
    }

    public bool IsTriggering(){
      return _sequence_triggering;
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


  [UnitTitle("Move Object To Pos")]
  [UnitCategory("Sequence/AI")]
  public class MoveObjectToSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _pos_input;
    [DoNotSerialize]
    private ValueInput _obj_input;


    protected override void Definition(){
      base.Definition();

      _pos_input = ValueInput("Position", Vector3.zero);
      _obj_input = ValueInput<ObjectReference.ObjRefID>("ObjectRef");
    }


    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = MoveObjectToSequence.SequenceID,
        SequenceData = new MoveObjectToSequence.SequenceData{
          Position = flow.GetValue<Vector3>(_pos_input),
          RefID = flow.GetValue<ObjectReference.ObjRefID>(_obj_input)
        }
      };
    }
  }
}