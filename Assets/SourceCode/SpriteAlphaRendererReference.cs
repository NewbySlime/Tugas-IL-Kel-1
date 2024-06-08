using UnityEngine;


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