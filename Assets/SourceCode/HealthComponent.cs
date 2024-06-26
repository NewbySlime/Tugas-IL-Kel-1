using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;


public class HealthComponent: MonoBehaviour{
  public const string AudioID_Dead = "dead";


  public delegate void OnHealthChanged(int new_health);
  public event OnHealthChanged OnHealthChangedEvent;

  public delegate void OnDamaged(int points);
  public event OnDamaged OnDamagedEvent;

  public delegate void OnHealed(int points);
  public event OnHealed OnHealedEvent;

  public delegate void OnDamageCancelled();
  public event OnDamageCancelled OnDamageCancelledEvent;

  public delegate void OnDead();
  public event OnDead OnDeadEvent;

  public delegate void OnRuntimeDataSet();
  public event OnRuntimeDataSet OnRuntimeDataSetEvent;

  public struct HealthContext{
    public int HealthPoint;
    public bool InvincibleFlag;
  }


  [Serializable]
  public struct RuntimeData{
    public int CurrentHealth;
  }


  [SerializeField]
  private int _MaxHealth;
  public int MaxHealth{get => _MaxHealth;}

  [SerializeField]
  private Rigidbody2D _TargetRigidbody;

  [SerializeField]
  private Animator _TargetAnimator;

  [SerializeField]
  private AudioCollectionHandler _TargetAudioHandler;

  [SerializeField]
  private float _DestroyTimer;

  [SerializeField]
  private bool _IsInvincible = false;

  private int _current_health;
  private bool _invincible_flag;

  private bool _death_effect_triggering = false;

  public bool TriggerDeadAnimation = true;
  public bool FallOffScreenOnDead = true;
  public bool DestroyOnDead = true;

  public DamagerComponent SourceDamager{private set; get;}


  private IEnumerator _trigger_dead_effect(){
    DEBUGModeUtils.Log("triggering dead anim");
    _death_effect_triggering = true;
    if(_TargetRigidbody != null && FallOffScreenOnDead)
      _TargetRigidbody.excludeLayers = LayerMask.NameToLayer("Everything");
    
    yield return new WaitForSeconds(_DestroyTimer);
    _death_effect_triggering = false;

    if(!DestroyOnDead)
      yield break;
    
    // wait until coroutines picked up on _death_effect_triggering to false using WaitUntil
    yield return null;
    yield return new WaitForEndOfFrame();

    Destroy(gameObject);
  }


  private void _trigger_on_dead(){
    StartCoroutine(_trigger_dead_effect());
    OnDeadEvent?.Invoke();
  }


  private void _check_health(){
    DEBUGModeUtils.Log("Check health");
    OnHealthChangedEvent?.Invoke(_current_health);

    if(_current_health > _MaxHealth)
      _current_health = _MaxHealth;

    if(_current_health <= 0)
      _trigger_on_dead();

    if(_TargetAnimator != null && TriggerDeadAnimation)
      _TargetAnimator.SetBool("is_dead", _current_health <= 0);

    if(_TargetAudioHandler != null && TriggerDeadAnimation)
      _TargetAudioHandler.TriggerSound(AudioID_Dead);
  }
  

  public void Start(){
    SetHealth(new(){
      HealthPoint = MaxHealth,
      InvincibleFlag = _IsInvincible
    });
  }


  public bool IsDead(){
    return _current_health <= 0;
  }

  public bool IsDeadEffectDone(){
    return !_death_effect_triggering;
  }


  public void SetHealth(HealthContext context){
    _current_health = context.HealthPoint;
    _invincible_flag = context.InvincibleFlag;

    _check_health();
  }

  public int GetHealth(){
    return _current_health;
  }


  public void DoDamage(DamagerComponent.DamagerData damage_data, DamagerComponent source = null){
    if(_invincible_flag){
      OnDamageCancelledEvent?.Invoke();
      return;
    }

    SourceDamager = source;
    
    _current_health -= (int)damage_data.damage_points;
    OnDamagedEvent?.Invoke((int)damage_data.damage_points);

    _check_health();
  }

  public void DoHeal(uint heal_points){
    _current_health += (int)heal_points;
    OnHealedEvent?.Invoke((int)heal_points);

    _check_health();
  }


  public RuntimeData AsRuntimeData(){
    return new(){
      CurrentHealth = _current_health
    };
  }


  public void FromRuntimeData(RuntimeData data){
    DEBUGModeUtils.Log(string.Format("runtime health: {0}", data.CurrentHealth));
    _current_health = data.CurrentHealth;
    _check_health();

    OnRuntimeDataSetEvent?.Invoke();
  }
}