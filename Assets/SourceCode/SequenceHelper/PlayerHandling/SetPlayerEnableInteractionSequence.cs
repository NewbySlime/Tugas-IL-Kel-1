using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  public class SetPlayerEnableInteractionSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "set_player_enable_interaction";

    public struct SequenceData{
      public bool Enable;
    }


    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      PlayerController _player = FindAnyObjectByType<PlayerController>();
      if(_player == null){
        Debug.LogError("Cannot find PlayerController.");
        return;
      }

      _player.SetEnableInteraction(_seq_data.Enable);
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


  [UnitTitle("Set Enable Interaction")]
  [UnitCategory("Sequence/Player")]
  public class SetPlayerEnableInteractionSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _enable_input;


    protected override void Definition(){
      base.Definition();

      _enable_input = ValueInput("Enable", true);
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetPlayerEnableInteractionSequence.SequenceID,
        SequenceData = new SetPlayerEnableInteractionSequence.SequenceData{
          Enable = flow.GetValue<bool>(_enable_input)
        }
      };
    }
  }
}