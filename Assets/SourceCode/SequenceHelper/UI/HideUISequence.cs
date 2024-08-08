using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system to hide an object that has the compatibility with using <see cref="UIUtility"/>.
  /// </summary>
  public class HideUISequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "hide_ui";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// The target object to be hidden.
      /// </summary>
      public ObjectReference.ObjRefID TargetObjRef;
      
      
      /// <summary>
      /// If the UI needs to be hidden or shown.
      /// </summary>
      public bool HideFlag;

      /// <summary>
      /// Flag to skip the hide/show animation.
      /// </summary>
      public bool SkipAnimation;


      /// <summary>
      /// Flag to tell if the sequence need to wait until the animation is finished.
      /// </summary>
      public bool WaitUntilFinished;
    }


    private SequenceData _seq_data;

    private bool _is_triggering = false;


    private IEnumerator _trigger_sequence(){
      _is_triggering = true;

      GameObject _ref_obj = ObjectReference.GetReferenceObject(_seq_data.TargetObjRef);
      if(_ref_obj == null){
        Debug.LogError(string.Format("Referenced Object is null. (RefID: {0})", _seq_data.TargetObjRef));
        yield break;
      }

      if(_seq_data.WaitUntilFinished)
        yield return UIUtility.SetHideUI(_ref_obj, _seq_data.HideFlag, _seq_data.SkipAnimation);
      else
        StartCoroutine(UIUtility.SetHideUI(_ref_obj, _seq_data.HideFlag, _seq_data.SkipAnimation));

      _is_triggering = false;
    }


    public void StartTriggerAsync(){
      if(IsTriggering())
        return;

      StartCoroutine(_trigger_sequence());
    }

    public bool IsTriggering(){
      return _is_triggering;
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


  [UnitTitle("Hide UI")]
  [UnitCategory("Sequence/UI")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="HideUISequence"/>.
  /// </summary>
  public class HideUISequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _target_obj_input;

    [DoNotSerialize]
    private ValueInput _hide_flag_input;
    [DoNotSerialize]
    private ValueInput _skip_animation_input;

    [DoNotSerialize]
    private ValueInput _wait_until_finished_input;


    protected override void Definition(){
      base.Definition();

      _target_obj_input = ValueInput<ObjectReference.ObjRefID>("TargetObjRef");
      _hide_flag_input = ValueInput("Hide", false);
      _skip_animation_input = ValueInput("SkipAnimation", false);
      _wait_until_finished_input = ValueInput("WaitUntilFinished", true);
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = HideUISequence.SequenceID,
        SequenceData = new HideUISequence.SequenceData{
          TargetObjRef = flow.GetValue<ObjectReference.ObjRefID>(_target_obj_input),
          HideFlag = flow.GetValue<bool>(_hide_flag_input),
          SkipAnimation = flow.GetValue<bool>(_skip_animation_input),
          WaitUntilFinished = flow.GetValue<bool>(_wait_until_finished_input)
        }
      };
    }
  }
}