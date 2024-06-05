using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  public class EnemyFightAddWatchObject: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "enemy_fight_add_watch_enemy";

    public struct SequenceData{
      public ObjectReference.ObjRefID MiniGameObjRef;
      public ObjectReference.ObjRefID TargetObjRef;
    }


    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      GameObject _mini_game_obj = ObjectReference.GetReferenceObject(_seq_data.MiniGameObjRef);
      if(_mini_game_obj == null){
        Debug.LogError(string.Format("MiniGame Object is null. (RefID: {0})", _seq_data.MiniGameObjRef));
        return;
      }

      EnemyFightMG _mini_game = _mini_game_obj.GetComponent<EnemyFightMG>();
      if(_mini_game == null){
        Debug.LogError(string.Format("MiniGame Object does not have EnemyFightMG. (RefID: {0})", _seq_data.MiniGameObjRef));
        return;
      }


      GameObject _target_obj = ObjectReference.GetReferenceObject(_seq_data.TargetObjRef);
      if(_target_obj == null){
        Debug.LogError(string.Format("Enemy Object is null. (RefID: {0})", _seq_data.TargetObjRef));
        return;
      }


      _mini_game.AddWatchObject(_target_obj);
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


  [UnitTitle("Sequence/Game/EnemyFight")]
  public class EnemyFightAddWatchObjectVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _mini_game_input;

    [DoNotSerialize]
    private ValueInput _target_obj_input;


    protected override void Definition(){
      base.Definition();

      _mini_game_input = ValueInput<ObjectReference.ObjRefID>("EnemyFightMGRef");
      _target_obj_input = ValueInput<ObjectReference.ObjRefID>("TargetObjRef");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = EnemyFightAddWatchObject.SequenceID,
        SequenceData = new EnemyFightAddWatchObject.SequenceData{
          MiniGameObjRef = flow.GetValue<ObjectReference.ObjRefID>(_mini_game_input),
          TargetObjRef = flow.GetValue<ObjectReference.ObjRefID>(_target_obj_input)
        }
      };
    }
  }
}