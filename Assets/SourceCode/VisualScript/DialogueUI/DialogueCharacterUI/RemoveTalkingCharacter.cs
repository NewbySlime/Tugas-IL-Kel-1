using UnityEngine;
using Unity.VisualScripting;


[UnitTitle("Remove Character")]
[UnitCategory("Dialogue/DialogueCharacterUI")]
public class RemoveTalkingCharacter: DialogueCharacterUI_DataInterface{
  [DoNotSerialize]
  private ValueInput _character_id_input;
  
  [DoNotSerialize]
  private ValueInput _skip_remove_anim_input;


  protected override void ModifyDialogueData(Flow flow, ref DialogueCharacterUI.ExtendedDialogue data){
    data.RemovedCharacters.Add(new DialogueCharacterUI.ExtendedDialogue.CharacterRemoveData{
      CharacterID = flow.GetValue<string>(_character_id_input),
      SkipAnimation = flow.GetValue<bool>(_skip_remove_anim_input)
    });
  }

  protected override void Definition(){
    base.Definition();

    _character_id_input = ValueInput("Character", "");
    _skip_remove_anim_input = ValueInput("SkipAnimation", false);
  }
}