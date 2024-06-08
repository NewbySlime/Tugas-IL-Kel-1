using System.Collections;
using UnityEngine;


public class BaseProgressUI: MonoBehaviour{
  [SerializeField]
  private GameObject _MaterialReference;

  [SerializeField]
  private Color _BGProgressMult;

  private IMaterialReference _mat_reference;


  private IEnumerator _start_co_func(){
    yield return null;
    yield return new WaitForEndOfFrame();

    _mat_reference.GetMaterial().SetColor("_ProgressionColorMultiply", _BGProgressMult);
  }


  public void Start(){
    IMaterialReference[] _reference_list = _MaterialReference.GetComponents<IMaterialReference>();
    if(_reference_list.Length <= 0){
      Debug.LogError("MaterialReference does not have IMaterialReference.");
      throw new MissingReferenceException();
    }
    else if(_reference_list.Length > 1)
      Debug.LogWarning("MaterialReference have more than one IMaterialReference, is it is not yet supported.");

    _mat_reference = _reference_list[0];

    StartCoroutine(_start_co_func());
  }

  public void SetProgress(float val){
    _mat_reference.GetMaterial().SetFloat("_Progression", val);
  }
}