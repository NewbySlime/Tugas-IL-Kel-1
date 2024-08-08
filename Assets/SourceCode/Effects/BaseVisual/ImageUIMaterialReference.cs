using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// A class that handle material/shader usage and used as a reference for the Material. See also: <see cref="IMaterialReference"/>>.
/// 
/// A brief explanation, Unity's Graphical UI classes will only use the instance of the same Material (file) that exists statically. Therefore, this class intend to fix that by creating a new instance under the same Material file.
/// </summary>
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