using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  [UnitTitle("Get Dialogue Input Focus")]
  [UnitCategory("Sequence/Dialogue")]
  /// <summary>
  /// A sequence helper node to get input context for UI <see cref="DialogueCharacterUI"/>.
  /// </summary>
  public class GetDialogueInputFocusVS: Unit{
    [DoNotSerialize]
    private ValueOutput _focus_output;


    protected override void Definition(){
      _focus_output = ValueOutput("FocusContext", (flow) => new RegisterInputFocusSequence.InputFocusData{
        RefID = DialogueCharacterUI.ObjectRef,
        InputContext = DialogueCharacterUI.InputContext
      });
    }
  }
}