using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(MovementController))]
[RequireComponent(typeof(FollowerBehaviour))]
[RequireComponent(typeof(VulnerableEffect))]
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

#if DEBUG
  [SerializeField]
  private GameObject DEBUG_TargetObject;
#endif 

  
  private MovementController _movement;
  private FollowerBehaviour _follower_behaviour;
  private VulnerableEffect _vulnerable_effect;

  private bool _is_striking = false;

  private float _cooldown_strike = -1;


  private IEnumerator _start_strike(){
    _is_striking = true;
    _follower_behaviour.SetTargetFollow(null);

    yield return new WaitForSeconds(_DelayStriking);

    float _walk_val = _StrikingSpeed/_movement.MovementSpeed;

    Vector2 _delta_target = _TargetEnemy.transform.position-transform.position;
    Vector2 _strike_direction = _delta_target.normalized;
    Func<bool> _wallhug_check = _strike_direction.x > 0 ? _movement.IsWallHuggingRight: _movement.IsWallHuggingLeft;
    Debug.Log("boss start striking"); 
    _StrikeDamager.enabled = true;
    while(!_wallhug_check()){
      _movement.DoWalk(_walk_val * (_strike_direction.x > 0? 1: -1));;
      yield return new WaitForFixedUpdate();
    }

    _StrikeDamager.enabled = false;
    Debug.Log("boss stopped striking striking");
    _movement.DoWalk(0);

    yield return new WaitForFixedUpdate();
    _movement.enabled = false;

    Vector2 _exaggeration_direction = new Vector2(_strike_direction.x > 0? -1: 1, 3).normalized;
    Debug.Log(string.Format("exaggeration dir {0}", _exaggeration_direction));
    _movement.GetRigidbody().AddForce(_exaggeration_direction * _BumpExaggerationForce);

    _vulnerable_effect.StartEffect();
    yield return new WaitUntil(_vulnerable_effect.IsEffectStopped);
    _movement.enabled = true;

    _follower_behaviour.SetTargetFollow(_TargetEnemy);
    _is_striking = false;

    _cooldown_strike = _StrikingCooldownDelay;
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

    Debug.Log("striking.");
    StartCoroutine(_start_strike());
  }
}