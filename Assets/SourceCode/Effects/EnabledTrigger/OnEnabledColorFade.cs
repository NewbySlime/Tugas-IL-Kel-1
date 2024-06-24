using System.Collections;
using UnityEngine;


public class OnEnabledColorFade: MonoBehaviour, IEnableTrigger, IObjectInitialized{
  [SerializeField]
  private GameObject _MaterialReference;

  [SerializeField]
  private Color _FadeFromColorMult;
  [SerializeField]
  private Color _FadeFromColorAdd;

  [SerializeField]
  private float _FadeTime;

  [SerializeField]
  private bool _UseScaledTiming = true;

  private IMaterialReference _mat_reference;

  private Coroutine _effect_coroutine;


  public bool IsInitialized{private set; get;} = false;
  public bool EnableTrigger = true;


  private void _trigger_effect(){
    DEBUGModeUtils.Log(string.Format("trigger effect {0}", !EnableTrigger));
    DEBUGModeUtils.Log(string.Format("trigger effect {0}", _effect_coroutine != null));
    DEBUGModeUtils.Log(string.Format("trigger effect {0}", !IsInitialized));
    if(!EnableTrigger || _effect_coroutine != null || !IsInitialized)
      return;

    _effect_coroutine = StartCoroutine(_trigger_effect_co_func());
  }

  private IEnumerator _trigger_effect_co_func(){
    Color _mult_to_color = _FadeFromColorMult;
    _mult_to_color.a = 0;
    
    Material _this_mat = _mat_reference.GetMaterial();

    float _timer = _FadeTime;
    while(_timer > 0){
      float _val = _timer/_FadeTime;
      _this_mat.SetColor("_ColorMultiply", Color.Lerp(_mult_to_color, _FadeFromColorMult, _val));
      _this_mat.SetColor("_ColorAdditiveAfter", Color.Lerp(new Color(0,0,0,0), _FadeFromColorAdd, _val));
      _this_mat.SetFloat("_AlphaMaskWeight", _val);

      yield return null;

      _timer -= _UseScaledTiming? Time.deltaTime: Time.unscaledDeltaTime;
    }

    _effect_finished();
  }

  private void _effect_finished(){
    _effect_coroutine = null;

    Material _this_mat = _mat_reference.GetMaterial();
    _this_mat.SetColor("_ColorMultiply", new Color(0,0,0,0));
    _this_mat.SetColor("_ColorAdditiveAfter", new Color(0,0,0,0));
      _this_mat.SetFloat("_AlphaMaskWeight", 0);
  }


  private IEnumerator _start_co_func(){
    yield return null;
    yield return new WaitForEndOfFrame();

    IsInitialized = true;
    _trigger_effect();
  }


  public void Start(){
    IMaterialReference[] _list_refs = _MaterialReference.GetComponents<IMaterialReference>();
    if(_list_refs.Length <= 0){
      Debug.LogError("MaterialReference does not have IMaterialReference Object.");
      throw new MissingReferenceException();
    }
    else if(_list_refs.Length > 1)
      Debug.LogWarning("MaterialReference has more than one IMaterialReference. It is not yet supported.");

    _mat_reference = _list_refs[0];
    StartCoroutine(_start_co_func());
  }


  public void CancelEffect(){
    if(_effect_coroutine == null)
      return;

    StopCoroutine(_effect_coroutine);
    _effect_finished();
  }


  public void OnEnable(){
    _trigger_effect();
  }

  public void OnDisable(){
    CancelEffect();
  }


  public void TriggerSetOnEnable(bool flag){
    DEBUGModeUtils.Log(string.Format("trigger set {0}", flag));
    EnableTrigger = flag;
  }


  public bool GetIsInitialized(){
    return IsInitialized;
  }
}