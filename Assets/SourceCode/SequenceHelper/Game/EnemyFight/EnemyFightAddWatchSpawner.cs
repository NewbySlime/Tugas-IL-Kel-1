using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  public class EnemyFightAddWatchSpawner: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "enemy_fight_add_watch_spawner";

    public struct SequenceData{
      public ObjectReference.ObjRefID MiniGameRef;
      public ObjectReference.ObjRefID TargetSpawnerRef;
    }

    
    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      GameObject _mini_game_obj = ObjectReference.GetReferenceObject(_seq_data.MiniGameRef);
      if(_mini_game_obj == null){
        Debug.LogError(string.Format("MiniGame Object is null. (RefID: {0})", _seq_data.MiniGameRef));
        return;
      }

      EnemyFightMG _mini_game = _mini_game_obj.GetComponent<EnemyFightMG>();
      if(_mini_game == null){
        Debug.LogError(string.Format("MiniGame Object does not have EnemyFightMG. (RefID: {0})", _seq_data.TargetSpawnerRef));
        return;
      }


      GameObject _target_spawner_obj = ObjectReference.GetReferenceObject(_seq_data.TargetSpawnerRef);
      if(_target_spawner_obj == null){
        Debug.LogError(string.Format("Target Spawner Object is null. (RefID: {0})", _seq_data.TargetSpawnerRef));
        return;
      }

      ObjectSpawner _target_spawner = _target_spawner_obj.GetComponent<ObjectSpawner>();
      if(_target_spawner == null){
        Debug.LogError(string.Format("Target Spawner does not have ObjectSpawner. (RefID: {0})", _seq_data.TargetSpawnerRef));
        return;
      }


      _mini_game.AddWatchObjectFromSpawner(_target_spawner);
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


  [UnitTitle("Add Watch Spawner")]
  [UnitCategory("Sequence/Game/EnemyFight")]
  public class EnemyFightAddWatchSpawnerVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _mini_game_input;

    [DoNotSerialize]
    private ValueInput _target_spawner_input;


    protected override void Definition(){
      base.Definition();

      _mini_game_input = ValueInput<ObjectReference.ObjRefID>("MiniGameRef");
      _target_spawner_input = ValueInput<ObjectReference.ObjRefID>("TargetSpawnerRef");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = EnemyFightAddWatchSpawner.SequenceID,
        SequenceData = new EnemyFightAddWatchSpawner.SequenceData{
          MiniGameRef = flow.GetValue<ObjectReference.ObjRefID>(_mini_game_input),
          TargetSpawnerRef = flow.GetValue<ObjectReference.ObjRefID>(_target_spawner_input)
        }
      };
    }
  }
}