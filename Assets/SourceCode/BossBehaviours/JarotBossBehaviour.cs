using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(MovementController))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(FollowerBehaviour))]
[RequireComponent(typeof(VulnerableEffect))]
/// <summary>
/// Enamy Behaviour controller for Jarot. For behaviour diagram, look at <b>Reference/Diagrams/JarotBossBehaviour.drawio</b>.
/// 
/// This behaviour uses following component(s);
/// - <see cref="MovementController"/> to let the behaviour have control over the object movement.
/// - <see cref="HealthComponent"> to give the object ability to have vitality features.
/// - <see cref="FollowerBehaviour"/> as an extension of the behaviour used as a AI follow helper.
/// - <see cref="VulnerableEffect"/> used as a vulnerable time window after power striking.
/// 
/// This behaviour uses external component(s);
/// - <see cref="DamagerComponent"/> collider object that seperate from the actual object that acts as a Damager.
/// - <b>Animator</b> Unity animation handler for certain object state.
/// - <b>DEBUG_TARGET</b> [DEBUG_MODE] target object to follow once the Game started.
/// </summary>
public class JarotBossBehaviour: InterfaceEnemyBehaviour{
  [SerializeField]
  private float _MinStrikeRange = 5f;

  [SerializeField]
  private float _DelayStriking = 1.2f;

  [SerializeField]
  private float _StrikingCooldownDelay = 0.4f;

  [SerializeField]
  private float _StrikingSpeed = 20f;

  [SerializeField]
  private float _StrikingAngleMax = 20;
  [SerializeField]
  private float _StrikingAngleMin = -20;

  [SerializeField]
  private float _BumpExaggerationForce = 300f;

  [SerializeField]
  private DamagerComponent _StrikeDamager;

  [SerializeField]
  private Animator _TargetAnimator;

#if DEBUG
  [SerializeField]
  private GameObject DEBUG_TargetObject;
#endif 

  private MovementController _movement;
  private HealthComponent _health_component;
  private FollowerBehaviour _follower_behaviour;
  private VulnerableEffect _vulnerable_effect;

  private bool _is_striking = false;

  private float _cooldown_strike = -1;


  private IEnumerator _start_strike(){
    _is_striking = true;
    _follower_behaviour.SetTargetFollow(null);

    yield return null;
    yield return new WaitForEndOfFrame();

    Vector2 _delta_target = _TargetEnemy.transform.position-transform.position;
    Vector2 _strike_direction = _delta_target.normalized;

    _movement.LookAt(_strike_direction);

    if(_TargetAnimator != null)
      _TargetAnimator.SetInteger("striking_idx", 1);

    yield return new WaitForSeconds(_DelayStriking);

    if(_TargetAnimator != null)
      _TargetAnimator.SetInteger("striking_idx", 2);

    float _walk_val = _StrikingSpeed/_movement.MovementSpeed;
    Func<bool> _wallhug_check = _strike_direction.x > 0 ? _movement.IsWallHuggingRight: _movement.IsWallHuggingLeft;
    DEBUGModeUtils.Log("boss start striking"); 
    _StrikeDamager.enabled = true;
    while(!_wallhug_check()){
      _movement.DoWalk(_walk_val * (_strike_direction.x > 0? 1: -1));;
      yield return new WaitForFixedUpdate();
    }

    if(_TargetAnimator != null)
      _TargetAnimator.SetInteger("striking_idx", 0);

    _StrikeDamager.enabled = false;
    DEBUGModeUtils.Log("boss stopped striking striking");
    _movement.DoWalk(0);

    yield return new WaitForFixedUpdate();
    _movement.enabled = false;

    Vector2 _exaggeration_direction = new Vector2(_strike_direction.x > 0? -1: 1, 3).normalized;
    DEBUGModeUtils.Log(string.Format("exaggeration dir {0}", _exaggeration_direction));
    _movement.GetRigidbody().AddForce(_exaggeration_direction * _BumpExaggerationForce);

    _vulnerable_effect.StartEffect();
    yield return new WaitUntil(_vulnerable_effect.IsEffectStopped);
    _movement.enabled = true;

    _follower_behaviour.SetTargetFollow(_TargetEnemy);
    _is_striking = false;

    _cooldown_strike = _StrikingCooldownDelay;
  }

  private void _on_dead(){
    SetEnemy(null);
  }


#if DEBUG
  private IEnumerator _debug_start(){
    yield return null;
    yield return new WaitForEndOfFrame();

    SetEnemy(DEBUG_TargetObject);
  }
#endif


  protected override void _on_target_enemy_changed(){
    _follower_behaviour.SetTargetFollow(_TargetEnemy);
  }


  public void Start(){
    _health_component = GetComponent<HealthComponent>();
    _health_component.OnDeadEvent += _on_dead;

    _movement = GetComponent<MovementController>();
    _follower_behaviour = GetComponent<FollowerBehaviour>();
    _vulnerable_effect = GetComponent<VulnerableEffect>();

    _StrikeDamager.enabled = false;

    _StrikingAngleMin = Mathf.Repeat(180+_StrikingAngleMin, 180);

#if DEBUG
    StartCoroutine(_debug_start());
#endif
  }
  
  public void FixedUpdate(){
    if(_is_striking)
      return;

    if(_TargetEnemy == null)
      return;

    if(_cooldown_strike > 0){
      _cooldown_strike -= Time.fixedDeltaTime;
      return;
    }

    Vector2 _delta_target = _TargetEnemy.transform.position-transform.position;
    Vector2 _direction = _delta_target.normalized;
    float _current_angle = Mathf.Repeat(360+MathExt.DirectionToAngle(_direction)-MathExt.DirectionToAngle(Vector2.right), 180);
    if(_current_angle < _StrikingAngleMin && _current_angle > _StrikingAngleMax)
      return;

    float _distance_to_target = _delta_target.magnitude;
    if(_distance_to_target > _MinStrikeRange)
      return;

    DEBUGModeUtils.Log("striking.");
    StartCoroutine(_start_strike());
  }
}