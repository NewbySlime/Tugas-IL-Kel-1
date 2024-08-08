using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// UI component for handling and catching Unity's UI event and re-emits as C# event. 
/// </summary>
public class ButtonBaseUI: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerUpHandler{
  /// <summary>
  /// Event for when the button UI is pressed.
  /// </summary>
  public event OnButtonPressed OnButtonPressedEvent;
  public delegate void OnButtonPressed();

  /// <summary>
  /// Event for when the button UI is released. But it will not emit when the mouse is not hovering on the button.
  /// </summary>
  public event OnButtonReleased OnButtonReleasedEvent;
  public delegate void OnButtonReleased();

  /// <summary>
  /// Event for when the button entering UI's area.
  /// </summary>
  public event OnButtonHover OnButtonHoverEvent;
  public delegate void OnButtonHover();

  /// <summary>
  /// Event fo rwhen the button exiting UI's area.
  /// </summary>
  public event OnButtonExit OnButtonExitEvent;
  public delegate void OnButtonExit();


  private bool _is_hover = false;


  /// <summary>
  /// Function to catch mouse pressed event within the UI's area.
  /// </summary>
  /// <param name="event_data"></param>
  public void OnPointerClick(PointerEventData event_data){
    OnButtonPressedEvent?.Invoke();
  }

  /// <summary>
  /// Function to catch mouse released event (only triggerred when the mouse is still in the UI's area).
  /// </summary>
  /// <param name="event_data"></param>
  public void OnPointerUp(PointerEventData event_data){
    if(!_is_hover)
      return;

    OnButtonReleasedEvent?.Invoke();
  }


  /// <summary>
  /// Function to catch mouse entering to the UI's area event.
  /// </summary>
  /// <param name="event_data"></param>
  public void OnPointerEnter(PointerEventData event_data){
    _is_hover = true;
  
    OnButtonHoverEvent?.Invoke();
  }

  /// <summary>
  /// Function to catch mouse exiting from the UI's area event.
  /// </summary>
  /// <param name="event_data"></param>
  public void OnPointerExit(PointerEventData event_data){
    _is_hover = false;
    
    OnButtonExitEvent?.Invoke();
  }
}