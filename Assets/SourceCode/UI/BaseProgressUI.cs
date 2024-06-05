using System.Collections;
using UnityEngine;


public class BaseProgressUI: MonoBehaviour{
  [SerializeField]
  private IMaterialReference _MaterialReference;

  [SerializeField]
  private Color _BGProgressMult;

  private IEnumerator _start_co_func(){
    yield return null;
    yield return new WaitForEndOfFrame();

    _MaterialReference.GetMaterial().SetColor("_ProgressionColorMultiply", _BGProgressMult);
  }


  public void Start(){
    StartCoroutine(_start_co_func());
  }

  public void SetProgress(float val){
    _MaterialReference.GetMaterial().SetFloat("_Progression", val);
  }
}