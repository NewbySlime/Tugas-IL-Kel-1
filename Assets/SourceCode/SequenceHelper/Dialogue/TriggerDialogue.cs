using System.Collections;
using System.Reflection;
using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system that will trigger Dialogue Sequences by the data supplied.
  /// </summary>
  public class TriggerDialogue: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "trigger_dialogue";
    
    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// Dialogue Sequence data to be used for Dialogue presentation.
      /// </summary>
      public DialogueUI.DialogueSequence DialogueList;
    }

    private SequenceData _seq_data;

    private DialogueCharacterUI _dialogue_ui;

    private bool _dialogue_triggering = false;


    private IEnumerator _start_trigger(){
      _dialogue_triggering = true;

      _dialogue_ui.StartDialogue(_seq_data.DialogueList);
      yield return new WaitUntil(() => _dialogue_ui.IsDialogueFinished());

      _dialogue_triggering = false;
    }


    public void Start(){
      GameUIHandler _game_ui_handler = FindAnyObjectByType<GameUIHandler>();
      if(_game_ui_handler == null){
        Debug.LogError("Cannot find GameUIHandler.");
        throw new MissingReferenceException();
      }

      _dialogue_ui = _game_ui_handler.GetDialogueHUDUI();
    }


    public void StartTriggerAsync(){
      StartCoroutine(_start_trigger());
    }

    public bool IsTriggering(){
      return _dialogue_triggering;
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


  [UnitTitle("Trigger Dialogue")]
  [UnitCategory("Sequence")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="TriggerDialogue"/>.
  /// </summary>
  public class TriggerDialogueVS: AddSubSequence{
    [DoNotSerialize]
    private ControlOutput _dialogue_output_flow;

    [DoNotSerialize]
    private ValueOutput _dialogue_output;

    private DialogueUI.DialogueSequence _dialogue_sequence;

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      _dialogue_sequence = new();
      flow.Invoke(_dialogue_output_flow);

      init_data = new SequenceHandlerVS.SequenceInitializeData.DataPart{
        SequenceID = TriggerDialogue.SequenceID,
        SequenceData = new TriggerDialogue.SequenceData{
          DialogueList = _dialogue_sequence
        }
      };
    }

    protected override void Definition(){
      base.Definition();

      _dialogue_output_flow = ControlOutput("DialogueFlow");
      _dialogue_output = ValueOutput("DialogueData", (flow) => _dialogue_sequence);
    }
  }
}