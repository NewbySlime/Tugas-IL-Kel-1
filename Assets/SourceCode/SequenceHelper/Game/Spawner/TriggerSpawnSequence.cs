using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system to trigger the target <see cref="ObjectSpawner"/> to spawn the objects.
  /// </summary>
  public class TriggerSpawnSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "trigger_spawn";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// The target spawner object.
      /// </summary>
      public ObjectReference.ObjRefID SpawnerRef;
    }

    
    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      GameObject _spawner_obj = ObjectReference.GetReferenceObject(_seq_data.SpawnerRef);
      if(_spawner_obj == null){
        Debug.LogError(string.Format("Referenced Object is null. (RefID: {0})", _seq_data.SpawnerRef));
        return;
      }

      ObjectSpawner _spawner = _spawner_obj.GetComponent<ObjectSpawner>();
      if(_spawner == null){
        Debug.LogError(string.Format("Referenced Object does not have ObjectSpawner. (Name: {0}, RefID: {1})", _spawner_obj.name, _seq_data.SpawnerRef));
      }

      _spawner.TriggerSpawn();
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


  [UnitTitle("Trigger Spawn")]
  [UnitCategory("Sequence/Game/Spawner")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="TriggerSpawnSequence"/>.
  /// </summary>
  public class TriggerSpawnSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _spawner_ref_input;


    protected override void Definition(){
      base.Definition();

      _spawner_ref_input = ValueInput<ObjectReference.ObjRefID>("SpawnerRef");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = TriggerSpawnSequence.SequenceID,
        SequenceData = new TriggerSpawnSequence.SequenceData{
          SpawnerRef = flow.GetValue<ObjectReference.ObjRefID>(_spawner_ref_input)
        }
      };
    }
  }
}