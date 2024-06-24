using UnityEngine;
using UnityEngine.EventSystems;


public class ButtonBaseUI: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerUpHandler{
  public delegate void OnButtonPressed();
  public event OnButtonPressed OnButtonPressedEvent;

  public delegate void OnButtonReleased();
  public event OnButtonReleased OnButtonReleasedEvent;

  public delegate void OnButtonHover();
  public event OnButtonHover OnButtonHoverEvent;


  private bool _is_hover = false;


  public void OnPointerClick(PointerEventData event_data){
    OnButtonPressedEvent?.Invoke();
  }

  public void OnPointerUp(PointerEventData event_data){
    if(!_is_hover)
      return;

    OnButtonReleasedEvent?.Invoke();
  }


  public void OnPointerEnter(PointerEventData event_data){
    _is_hover = true;
  }

  public void OnPointerExit(PointerEventData event_data){
    _is_hover = false;
  }
}