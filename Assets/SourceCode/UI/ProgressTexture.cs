using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
/// <summary>
/// UI Class for handling progress bar using texture as the bar's sprite.
/// This class uses shader to handle the progress bar.
/// 
/// This class uses external component(s);
/// - <b>Unity's MaskableGraphic</b> as the texture visualization.
/// </summary>
public class ProgressTexture: MonoBehaviour{
  [SerializeField]
  private Image _TargetSpriteManipulation;
  [SerializeField]
  private Color _ColorMultiplyBackground;
  [SerializeField]
  private Material _SpriteMaterial;

  [SerializeField]
  // the initial sprite to be used.
  private Texture _SpriteTexture;

  private Material _target_meterial;


  public void Start(){
    DEBUGModeUtils.Log("progress starting.");
    _target_meterial = new Material(_SpriteMaterial);
    _TargetSpriteManipulation.material = _target_meterial;

    _target_meterial.SetFloat("_ProgressionUseMainTex", 1);
    SetColorMultiplyBG(_ColorMultiplyBackground);

    if(_SpriteTexture != null)
      SetTexture(_SpriteTexture);
  }


  /// <summary>
  /// To multiply the texture's color with the supplied color.
  /// </summary>
  /// <param name="col">The color value</param>
  public void SetColorMultiplyBG(Color col){
    _target_meterial.SetColor("_ProgressionColorMultiply", col);
  }

  /// <summary>
  /// To set bar's texture.
  /// </summary>
  /// <param name="texture">The texture to be used</param>
  public void SetTexture(Texture texture){
    _target_meterial.SetTexture("_MainTex", texture);
  }

  /// <summary>
  /// To set the progress value of this UI.
  /// </summary>
  /// <param name="progress"></param>
  public void SetProgress(float progress){
    _target_meterial.SetFloat("_Progression", progress);
  }
}