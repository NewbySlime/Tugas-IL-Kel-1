using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[RequireComponent(typeof(HealthComponent))]
public class DamagedEffect: MonoBehaviour{
  [SerializeField]
  private SpriteRenderer _TargetSpriteManipulation;
  [SerializeField]
  private Rigidbody2D _TargetExaggerationBody;

  [SerializeField]
  private Animator _TargetAnimator;

  private HealthComponent _health;


  public float DamagedEffectTime;
  public float ChangeColorInterval;
  public float ExaggerationForce;


  private IEnumerator _on_damaged(){
    // tunggu sampai FixedUpdate selanjutnya
    yield return null;
    yield return new WaitForEndOfFrame();

    if(_TargetExaggerationBody != null){
      Vector2 _hit_direction = (_health.SourceDamager.transform.position-transform.position).normalized;
      _hit_direction = _hit_direction.x < 0? new Vector2(1, 1): new Vector2(-1, 1);

      _TargetExaggerationBody.AddForce(_hit_direction.normalized * ExaggerationForce);
    }

    if(_TargetAnimator != null)
      _TargetAnimator.SetBool("is_damaged", true);

    int i = 0;

    float _effect_timer = DamagedEffectTime;
    float _interval_timer = ChangeColorInterval;
    while(_effect_timer > 0){
      _interval_timer -= Time.deltaTime;
      if(_interval_timer <= 0){
        Color _color_add = new Color(0,0,0,0);
        Color _color_mult = new Color(1,1,1,1);

        switch(i){
          // red
          case 0:{
            _color_add = Color.red;
            _color_mult = Color.Lerp(new Color(0,0,0,0), Color.white, 0.7f);
          }break;

          // white
          case 1:{
            _color_add = Color.white;
            _color_mult = Color.Lerp(new Color(0,0,0,0), Color.white, 0.7f);
          }break;

          // default
          default:{
            i = -1;
          }break;
        }

        _color_add.a = 0;
        _TargetSpriteManipulation.material.SetColor("_ColorMultiply", _color_mult);
        _TargetSpriteManipulation.material.SetColor("_ColorAdditiveAfter", _color_add);

        i++;
        _interval_timer = ChangeColorInterval;
      }      

      _effect_timer -= Time.deltaTime;
      yield return null;
    }

    _TargetSpriteManipulation.material.SetColor("_ColorMultiply", Color.white);
    _TargetSpriteManipulation.material.SetColor("_ColorAdditiveAfter", new Color(0,0,0,0));

    if(_TargetAnimator != null)
      _TargetAnimator.SetBool("is_damaged", false);
  }

  private void _on_damaged_async(int damage_points){
    StartCoroutine(_on_damaged());
  }


  public void Start(){
    _health = GetComponent<HealthComponent>();
    _health.OnDamagedEvent += _on_damaged_async;

    RigidbodyMessageRelay _rb_relay = _TargetExaggerationBody.gameObject.GetComponent<RigidbodyMessageRelay>();
    if(_rb_relay == null){
      Debug.LogWarning("Cannot get Relay in Target Rigidbody's GameObject.");
      return;
    }
  }
}