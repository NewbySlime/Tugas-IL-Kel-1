using System.Collections;
using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  public class SceneSequenceTrigger: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "sequence_trigger";

    public struct SequenceData{
      public string SequenceID;
    }


    private SequenceData _seq_data;

    private bool _is_triggering = false;


    private IEnumerator _start_sequence(){
      _is_triggering = true;
      
      while(true){
        LevelSequenceDatabase _seq_database = FindAnyObjectByType<LevelSequenceDatabase>();
        if(_seq_database == null){
          Debug.LogWarning("Cannot get database of Level Sequences.");
          break;
        }

        if(!_seq_database.HasSequence(_seq_data.SequenceID)){
          Debug.LogWarning(string.Format("Cannot get Sequence with ID:'{0}'", _seq_data.SequenceID));
          break;
        }

        yield return _seq_database.StartSequence(_seq_data.SequenceID);
        break;
      }

      _is_triggering = false;
    }


    public void Start(){
      GameHandler _game_handler = FindAnyObjectByType<GameHandler>();
      if(_game_handler == null){
        Debug.LogError("Cannot get Game Handler.");
        throw new MissingComponentException();
      }
    }


    public void StartTriggerAsync(){
      StartCoroutine(_start_sequence());
    }

    public bool IsTriggering(){
      return _is_triggering;
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


  [UnitTitle("Sequence Trigger")]
  [UnitCategory("Sequence")]
  public class SceneSequenceTriggerVS: AddSubSequence{
    [DoNotSerialize]
    public ValueInput _sequence_id_input;

    protected override void Definition(){
      base.Definition();

      _sequence_id_input = ValueInput("SequenceID", "");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new SequenceHandlerVS.SequenceInitializeData.DataPart{
        SequenceID = SceneSequenceTrigger.SequenceID,
        SequenceData = new SceneSequenceTrigger.SequenceData{
          SequenceID = flow.GetValue<string>(_sequence_id_input)
        }
      };
    }
  }
}