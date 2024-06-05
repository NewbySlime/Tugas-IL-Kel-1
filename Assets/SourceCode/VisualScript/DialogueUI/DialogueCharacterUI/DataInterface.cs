using UnityEngine;
using Unity.VisualScripting;
using Unity.Properties;


[UnitTitle("CreateData")]
[UnitCategory("Dialogue/DialogueCharacterUI/.interface")]
public class DialogueCharacterUI_DataInterface: AddExtendedDialogueData{
  protected virtual void ModifyDialogueData(Flow flow, ref DialogueCharacterUI.ExtendedDialogue data){}

  protected override void SetExtendedData(Flow flow, DialogueUI.DialogueData data){
    if(data.DialogueCharacterUIData == null)
      data.DialogueCharacterUIData = new DialogueCharacterUI.ExtendedDialogue();
    
    ModifyDialogueData(flow, ref data.DialogueCharacterUIData);
  }
}