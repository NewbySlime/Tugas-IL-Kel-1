using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class ParallaxComponent: MonoBehaviour{
  [SerializeField]
  private Material _Material;

  [SerializeField]
  private float _PositionMultiplier = 1;
  [SerializeField]
  private float _ImageSize = 1;

  [SerializeField]
  private Vector2 _InitialOffset;


  private Image _image_renderer;

  private Material _current_material;


  public void Start(){
    _image_renderer = GetComponent<Image>();
    _image_renderer.enabled = true;

    _current_material = new Material(_Material);
    _image_renderer.material = _current_material;

    _current_material.SetFloat("_PositionMultiplier", _PositionMultiplier);
    _current_material.SetFloat("_ImageSizeMultiplier", _ImageSize);

    _current_material.SetVector("_Offset", _InitialOffset);
  }

  
  public void SetPosition(Vector2 pos){
    _current_material.SetVector("_Position", pos);
  }
}