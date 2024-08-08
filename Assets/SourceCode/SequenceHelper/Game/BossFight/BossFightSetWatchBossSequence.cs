using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system for binding the enemy (boss) object to the target <see cref="BossFightMG"/> object.
  /// </summary>
  public class BossFightSetWatchBossSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "boss_fight_set_watch_boss";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// The target object that has <see cref="BossFightMG"/> component.
      /// </summary>
      public ObjectReference.ObjRefID BossFightMGRef;

      /// <summary>
      /// The target enemy (boss) object.
      /// </summary>
      public ObjectReference.ObjRefID TargetBossRef;
    }


    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      GameObject _boss_fight_ref = ObjectReference.GetReferenceObject(_seq_data.BossFightMGRef);
      if(_boss_fight_ref == null){
        Debug.LogError(string.Format("Reference Boss Fight MiniGame Object is null. (RefID: {0})", _seq_data.BossFightMGRef));
        return;
      }

      BossFightMG _boss_fight = _boss_fight_ref.GetComponent<BossFightMG>();
      if(_boss_fight == null){
        Debug.LogError(string.Format("Reference Boss Fight MiniGame Object does not BossFightMG. (Name: {0}, RefID: {1})", _boss_fight_ref.name, _seq_data.BossFightMGRef));
        return;
      }


      GameObject _target_boss_ref = ObjectReference.GetReferenceObject(_seq_data.TargetBossRef);
      if(_target_boss_ref == null){
        Debug.LogError(string.Format("Reference Target Boss Object is null. (RefID: {0})", _seq_data.TargetBossRef));
        return;
      }


      _boss_fight.SetWatchBoss(_target_boss_ref);
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


  [UnitTitle("Set Watch Boss")]
  [UnitCategory("Sequence/Game/BossFight")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="BossFightSetWatchBossSequence"/>.
  /// </summary>
  public class BossFightSetWatchBossSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _mini_game_ref;
    [DoNotSerialize]
    private ValueInput _target_boss_ref;


    protected override void Definition(){
      base.Definition();

      _mini_game_ref = ValueInput<ObjectReference.ObjRefID>("MiniGameRef");
      _target_boss_ref = ValueInput<ObjectReference.ObjRefID>("TargetBossRef");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = BossFightSetWatchBossSequence.SequenceID,
        SequenceData = new BossFightSetWatchBossSequence.SequenceData{
          BossFightMGRef = flow.GetValue<ObjectReference.ObjRefID>(_mini_game_ref),
          TargetBossRef = flow.GetValue<ObjectReference.ObjRefID>(_target_boss_ref)
        }
      };
    }
  }
}