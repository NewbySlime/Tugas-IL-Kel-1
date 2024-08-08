using UnityEngine;
using Unity.VisualScripting;
using Unity.Properties;


[UnitTitle("CreateData")]
[UnitCategory("Dialogue/DialogueCharacterUI/.interface")]
/// <summary>
/// Interface Node for adding/modifying data of certain variable in <see cref="DialogueCharacterUI.ExtendedDialogue"/>.
/// This class will automatically create the extended dialogue data when it doesn't exist in current part of dialogue data.  
/// </summary>
public class DialogueCharacterUI_DataInterface: AddExtendedDialogueData{
  protected virtual void ModifyDialogueData(Flow flow, ref DialogueCharacterUI.ExtendedDialogue data){}

  protected override void SetExtendedData(Flow flow, DialogueUI.DialogueData data){
    if(data.DialogueCharacterUIData == null)
      data.DialogueCharacterUIData = new DialogueCharacterUI.ExtendedDialogue();
    
    ModifyDialogueData(flow, ref data.DialogueCharacterUIData);
  }
}