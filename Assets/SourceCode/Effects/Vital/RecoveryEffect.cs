using UnityEngine;
using System.Collections;


[RequireComponent(typeof(HealthComponent))]
public class RecoveryEffect: MonoBehaviour{
  [SerializeField]
  private LayerMask _ExcludedLayerRecovery;
  [SerializeField]
  private Rigidbody2D _TargetRecoveryBody;
  
  [SerializeField]
  private SpriteRenderer _TargetSpriteManipulation;


  private HealthComponent _health;

  public float RecoveryTime = 5f;

  public float MaxBlinkInterval = 0.7f;
  public float MinBlinkInterval = 0.2f;

  public float BlinkVisibleInterval = 0.1f;


  private IEnumerator _on_damaged(){
    if(_TargetRecoveryBody == null)
      yield break;

    LayerMask _previous_layer = _TargetRecoveryBody.excludeLayers;
    _TargetRecoveryBody.excludeLayers = _ExcludedLayerRecovery;

    DamagedEffect _effect = GetComponent<DamagedEffect>();
    if(_effect != null)
      yield return new WaitForSeconds(_effect.DamagedEffectTime);
    
    float _recovery_timer = RecoveryTime;
    float _blink_timer = MaxBlinkInterval;
    int _blink_iter = 0;
    while(_recovery_timer > 0){
      yield return null;

      if(_blink_timer > 0){
        _blink_timer -= Time.deltaTime;

        if(_blink_timer <= 0){
          switch(_blink_iter){
            case 0:{
              _TargetSpriteManipulation.material.SetColor("_ColorMultiply", new Color(0,0,0,0));
              _TargetSpriteManipulation.material.SetFloat("_AlphaMaskWeight", 0);

              _blink_timer = (MaxBlinkInterval-MinBlinkInterval) * (_recovery_timer/RecoveryTime) + MinBlinkInterval;
            }break;

            default:{
              _TargetSpriteManipulation.material.SetColor("_ColorMultiply", Color.white);
              _TargetSpriteManipulation.material.SetFloat("_AlphaMaskWeight", 1);

              _blink_timer = BlinkVisibleInterval;
              _blink_iter = -1;
            }break;
          }

          _blink_iter++;
        }

        Debug.Log(string.Format("blink timer {0}", _blink_timer));
      }

      _recovery_timer -= Time.deltaTime;
    }

    _TargetSpriteManipulation.material.SetColor("_ColorMultiply", Color.white);
    _TargetSpriteManipulation.material.SetFloat("_AlphaMaskWeight", 1);
    _TargetRecoveryBody.excludeLayers = _previous_layer;
  }

  private void _on_damaged_async(int damage_points){
    if(_health.GetHealth() <= 0)
      return;

    StartCoroutine(_on_damaged());
  }

  
  public void Start(){
    _health = GetComponent<HealthComponent>();
    _health.OnDamagedEvent += _on_damaged_async;
  }
}