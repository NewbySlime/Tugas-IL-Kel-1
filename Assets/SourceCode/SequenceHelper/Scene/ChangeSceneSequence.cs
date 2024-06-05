using System.Collections;
using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  public class ChangeSceneSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "change_scene";

    public struct SequenceData{
      public string SceneID;
      public bool WaitUntilSceneChanged;
    }


    private GameHandler _game_handler;
    private SequenceData _seq_data;

    private bool _scene_changed = false;


    private void _on_scene_changed(string scene_id, GameHandler.GameContext context){
      if(scene_id != _seq_data.SceneID)
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

      if(_game_handler.SceneInitialized)
        _on_scene_changed(_game_handler.GetCurrentSceneID(), _game_handler.GetCurrentSceneContext());
    }


    public void StartTriggerAsync(){
      _scene_changed = false;
      _game_handler.ChangeScene(_seq_data.SceneID);

      if(!_seq_data.WaitUntilSceneChanged)
        _scene_changed = true;
    }

    public bool IsTriggering(){
      return !_scene_changed;
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


  [UnitTitle("Change Scene")]
  [UnitCategory("Sequence")]
  public class ChangeSceneSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _scene_id_input;

    [DoNotSerialize]
    private ValueInput _wait_until_input;


    protected override void Definition(){
      base.Definition();

      _scene_id_input = ValueInput("SceneID", "");
      _wait_until_input = ValueInput("WaitUntilChanged", true);
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new SequenceHandlerVS.SequenceInitializeData.DataPart{
        SequenceID = ChangeSceneSequence.SequenceID,
        SequenceData = new ChangeSceneSequence.SequenceData{
          SceneID = flow.GetValue<string>(_scene_id_input),
          WaitUntilSceneChanged = flow.GetValue<bool>(_wait_until_input)
        }
      };
    }
  }
}