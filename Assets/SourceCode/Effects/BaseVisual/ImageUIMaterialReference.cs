using UnityEngine;
using UnityEngine.UI;


public class ImageUIMaterialReference: MonoBehaviour, IMaterialReference{
  [SerializeField]
  private MaskableGraphic _ImageUI;

  [SerializeField]
  private Material _BaseMaterial;


  private Material _current_material;


  public void Start(){
    _current_material = new Material(_BaseMaterial);
    _ImageUI.material = _current_material;  
  }


  public Material GetMaterial(){
    return _current_material;
  }
}