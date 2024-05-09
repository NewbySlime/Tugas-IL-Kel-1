using System.Collections;
using UnityEngine;



namespace SequenceHelper{
  public class ChangeSceneSequence: MonoBehaviour, ISequenceAsync{
    [SerializeField]
    private string _SceneID;

    [SerializeField]
    private bool _WaitUntilSceneChangedFinished;


    private GameHandler _game_handler;

    private bool _scene_changed = false;


    private void _on_scene_changed(string scene_id, GameHandler.GameContext context){
      if(scene_id != _SceneID)
        return;

      _scene_changed = true;
    }


    public void Start(){
      _game_handler = FindAnyObjectByType<GameHandler>();
      if(_game_handler == null){
        Debug.LogError("Cannot get Game Handler.");
        throw new UnityEngine.MissingComponentException();
      }

      _game_handler.SceneChangedFinishedEvent += _on_scene_changed;
    }


    public void StartTriggerAsync(){
      _scene_changed = false;
      _game_handler.ChangeScene(_SceneID);

      if(!_WaitUntilSceneChangedFinished)
        _scene_changed = true;
    }

    public bool IsTriggering(){
      return !_scene_changed;
    }
  }


  public class SceneSequenceTrigger: MonoBehaviour, ISequenceAsync{
    [SerializeField]
    private string _SequenceID;

    private bool _is_triggering = false;


    private IEnumerator _start_sequence(){
      _is_triggering = true;
      
      while(true){
        LevelSequenceDatabase _seq_database = FindAnyObjectByType<LevelSequenceDatabase>();
        if(_seq_database == null){
          Debug.LogWarning("Cannot get database of Level Sequences.");
          break;
        }

        if(!_seq_database.HasSequence(_SequenceID)){
          Debug.LogWarning(string.Format("Cannot get Sequence with ID:'{0}'", _SequenceID));
          break;
        }

        yield return _seq_database.StartSequence(_SequenceID);
        break;
      }

      _is_triggering = false;
    }


    public void Start(){
      GameHandler _game_handler = FindAnyObjectByType<GameHandler>();
      if(_game_handler == null){
        Debug.LogError("Cannot get Game Handler.");
        throw new UnityEngine.MissingComponentException();
      }
    }


    public void StartTriggerAsync(){
      StartCoroutine(_start_sequence());
    }


    public bool IsTriggering(){
      return _is_triggering;
    }
  }
}