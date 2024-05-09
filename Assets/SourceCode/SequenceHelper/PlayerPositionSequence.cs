using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  namespace PlayerHandling{
    public class SetPlayerPositionSequence: MonoBehaviour, ISequenceAsync{
      [SerializeField]
      private Vector3 _SetPosition;

      
      public void StartTriggerAsync(){
        PlayerController _player = FindAnyObjectByType<PlayerController>();
        if(_player == null){
          Debug.LogWarning("Player doesn't exist in current scene");
          return;
        }

        _player.transform.position = _SetPosition;
      }

      public bool IsTriggering(){
        return false;
      }
    }


    public class TeleportPlayerCheckpointSequence: MonoBehaviour, ISequenceAsync{
      [SerializeField]
      private string _CheckpointID;

      private LevelCheckpointDatabase _checkpoint_database;


      public void Start(){
        _checkpoint_database = FindAnyObjectByType<LevelCheckpointDatabase>();
        if(_checkpoint_database == null){
          Debug.LogError("Cannot get Checkpoint Database.");
          throw new UnityEngine.MissingComponentException();
        }
      }


      public void StartTriggerAsync(){
        CheckpointHandler _checkpoint = _checkpoint_database.GetCheckpoint(_CheckpointID);
        if(_checkpoint == null){
          Debug.LogWarning(string.Format("Checkpoint (ID: '{0}') cannot be found.", _CheckpointID));
          return;
        }

        PlayerController _player = FindAnyObjectByType<PlayerController>();
        if(_player == null){
          Debug.LogWarning("Player doesn't exist in current scene");
          return;
        }

        _checkpoint.TeleportObject(_player.gameObject);
      }

      public bool IsTriggering(){
        return false;
      }
    }
  }
}