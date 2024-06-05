using UnityEngine;
using UnityEngine.EventSystems;


public class ButtonBaseUI: MonoBehaviour, IPointerClickHandler, IPointerUpHandler{
  public delegate void OnButtonPressed();
  public event OnButtonPressed OnButtonPressedEvent;

  public delegate void OnButtonReleased();
  public event OnButtonReleased OnButtonReleasedEvent;


  public void OnPointerClick(PointerEventData event_data){
    OnButtonPressedEvent?.Invoke();
  }

  public void OnPointerUp(PointerEventData event_data){
    OnButtonReleasedEvent?.Invoke();
  }
}