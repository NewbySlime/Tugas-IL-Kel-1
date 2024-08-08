using System.Collections;
using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system for trigger scene/level change.
  /// </summary>
  public class ChangeSceneSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for reigstering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "change_scene";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// The target Scnee/Level.
      /// </summary>
      public string SceneID;
      /// <summary>
      /// Wait until scene is changed.
      /// WARN: only use this from <see cref="SequenceHandlerVS"/> that has "DontDestroy" attribute for the object.
      /// </summary>
      public bool WaitUntilSceneChanged;

      /// <summary>
      /// Flag to do save when the Game is changing scene.
      /// </summary>
      public bool AutoSave; 
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
      _game_handler.ChangeScene(_seq_data.SceneID, "", _seq_data.AutoSave);

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
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="ChangeSceneSequence"/>.
  /// </summary>
  public class ChangeSceneSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _scene_id_input;

    [DoNotSerialize]
    private ValueInput _wait_until_input;

    [DoNotSerialize]
    private ValueInput _auto_save_input;


    protected override void Definition(){
      base.Definition();

      _scene_id_input = ValueInput("SceneID", "");
      _wait_until_input = ValueInput("WaitUntilChanged", true);
      _auto_save_input = ValueInput("AutoSave", true);
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new SequenceHandlerVS.SequenceInitializeData.DataPart{
        SequenceID = ChangeSceneSequence.SequenceID,
        SequenceData = new ChangeSceneSequence.SequenceData{
          SceneID = flow.GetValue<string>(_scene_id_input),
          WaitUntilSceneChanged = flow.GetValue<bool>(_wait_until_input),
          AutoSave = flow.GetValue<bool>(_auto_save_input)
        }
      };
    }
  }
}