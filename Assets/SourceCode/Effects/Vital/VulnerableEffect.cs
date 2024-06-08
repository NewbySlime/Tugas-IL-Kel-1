using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;



[RequireComponent(typeof(HealthComponent))]
public class VulnerableEffect: MonoBehaviour{
  [SerializeField]
  private float _EffectTime = 3;
  [SerializeField]
  private bool _CancelEffectOnDamaged = true;

  [SerializeField]
  private Animator _TargetAnimator;

  [SerializeField]
  private SpriteRenderer _TargetVisualEffect;

  [SerializeField]
  private float _VisEffectBounceTimeMax = 0.4f;
  [SerializeField]
  private float _VisEffectBounceTimeMin = 0.1f;

  [SerializeField]
  private Color _VisEffectBounceToColor = Color.yellow;

  [SerializeField]
  private Material _VisEffectMaterial;


  private Material _instantiated_mat;
  private Material _default_mat;

  private HealthComponent _health;

  private float _effect_timer = -1;

  private Coroutine _viseffect_coroutine = null;


  private IEnumerator _trigger_viseffect(){
    if(_TargetVisualEffect == null)
      yield break;

    _TargetVisualEffect.material = _instantiated_mat;
    while(_effect_timer > 0){
      float _base_fade_time = 
        (_VisEffectBounceTimeMax-_VisEffectBounceTimeMin) *
        (_effect_timer/_EffectTime) +
        _VisEffectBounceTimeMin;

      float _fade_timer = _base_fade_time;

      while(_fade_timer > 0){
        float _color_lerp_value = Mathf.Abs(_fade_timer - _base_fade_time/2) / (_base_fade_time/2);
        Color _result_col = Color.Lerp(new Color(0,0,0,0), _VisEffectBounceToColor, _color_lerp_value);

        _instantiated_mat.SetColor("_ColorAdditiveAfter", _result_col);
        
        yield return null;
        _fade_timer -= Time.deltaTime;
      }
    }

    _trigger_viseffect_finished();
  }

  private void _trigger_viseffect_finished(){
    _TargetVisualEffect.material = _default_mat;
    _viseffect_coroutine = null;
    _effect_timer = 0;
  }


  private void _set_invincible(bool invincible){
    _health.SetHealth(new(){
      HealthPoint = _health.GetHealth(),
      InvincibleFlag = invincible
    });
    
    if(_TargetAnimator != null)
      _TargetAnimator.SetBool("is_vulnerable", !invincible);
  }


  private IEnumerator _start_co_func(){
    yield return null;
    yield return new WaitForEndOfFrame();

    _set_invincible(true);
  }


  public void Start(){
    _health = GetComponent<HealthComponent>();

    if(_TargetVisualEffect != null)
      _default_mat = _TargetVisualEffect.material;
    
    _instantiated_mat = new Material(_VisEffectMaterial);
    StartCoroutine(_start_co_func());
  }

  public void FixedUpdate(){
    if(_effect_timer > 0){
      _effect_timer -= Time.fixedDeltaTime;
      
      if(_effect_timer <= 0)
        CancelEffect();
    }
  }

  
  public void StartEffect(){
    _set_invincible(false);
    _effect_timer = _EffectTime;

    if(_viseffect_coroutine == null)
      _viseffect_coroutine = StartCoroutine(_trigger_viseffect());
  }

  public bool IsEffectStopped(){
    return _effect_timer <= 0;
  }


  public void CancelEffect(){
    StopCoroutine(_viseffect_coroutine);

    _set_invincible(true);
    _trigger_viseffect_finished();
  }
}