using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system to set the <see cref="PlayerController"/>'s configuration.
  /// NOTE: since <see cref="PlayerController"/> object must be one in runtime, no need to supply <see cref="ObjectReference.ObjRefID"/>.
  /// </summary>
  public class SetPlayerConfigFlagSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "set_player_walk_flag";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// Flag if the player can use a weapon.
      /// </summary>
      public bool AllowUseWeapon;

      /// <summary>
      /// Flag if the player can jump.
      /// </summary>
      public bool AllowJump;
      
      /// <summary>
      /// Flag if the player can do interaction to another object.
      /// </summary>
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
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="SetPlayerConfigFlagSequence"/>.
  /// </summary>
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