using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  public class SetPlayerConfigFlagSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "set_player_walk_flag";

    public struct SequenceData{
      public bool AllowUseWeapon;
      public bool AllowJump;
      public bool AllowInteraction;
    }

    
    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      PlayerController _player = FindAnyObjectByType<PlayerController>();
      if(_player == null){
        Debug.LogError("Cannot find PlayerComponent.");
        return;
      }

      _player.AllowUseWeapon = _seq_data.AllowUseWeapon;
      _player.AllowJump = _seq_data.AllowJump;
      _player.SetEnableInteraction(_seq_data.AllowInteraction);
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


  [UnitTitle("Set Player Config")]
  [UnitCategory("Sequence/Player")]
  public class SetPlayerConfigFlagSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _allow_weapon_input;
    [DoNotSerialize]
    private ValueInput _allow_jump;
    [DoNotSerialize]
    private ValueInput _allow_interaction;


    protected override void Definition(){
      base.Definition();

      _allow_weapon_input = ValueInput("AllowWeaponUsage", true);
      _allow_jump = ValueInput("AllowJump", true);
      _allow_interaction = ValueInput("AllowInteraction", true);
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetPlayerConfigFlagSequence.SequenceID,
        SequenceData = new SetPlayerConfigFlagSequence.SequenceData{
          AllowUseWeapon = flow.GetValue<bool>(_allow_weapon_input),
          AllowJump = flow.GetValue<bool>(_allow_jump),
          AllowInteraction = flow.GetValue<bool>(_allow_interaction)
        }
      };
    }
  }
}