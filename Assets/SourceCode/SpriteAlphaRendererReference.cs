using UnityEngine;


/// <summary>
/// Class implementing interface <see cref="IAlphaRendererReference"/> for modifying an alpha value of a target <b>SpriteRenderer</b> component.
/// 
/// This class uses external component(s);
/// - Unity's <b>SpriteRenderer</b> component as the target alpha manipulation.
/// </summary>
public class SpriteAlphaRendererReference: MonoBehaviour, IAlphaRendererReference{
  [SerializeField]
  private SpriteRenderer _TargetSpriteRenderer;


  public void SetAlpha(float value){
    Color _current_col = _TargetSpriteRenderer.material.color;
    _current_col.a = value;

    _TargetSpriteRenderer.material.color = _current_col;
  }

  public float GetAlpha(){
    return _TargetSpriteRenderer.material.color.a;
  }
}