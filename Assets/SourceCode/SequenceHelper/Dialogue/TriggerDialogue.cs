using System.Collections;
using System.Reflection;
using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  public class TriggerDialogue: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "trigger_dialogue";
    
    public struct SequenceData{
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