using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  public class SetLastCheckpointSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "set_last_checkpoint";

    public struct SequenceData{
      public string CheckpointID;
    }


    private GameHandler _game_handler;

    private SequenceData _seq_data;


    public void Start(){
      _game_handler = FindAnyObjectByType<GameHandler>();
      if(_game_handler == null){
        Debug.LogError("Cannot find GameHandler.");
        return;
      }
    }


    public void StartTriggerAsync(){
      LevelCheckpointDatabase _database = FindAnyObjectByType<LevelCheckpointDatabase>();
      if(_database == null)
        Debug.LogWarning("Cannot find database for Checkpoints.");
      else if(_database.GetCheckpoint(_seq_data.CheckpointID) == null)
        Debug.LogWarning(string.Format("Checkpoint (ID: {0}) cannot be found.", _seq_data.CheckpointID));

      _game_handler.SetLastCheckpoint(_seq_data.CheckpointID);
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
    private ValueInput _checkpoint_id_input;


    protected override void Definition(){
      base.Definition();

      _checkpoint_id_input = ValueInput("CheckpointID", "");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetLastCheckpointSequence.SequenceID,
        SequenceData = new SetLastCheckpointSequence.SequenceData{
          CheckpointID = flow.GetValue<string>(_checkpoint_id_input)
        }
      };
    }
  }
}