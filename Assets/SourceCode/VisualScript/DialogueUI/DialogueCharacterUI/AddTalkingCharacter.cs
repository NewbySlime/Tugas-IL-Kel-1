using UnityEngine;
using Unity.VisualScripting;


[UnitTitle("Add Character")]
[UnitCategory("Dialogue/DialogueCharacterUI")]
/// <summary>
/// Extended <see cref="DialogueCharacterUI_DataInterface"/> Node for giving instruction to adding character for this part of dialogue data to <see cref="DialogueCharacterUI"/>.
/// </summary>
public class AddTalkingCharacter: DialogueCharacterUI_DataInterface{
  [DoNotSerialize]
  private ValueInput _character_id_input;
  [DoNotSerialize]
  private ValueInput _character_pos_input;

  protected override void ModifyDialogueData(Flow flow, ref DialogueCharacterUI.ExtendedDialogue data){
    data.AddedCharacters.Add(new DialogueCharacterUI.ExtendedDialogue.CharacterInitData{
      CharacterID = flow.GetValue<string>(_character_id_input),
      UIPosition = flow.GetValue<DialogueCharacterUI.ShowLayoutPosition>(_character_pos_input)
    });
  }

  protected override void Definition(){
    base.Definition();

    _character_id_input = ValueInput("Character", "");
    _character_pos_input = ValueInput("LayoutPosition", DialogueCharacterUI.ShowLayoutPosition.Main);
  }
}