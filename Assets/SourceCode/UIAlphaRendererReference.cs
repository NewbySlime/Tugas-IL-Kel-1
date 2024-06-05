using UnityEngine;


public class UIAlphaRendererReference: MonoBehaviour, IAlphaRendererReference{
  [SerializeField]
  private CanvasGroup _TargetCanvas;


  public void SetAlpha(float value){
    _TargetCanvas.alpha = value;
  }
}