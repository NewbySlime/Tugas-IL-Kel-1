using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class ProgressTexture: MonoBehaviour{
  [SerializeField]
  private Image _TargetSpriteManipulation;
  [SerializeField]
  private Color _ColorMultiplyBackground;
  [SerializeField]
  private Material _SpriteMaterial;

  [SerializeField]
  private Texture _SpriteTexture;

  private Material _target_meterial;


  public void Start(){
    Debug.Log("progress starting.");
    _target_meterial = new Material(_SpriteMaterial);
    _TargetSpriteManipulation.material = _target_meterial;

    _target_meterial.SetFloat("_ProgressionUseMainTex", 1);
    SetColorMultiplyBG(_ColorMultiplyBackground);

    if(_SpriteTexture != null)
      SetTexture(_SpriteTexture);
  }


  public void SetColorMultiplyBG(Color col){
    _target_meterial.SetColor("_ProgressionColorMultiply", col);
  }

  public void SetTexture(Texture texture){
    _target_meterial.SetTexture("_MainTex", texture);
  }


  public void SetProgress(float progress){
    _target_meterial.SetFloat("_Progression", progress);
  }
}