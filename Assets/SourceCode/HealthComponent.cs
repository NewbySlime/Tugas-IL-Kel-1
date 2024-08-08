using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;


/// <summary>
/// Base component for Game's entities to handle health system for the entity. 
/// When the entity is "dead" the entity will be despawned, but can be opted to not despawned for reasons like presentation of the story.
/// 
/// This class uses external component(s);
/// - <see cref="Rigidbody2D"/> as the base body of the component.
/// - <see cref="Animator"/> for animation handling for effects used in this component.
/// - <see cref="AudioCollectionHandler"/> for playing audio of certain ID used as the effects in this component.
/// 
/// For another effects that can be used alongside this component;
/// - <see cref="DamagedEffect"/>
/// - <see cref="HealedEffect"/>
/// - <see cref="RecoveryEffect"/>
/// - <see cref="VulnerableEffect"/>
/// </summary>
public class HealthComponent: MonoBehaviour{
  /// <summary>
  /// ID of the audio used when in "dead" state.
  /// </summary>
  public const string AudioID_Dead = "dead";


  /// <summary>
  /// Event for when this health point are changed (cumulatively or deductively).
  /// </summary>
  public event OnHealthChanged OnHealthChangedEvent;
  public delegate void OnHealthChanged(int new_health);

  /// <summary>
  /// Event for when this component are damaged (or the health point are deducted).
  /// </summary>
  public event OnDamaged OnDamagedEvent;
  public delegate void OnDamaged(int points);

  /// <summary>
  /// Event for when this component are healed (or the health point are increased).
  /// </summary>
  public event OnHealed OnHealedEvent;
  public delegate void OnHealed(int points);

  /// <summary>
  /// Event for when applying damage has been cancelled (in invulnerable state). 
  /// </summary>
  public event OnDamageCancelled OnDamageCancelledEvent;
  public delegate void OnDamageCancelled();

  /// <summary>
  /// Event for when the component is dead (the health point are equal or below zero).
  /// </summary>
  public event OnDead OnDeadEvent;
  public delegate void OnDead();

  /// <summary>
  /// Event for when the configuration or data in this component has been changed by using <see cref="RuntimeData"/>.
  /// </summary>
  public event OnRuntimeDataSet OnRuntimeDataSetEvent;
  public delegate void OnRuntimeDataSet();

  
  /// <summary>
  /// Configuration data for <see cref="HealthComponent"/>.
  /// </summary>
  public struct HealthContext{
    public int HealthPoint;
    
    /// <summary>
    /// Any incoming damage will be ignored if this flag is enabled.
    /// </summary>
    public bool InvincibleFlag;
  }


  [Serializable]
  /// <summary>
  /// Data about <see cref="HealthComponent"/> for storing current state of the component.
  /// </summary>
  public struct RuntimeData{
    /// <summary>
    /// The health point in current state.
    /// </summary>
    public int CurrentHealth;
  }


  [SerializeField]
  private int _MaxHealth;
  /// <summary>
  /// The maximum health point of this component. Cannot be changed.
  /// </summary>
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

  /// <summary>
  /// Should the animation played when the component is in "dead" state.
  /// </summary>
  public bool TriggerDeadAnimation = true;
  
  /// <summary>
  /// Should the object fallen after the component is in "dead" state.
  /// Fallen "animation" handled dynamically by <b>Rigidbody</b> by ignoring every physics layer.
  /// </summary>
  public bool FallOffScreenOnDead = true;
  
  /// <summary>
  /// Should this object destroyed when the component is in "dead" state. The component will not instantly destroying the object, but it will wait for timer (_DestroyTimer) to finish.
  /// </summary>
  public bool DestroyOnDead = true;

  /// <summary>
  /// Previous damager that interacted with this component. Cannot be changed.
  /// </summary>
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


  /// <summary>
  /// To check if this object is in "dead" state.
  /// </summary>
  /// <returns>Is the object "dead"</returns>
  public bool IsDead(){
    return _current_health <= 0;
  }

  /// <summary>
  /// To check if the animation/effect for "dead" state is finished or not.
  /// </summary>
  /// <returns></returns>
  public bool IsDeadEffectDone(){
    return !_death_effect_triggering;
  }


  /// <summary>
  /// Sets this component configuration by using <see cref="HealthContext"/>.
  /// </summary>
  /// <param name="context">The health configuration to use</param>
  public void SetHealth(HealthContext context){
    _current_health = context.HealthPoint;
    _invincible_flag = context.InvincibleFlag;

    _check_health();
  }

  /// <summary>
  /// Gets current health point.
  /// </summary>
  /// <returns>Current health point</returns>
  public int GetHealth(){
    return _current_health;
  }


  /// <summary>
  /// Damage this component.
  /// </summary>
  /// <param name="damage_data">The damage data</param>
  /// <param name="source">Source of the damager</param>
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

  /// <summary>
  /// Heal this component.
  /// </summary>
  /// <param name="heal_points">How health points to heal</param>
  public void DoHeal(uint heal_points){
    _current_health += (int)heal_points;
    OnHealedEvent?.Invoke((int)heal_points);

    _check_health();
  }


  /// <summary>
  /// Gets internal data as runtime data for storing its current state.
  /// </summary>
  /// <returns>The internal data</returns>
  public RuntimeData AsRuntimeData(){
    return new(){
      CurrentHealth = _current_health
    };
  }


  /// <summary>
  /// Configure/restore this class using stored state.
  /// </summary>
  /// <param name="data">The stored state</param>
  public void FromRuntimeData(RuntimeData data){
    DEBUGModeUtils.Log(string.Format("runtime health: {0}", data.CurrentHealth));
    _current_health = data.CurrentHealth;
    _check_health();

    OnRuntimeDataSetEvent?.Invoke();
  }
}