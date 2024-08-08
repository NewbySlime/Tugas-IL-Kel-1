using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// UI class for giving prompt message to player.
/// 
/// This class uses external component(s);
/// - <see cref="ButtonBaseUI"/> for handling interaction of the options available.
/// - <b>Unity's TMP Text UI</b> for presenting the prompt message.
/// </summary>
public class PromptUI: MonoBehaviour{
  public delegate void OnPrompted();

  /// <summary>
  /// Event for when the player accepted with the prompt.
  /// </summary>
  public event OnPrompted OnPromptAcceptEvent;

  // Event for when the player cancelled or disagreeing with the prompt.
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


  /// <summary>
  /// Set the prompt message for the player.
  /// </summary>
  /// <param name="text">The prompt message</param>
  public void SetPromptText(string text){
    _TextPrompt.text = text;
  }
}