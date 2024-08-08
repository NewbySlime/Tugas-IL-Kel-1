using UnityEngine;


/// <summary>
/// Class implementing interface <see cref="IAlphaRendererReference"/> for modifying an alpha value of a target <b>CanvasGroup</b> component for UI elements.
/// 
/// This class uses external component(s);
/// - Unity's <b>CanvasGroup</b> component as the target alpha manipulation.
/// </summary>
public class UIAlphaRendererReference: MonoBehaviour, IAlphaRendererReference{
  [SerializeField]
  private CanvasGroup _TargetCanvas;


  public void SetAlpha(float value){
    _TargetCanvas.alpha = value;
  }

  public float GetAlpha(){
    return _TargetCanvas.alpha;
  }
}