using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PromptUI: MonoBehaviour{
  public delegate void OnPrompted();
  public event OnPrompted OnPromptAcceptEvent;
  public event OnPrompted OnPromptCancelEvent;

  [SerializeField]
  private ButtonBaseUI _AcceptButton;
  [SerializeField]
  private ButtonBaseUI _CancelButton;

  [SerializeField]
  private TMP_Text _TextPrompt;

  
  private void _on_accept_button(){
    OnPromptAcceptEvent?.Invoke();
  }

  private void _on_cancel_button(){
    OnPromptCancelEvent?.Invoke();
  }


  public void Start(){
    _AcceptButton.OnButtonReleasedEvent += _on_accept_button;
    _CancelButton.OnButtonReleasedEvent += _on_cancel_button;

    _TextPrompt.text = "";
  }

  public void SetPromptText(string text){
    _TextPrompt.text = text;
  }
}