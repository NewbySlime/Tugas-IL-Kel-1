using System.Collections;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Class for handling shader for "parallax" effect by being intermediate between current game state (camera position and such) and the shader attached.
/// The shader mentioned is in Assets/Shaders/ParallaxLit.shadergraph 
/// 
/// This class uses external component(s);
/// - Object with <see cref="IMaterialReference"/> that a reference to the shader in an attached graphic object.
/// </summary>
public class ParallaxComponent: MonoBehaviour{
  [SerializeField]
  private GameObject _TargetMaterial;

  [SerializeField]
  private float _PositionMultiplier = 1;
  [SerializeField]
  private float _ImageSize = 1;

  [SerializeField]
  private Vector2 _InitialOffset;


  private IMaterialReference _material_ref;

  private IEnumerator _start_co_func(){
    yield return null;
    yield return new WaitForEndOfFrame();

    Material _current_material = _material_ref.GetMaterial();
    _current_material.SetFloat("_PositionMultiplier", _PositionMultiplier);
    _current_material.SetFloat("_ImageSizeMultiplier", _ImageSize);

    _current_material.SetVector("_Offset", _InitialOffset);
  }


  public void Start(){
    IMaterialReference[] _material_references = _TargetMaterial.GetComponents<IMaterialReference>();
    if(_material_references.Length <= 0){
      Debug.LogError("TargetMaterial does not have IMaterialReference Object.");
      throw new MissingReferenceException();
    }
    else if(_material_references.Length > 1)
      Debug.LogWarning("TargetMaterial has more than one instance of IMaterialReference.");

    _material_ref = _material_references[0];
    StartCoroutine(_start_co_func());
  }

  
  /// <summary>
  /// Set the reference position for the parallax effect.
  /// </summary>
  /// <param name="pos">Reference position</param>
  public void SetPosition(Vector2 pos){
    _material_ref.GetMaterial().SetVector("_Position", pos);
  }
}