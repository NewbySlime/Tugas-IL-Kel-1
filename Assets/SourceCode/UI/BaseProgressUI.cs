using System.Collections;
using UnityEngine;


/// <summary>
/// A base class that any object can use or inherit to use progress bar.
/// This class uses shader to handle the progress bar.
/// 
/// This class uses external component(s);
/// - Target <b>GameObject</b> that has <see cref="IMaterialReference"/> component for manipulating target's material.
/// </summary>
public class BaseProgressUI: MonoBehaviour{
  [SerializeField]
  private GameObject _MaterialReference;

  [SerializeField]
  private Color _BGProgressMult;

  private IMaterialReference _mat_reference;

  private float _current_progress = 0;


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


  /// <summary>
  /// To set progress for the bar.
  /// </summary>
  /// <param name="val">The progress value</param>
  public void SetProgress(float val){
    _current_progress = val;
    _mat_reference.GetMaterial().SetFloat("_Progression", val);
  }

  /// <summary>
  /// To get progress value from the bar.
  /// </summary>
  /// <returns></returns>
  public float GetProgress(){
    return _current_progress;
  }
}