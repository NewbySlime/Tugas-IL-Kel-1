using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  public class SetLastCheckpointSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "set_last_checkpoint";

    public struct SequenceData{
      public ObjectReference.ObjRefID CheckpointRef;
    }


    private GameHandler _game_handler;

    private SequenceData _seq_data;

    
    public void Start(){
      _game_handler = FindAnyObjectByType<GameHandler>();
      if(_game_handler == null){
        Debug.LogError("Cannot find GameHandler.");
        throw new MissingReferenceException();
      }
    }


    public void StartTriggerAsync(){
      GameObject _ref_obj = ObjectReference.GetReferenceObject(_seq_data.CheckpointRef);
      if(_ref_obj == null){
        Debug.LogError(string.Format("Referenced Object is null. (RefID: {0})", _seq_data.CheckpointRef));
        return;
      }

      CheckpointHandler _checkpoint = _ref_obj.GetComponent<CheckpointHandler>();
      if(_checkpoint == null){
        Debug.LogError(string.Format("Referenced Object does not have CheckpointHandler. ({0}, RefID: {1})", _ref_obj.name, _seq_data.CheckpointRef));
        return;
      }

      _game_handler.SetLastCheckpoint(_checkpoint.CheckpointID);
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


  [UnitTitle("Set Last Checkpoint")]
  [UnitCategory("Sequence/Game")]
  public class SetLastCheckpointSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _checkpoint_ref_input;


    protected override void Definition(){
      base.Definition();

      _checkpoint_ref_input = ValueInput<ObjectReference.ObjRefID>("CheckpointRef");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetLastCheckpointSequence.SequenceID,
        SequenceData = new SetLastCheckpointSequence.SequenceData{
          CheckpointRef = flow.GetValue<ObjectReference.ObjRefID>(_checkpoint_ref_input)
        }
      };
    }
  }
}