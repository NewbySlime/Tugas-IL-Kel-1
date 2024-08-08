using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(ButtonBaseUI))]
/// <summary>
/// Class that will play a soundtrack when a certain set of even is triggered.
/// 
/// This class uses following component(s);
/// - <b>AudioSource</b> used for auditory feedback of the UI interaction.
/// - <see cref="ButtonBaseUI"/> used for the needed UI events.
/// </summary>
public class ButtonSFX: MonoBehaviour{
  [SerializeField]
  private AudioClip Audio_OnHover;

  [SerializeField]
  private AudioClip Audio_OnClick;


  private AudioSource _audio_source;
  private ButtonBaseUI _button_ui;


  private void _on_hover(){
    if(Audio_OnHover == null)
      return;

    _audio_source.PlayOneShot(Audio_OnHover);
  }

  private void _on_released(){
    if(Audio_OnClick == null)
      return;
      
    _audio_source.PlayOneShot(Audio_OnClick);
  }


  public void Start(){
    _audio_source = GetComponent<AudioSource>();

    _button_ui = GetComponent<ButtonBaseUI>();
    _button_ui.OnButtonHoverEvent += _on_hover;
    _button_ui.OnButtonReleasedEvent += _on_released;
  }
}