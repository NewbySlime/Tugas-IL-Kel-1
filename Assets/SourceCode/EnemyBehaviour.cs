using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(FollowerBehaviour))]
[RequireComponent(typeof(NPCRandomBehaviour))]
[RequireComponent(typeof(PathFollower))]
public class EnemyBehaviour: InterfaceEnemyBehaviour{
  [SerializeField]
  private AlertTrigger _AlertComponent;
  
  [SerializeField]
  private DamagerComponent _TargetStaticDamager;

  [SerializeField]
  private float _MinStrikeRange = 0.8f;


  private FollowerBehaviour _follower_behaviour;
  private NPCRandomBehaviour _npc_behaviour;

  private MovementController _movement;
  private PathFollower _path_follower;

  private HealthComponent _health_component;

  private HealthComponent _target_health;

  private bool _do_enable_strike_damager = true;


  private void _on_health_damaged(int points){
    _do_enable_strike_damager = false;
    _TargetStaticDamager.enabled = false;
  }

  private void _on_dead(){
    SetEnemy(null);
  }


  private void _on_alerted_enter(GameObject obj){
    SetEnemy(obj);
  }

  private void _on_target_obj_dead(){
    SetEnemy(null);
  }

  private IEnumerator _do_strike(){
    _do_enable_strike_damager = true;

    _path_follower.ForceJump(_TargetEnemy.transform.position);
    yield return new WaitForSeconds(_movement.ForceJumpStartDelay);

    _TargetStaticDamager.enabled = _do_enable_strike_damager;
    yield return new WaitUntil(() => !_path_follower.IsJumping());
    _TargetStaticDamager.enabled = false;
  }


  private IEnumerator _on_target_enemy_changed_co_func(){
    _npc_behaviour.enabled = _TargetEnemy == null;

    yield return null;
    yield return new WaitForEndOfFrame();

    _follower_behaviour.SetTargetFollow(_TargetEnemy);
    if(_TargetEnemy == null){
      if(_target_health != null)
        _target_health.OnDeadEvent -= _on_target_obj_dead;

      _target_health = null;
      yield break;
    }

    _target_health = _TargetEnemy.GetComponent<HealthComponent>();
    if(_target_health == null)
      Debug.LogWarning("TargetEnemy does not have HealthComponent.");
    else{
      _target_health.OnDeadEvent += _on_target_obj_dead;
    }
  }

  private IEnumerator _start_co_func(){
    yield return null;
    yield return new WaitForEndOfFrame();

    _TargetStaticDamager.enabled = false;
  }
  

  protected override void _on_target_enemy_changed(){
    StartCoroutine(_on_target_enemy_changed_co_func());
  }


  public void Start(){
    _follower_behaviour = GetComponent<FollowerBehaviour>();
    _npc_behaviour = GetComponent<NPCRandomBehaviour>();

    _movement = GetComponent<MovementController>();
    _path_follower = GetComponent<PathFollower>();

    _health_component = GetComponent<HealthComponent>();
    if(_health_component != null){
      _health_component.OnDamagedEvent += _on_health_damaged;
      _health_component.OnDeadEvent += _on_dead;
    }

    _AlertComponent.AlertObjectEnterEvent += _on_alerted_enter;

    StartCoroutine(_start_co_func());
  }

  public void FixedUpdate(){
    if(_path_follower.IsJumping())
      return;

    if(_TargetEnemy == null)
      return;

    float _dist_to_object = (_TargetEnemy.transform.position-transform.position).magnitude;
    if(_dist_to_object <= _MinStrikeRange)
      StartCoroutine(_do_strike());
  }
}