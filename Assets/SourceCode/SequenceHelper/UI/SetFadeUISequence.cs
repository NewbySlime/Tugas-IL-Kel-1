using UnityEngine;
using Unity.VisualScripting;
using System.Reflection;
using System.Collections;


namespace SequenceHelper{
  /// <summary>
  /// a custom Sequencing system to for hiding/showing the Fade UI (to block the screen).
  /// </summary>
  public class SetFadeUISequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "set_fade_ui";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// Do hide/show the Fade UI.
      /// </summary>
      public bool FadeToShow;

      /// <summary>
      /// Flag to skip the fade animation.
      /// </summary>
      public bool SkipAnimation;

      /// <summary>
      /// Flag for telling the Sequence system for wait until the fade animation finished.
      /// </summary>
      public bool WaitUntilFinish;
    }


    private SequenceData _seq_data;

    private FadeUI _fade_ui;


    public void Start(){
      GameUIHandler _ui_handler = FindAnyObjectByType<GameUIHandler>();
      if(_ui_handler == null){
        Debug.LogError("Cannot find GameUIHandler.");
        throw new MissingReferenceException();
      }

      _fade_ui = _ui_handler.GetGeneralFadeUI();
    }


    public void StartTriggerAsync(){
      StartCoroutine(UIUtility.SetHideUI(_fade_ui.gameObject, !_seq_data.FadeToShow, _seq_data.SkipAnimation));
    }

    public bool IsTriggering(){
      return _seq_data.WaitUntilFinish && !TimingBaseUI.AllTimerFinished(_fade_ui);
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


  [UnitTitle("Set Fade UI")]
  [UnitCategory("Sequence/UI")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node from sequence <see cref="SetFadeUISequence"/>.
  /// </summary>
  public class SetFadeUISequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _fade_cover_input;
    [DoNotSerialize]
    private ValueInput _skip_animation_input;
    [DoNotSerialize]
    private ValueInput _wait_until_finished_input;


    protected override void Definition(){
      base.Definition();

      _fade_cover_input = ValueInput("FadeCover", true);
      _skip_animation_input = ValueInput("SkipAnimation", false);
      _wait_until_finished_input = ValueInput("WaitUntilFinished", true);
    }


    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetFadeUISequence.SequenceID,
        SequenceData = new SetFadeUISequence.SequenceData{
          FadeToShow = flow.GetValue<bool>(_fade_cover_input),
          SkipAnimation = flow.GetValue<bool>(_skip_animation_input),
          WaitUntilFinish = flow.GetValue<bool>(_wait_until_finished_input)
        }
      };
    }
  }
}