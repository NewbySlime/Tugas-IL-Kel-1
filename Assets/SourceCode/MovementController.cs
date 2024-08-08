using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(Rigidbody2D))]
/// <summary>
/// Component for handling movement system used in the Game. For further explanation, see <b>Reference/Diagrams/MovementController.drawio</b>
/// 
/// IMPORTANT This class needs child objects to be functional;
/// - <b>"GroundCheck"</b> physics body (trigger) that has <see cref="RigidbodyMessageRelay"/> to let this class know that this object has touched a ground.
/// - (Optional) <b>"WallHugRight"</b> and <b>"WallHugLeft"</b> physics body (trigger) that has <see cref="RigidbodyMessageRelay"/> to let this class know that this object has touched a wall (that can be used for wall-jumping).
/// 
/// This class uses following component(s);
/// - <b>Rigidbody2D</b> physics body for this class.
/// - <b>Collider2D (any shape)</b> collider component for the physics body.
/// 
/// This class uses external component(s);
/// - <see cref="RigidbodyMessageRelay"/> for getting physics event for another detached part of the body.
/// - <b>Unity's Animator</b> component for handling movement animation.
/// - <see cref="AudioCollectionHandler"/> for playing certain audio used in the animation.
/// </summary>
public class MovementController: MonoBehaviour{
  /// <summary>
  /// Audio ID used for playing "jumping" character sound. 
  /// </summary>
  public const string AudioID_Jump = "jump";


  [SerializeField]
  // Clamped values of input direction for "walking" state of the component.
  private float WalkNormalizeClamp = 0.4f;

  [SerializeField]
  // Should the component have the ability to clamp its input direction value for "walking" state.
  private bool ClampWalkDir = true;

  [SerializeField]
  private float movement_speed = 100f;
  /// <summary>
  /// The supposed movement speed of the object.
  /// </summary>
  public float MovementSpeed{get => movement_speed;}

  [SerializeField]
  private float movement_speed_midair = 10f;

  [SerializeField]
  private float jump_force = 100f;

  [SerializeField]
  private float walljump_force = 80f;

  [SerializeField]
  private Vector2 walljump_direction = new Vector2(0.5f, 0.5f);

  [SerializeField]
  // How long (in Y-Axis) the jump to be exaggerated from the target position.
  private float forcejump_y_exaggeration = 1.5f;
  
  [SerializeField]
  private float forcejump_startdelay = 0.5f;
  /// <summary>
  /// How long the action delay should be when forcing this component to jump.
  /// </summary>
  public float ForceJumpStartDelay{get => forcejump_startdelay;}

  [SerializeField]
  private float forcejump_finishdelay = 1f;

  [SerializeField]
  private Animator _TargetAnimatorMovement;

  [SerializeField]
  private AudioCollectionHandler _TargetAudioHandler;

  private Rigidbody2D _object_rigidbody;
  private Collider2D _object_collider;

  private RigidbodyMessageRelay _ground_check_relay;

  private RigidbodyMessageRelay _wh_right_check_relay;
  private RigidbodyMessageRelay _wh_left_check_relay;

  private HashSet<int> _one_way_ground_collider_set = new();
  private HashSet<int> _one_way_collider_set = new();
  private LayerMask _OneWayColliderMask;

  private HashSet<int> _ground_collider_set = new();
  private bool _is_touching_ground;

  private Vector3 _last_position;


  private HashSet<int> _wallhug_left_collider_set = new();
  private bool _is_wallhug_left;
  
  private HashSet<int> _wallhug_right_collider_set = new();
  private bool _is_wallhug_right;

  private bool _forcejump_flag = false;

  private bool _is_movement_enabled = true;

  private float _walk_dir_x = 0;

  private bool _set_ignore_one_way_flag = false;


  /// <summary>
  /// Should the movement direction clamped to "walking" state.
  /// </summary>
  public bool ToggleWalk = false;


  [HideInInspector]
  /// <summary>
  /// This will force this component to force only look at certain direction prompted using function <see cref="LookAt"/>.
  /// </summary>
  public bool FixedLookAt = false;

  [HideInInspector]
  /// <summary>
  /// Should this component disable the gliding prevention measures.
  /// </summary>
  public bool GlideOnGround = false;


  public void Start(){
    _OneWayColliderMask = LayerMask.NameToLayer("LevelObstacleOneWay");

    _object_rigidbody = GetComponent<Rigidbody2D>();
    _object_collider = GetComponent<Collider2D>();
    
    GameObject _ground_check = transform.Find("GroundCheck").gameObject;
    _ground_check_relay = _ground_check.GetComponent<RigidbodyMessageRelay>();
    _ground_check_relay.OnTriggerEntered2DEvent += OnGroundCheck_Enter;
    _ground_check_relay.OnTriggerExited2DEvent += OnGroundCheck_Exit;

    Transform _wallhug_right_check_tr = transform.Find("WallHugRight");
    if(_wallhug_right_check_tr != null){
      GameObject _wallhug_right_check = _wallhug_right_check_tr.gameObject;
      _wh_right_check_relay = _wallhug_right_check.GetComponent<RigidbodyMessageRelay>();
      _wh_right_check_relay.OnTriggerEntered2DEvent += OnHugWall_Right_Enter;
      _wh_right_check_relay.OnTriggerExited2DEvent += OnHugWall_Right_Exit;
    }

    Transform _wallhug_left_check_tr = transform.Find("WallHugLeft");
    if(_wallhug_left_check_tr != null){
      GameObject _wallhug_left_check = _wallhug_left_check_tr.gameObject;
      _wh_left_check_relay = _wallhug_left_check.GetComponent<RigidbodyMessageRelay>();
      _wh_left_check_relay.OnTriggerEntered2DEvent += OnHugWall_Left_Enter;
      _wh_left_check_relay.OnTriggerExited2DEvent += OnHugWall_Left_Exit;
    }
  }

  public void FixedUpdate(){
    Vector3 _speed = (transform.position - _last_position) / Time.fixedDeltaTime;
    _last_position = transform.position;

    // portion of code that handles movement
    if(!_forcejump_flag && _is_movement_enabled){
      float _clamped_walk_dir_x = ClampWalkDir? Mathf.Clamp(_walk_dir_x, -1, 1): _walk_dir_x;
      if(ToggleWalk)
        _clamped_walk_dir_x = Mathf.Clamp(_walk_dir_x, -WalkNormalizeClamp, WalkNormalizeClamp);

      if(_is_touching_ground && !GlideOnGround){
        _object_rigidbody.velocity = new Vector2(
          _clamped_walk_dir_x * movement_speed,
          _object_rigidbody.velocity.y
        );
      }
      else{
        Vector2 _result_vel = _object_rigidbody.velocity;
        _result_vel.x += _clamped_walk_dir_x * Time.fixedDeltaTime * movement_speed_midair;
        if(MathF.Abs(_result_vel.x) <= movement_speed)
          _object_rigidbody.velocity = _result_vel;
      }
    }


    // portion of code that handles animation based on the movement
    if(_TargetAnimatorMovement != null){
      float _val_velocity_x = _object_rigidbody.velocity.x/movement_speed;
      if(!FixedLookAt)
        _TargetAnimatorMovement.SetFloat("speed_horizontal", _val_velocity_x);

      _TargetAnimatorMovement.SetFloat("speed_horizontal_abs", Mathf.Abs(_val_velocity_x));

      float _val_velocity_y = 0;
      if(!_is_touching_ground)
        _val_velocity_y = _object_rigidbody.velocity.y/(jump_force*Time.fixedDeltaTime);

      _TargetAnimatorMovement.SetFloat("speed_vertical", _val_velocity_y);
      _TargetAnimatorMovement.SetFloat("speed_vertical_abs", Mathf.Abs(_val_velocity_y));
    }
  }


  /// <summary>
  /// Force this object to look at certain direction.
  /// </summary>
  /// <param name="direction">The direction to look at</param>
  public void LookAt(Vector2 direction){
    if(_TargetAnimatorMovement == null)
      return;

    if(Mathf.Abs(direction.x) > 0.1)
      _TargetAnimatorMovement.SetFloat("speed_horizontal", direction.x < 0? -1: 1);
  }


  /// <summary>
  /// Give input to this movement object to walk in a direction (X-Axis). 
  /// </summary>
  /// <param name="walk_dir_x">The normalized direction (in X-Axis)</param>
  public void DoWalk(float walk_dir_x){
    _walk_dir_x = walk_dir_x;
  } 


  /// <summary>
  /// Function to jump vertically. If the object is not on ground, but currently "wallhugging", this object will wall-jump instead of normal jump.
  /// </summary>
  public void DoJump(){
    if(!_is_movement_enabled)
      return;

    bool _is_trigger_jumping = false;
    if(_is_touching_ground){
      //_is_touching_ground = false;
      _object_rigidbody.AddForce(Vector3.up * jump_force);

      _is_trigger_jumping = true;
    }
    else if(_is_wallhug_right){
      //_is_wallhug_right = false;

      Vector2 _inv_dir = new Vector2(-walljump_direction.x, walljump_direction.y);
      _object_rigidbody.velocity = Vector2.zero;
      _object_rigidbody.AddForce(_inv_dir.normalized * walljump_force);

      _is_trigger_jumping = true;
    }
    else if(_is_wallhug_left){
      //_is_wallhug_left = false;
      
      _object_rigidbody.velocity = Vector2.zero;
      _object_rigidbody.AddForce(walljump_direction.normalized * walljump_force);
      
      _is_trigger_jumping = true;
    }

    if(_is_trigger_jumping){
      if(_TargetAudioHandler != null)
        _TargetAudioHandler.TriggerSound(AudioID_Jump);
    }
  }


  /// <summary>
  /// Function to enable the movement processing.
  /// </summary>
  /// <param name="enabled">Flag to enable movement processing</param>
  public void SetEnableMovement(bool enabled){
    _is_movement_enabled = enabled;
  }


  /// <summary>
  /// Check if this object is currently wallhugging (using wall of the object's left side).
  /// </summary>
  /// <returns>Is currently wallhugging</returns>
  public bool IsWallHuggingLeft(){
    return _is_wallhug_left;
  }

  /// <summary>
  /// Check if this object is currently wallhugging (using wall of the object's right side).
  /// </summary>
  /// <returns>Is currently wallhugging</returns>
  public bool IsWallHuggingRight(){
    return _is_wallhug_right;
  }

  /// <summary>
  /// Get <b>Rigidbody2D</b> physics body of this object. 
  /// </summary>
  /// <returns>The physics body</returns>
  public Rigidbody2D GetRigidbody(){
    return _object_rigidbody;
  }


  /// <summary>
  /// NOTE: Feature is unstable for now.
  /// Ignore "One way collision" objects from able to be interacted by this physics object.
  /// </summary>
  /// <param name="flag">Should the interaction ignored or not</param>
  public void SetIgnoreOneWayCollision(bool flag){
    _set_ignore_one_way_flag = flag;

    if(flag)
      _object_rigidbody.excludeLayers |= 1 << _OneWayColliderMask;
  }


  /// <summary>
  /// Let this object to jump in certain trajectory to reach target position. This function is different from <see cref="DoJump"/>, while the function only jumps in vertical direction, this function will jump in any direction in certain trajectory in order to land on target position.
  /// NOTE: This function will use delay before and after the jump. The delay variables can be changed in <see cref="forcejump_startdelay"/> and <see cref="forcejump_finishdelay"/>.
  /// </summary>
  /// <param name="target_pos">The target position of the </param>
  /// <returns></returns>
  public IEnumerator ForceJump(Vector3 target_pos){
    /* modified formulas from trajectory calculation:
      t^2 = (dy*2)/g
      t = ((dy*2)/g)^(1/2)

      vy = g*t(up)
      vx = x/t(total)

      Terms:
        bt: bottom to top
        tb: top to bottom
    */

    float _y_total_exaggeration = Mathf.Max(target_pos.y, transform.position.y) + Mathf.Abs(forcejump_y_exaggeration);
    float _y_bt = _y_total_exaggeration - transform.position.y;
    float _y_tb = _y_total_exaggeration - target_pos.y;

    float _g_acc = Mathf.Abs(_object_rigidbody.gravityScale * Physics2D.gravity.y);
    float _t_bt = Mathf.Pow((_y_bt*2)/_g_acc, (float)1/2);
    float _t_tb = Mathf.Pow((_y_tb*2)/_g_acc, (float)1/2);

    float _vel_y = _g_acc*_t_bt;
    float _vel_x = (target_pos.x-transform.position.x)/(_t_bt+_t_tb);

    _forcejump_flag = true;
    _object_rigidbody.velocity = Vector2.zero;
    yield return new WaitForSeconds(forcejump_startdelay);
    
    _object_collider.enabled = false;
    _object_rigidbody.velocity = new Vector2(_vel_x, _vel_y);
    yield return new WaitForSeconds(_t_bt);

    _object_collider.enabled = true;
    yield return new WaitForSeconds(_t_tb);

    yield return new WaitForSeconds(forcejump_finishdelay);

    _forcejump_flag = false;
  }


  /// <summary>
  /// Function to catch event for when an object entered in <b>GroundCheck</b> body collision (trigger).
  /// </summary>
  /// <param name="collider">The entered object</param>
  public void OnGroundCheck_Enter(Collider2D collider){
    DEBUGModeUtils.Log(string.Format("ground check enter {0} {1} id {2} count {3}", collider.gameObject.layer, (int)_OneWayColliderMask, collider.gameObject.GetInstanceID(), _ground_collider_set.Count));
    if(collider.gameObject.layer == _OneWayColliderMask)
      _one_way_ground_collider_set.Add(collider.gameObject.GetInstanceID());

    _ground_collider_set.Add(collider.gameObject.GetInstanceID());
    DEBUGModeUtils.Log(string.Format("ground check enter count {0}", _ground_collider_set.Count));
    
    _is_touching_ground = true;
    
    if(!_set_ignore_one_way_flag)
      _object_rigidbody.excludeLayers &= ~(1 << _OneWayColliderMask);

    if(_TargetAnimatorMovement != null){
      _TargetAnimatorMovement.SetBool("is_on_ground", true);
    }
  }

  /// <summary>
  /// Function to catch event for when an object exited in <b>GroundCheck</b> body collision (trigger).
  /// </summary>
  /// <param name="collider"></param>
  public void OnGroundCheck_Exit(Collider2D collider){
    DEBUGModeUtils.Log(string.Format("ground check exit id {0}", collider.gameObject.GetInstanceID()));
    if(_ground_collider_set.Contains(collider.gameObject.GetInstanceID()))
      _ground_collider_set.Remove(collider.gameObject.GetInstanceID());

    if(_one_way_ground_collider_set.Contains(collider.gameObject.GetInstanceID()))
      _one_way_ground_collider_set.Remove(collider.gameObject.GetInstanceID());

    if(_ground_collider_set.Count <= 0){
      _is_touching_ground = false;

      if(_TargetAnimatorMovement != null){
        _TargetAnimatorMovement.SetBool("is_on_ground", false);
      }
    }
  }

  
  /// <summary>
  /// Function to catch event for when an object entered in <b>WallHugLeft</b> body collision (trigger).
  /// </summary>
  /// <param name="collider">The entered object</param>
  public void OnHugWall_Left_Enter(Collider2D collider){
    _wallhug_left_collider_set.Add(collider.gameObject.GetInstanceID());
    _is_wallhug_left = true;

    if(_TargetAnimatorMovement != null){
      _TargetAnimatorMovement.SetBool("is_wallhugging", true);
    }
  }

  /// <summary>
  /// Function to catch event for when an object exited in <b>WallHugLeft</b> body collision (trigger).
  /// </summary>
  /// <param name="collider">The exited object</param>
  public void OnHugWall_Left_Exit(Collider2D collider){
    if(_wallhug_left_collider_set.Contains(collider.gameObject.GetInstanceID()))
      _wallhug_left_collider_set.Remove(collider.gameObject.GetInstanceID());

    if(_wallhug_left_collider_set.Count <= 0){
      _is_wallhug_left = false;
      
      if(_TargetAnimatorMovement != null){
        _TargetAnimatorMovement.SetBool("is_wallhugging", false);
      }
    }
  }


  /// <summary>
  /// Function to catch event for when an object entered in <b>WallHugRight</b> body collision (trigger).
  /// </summary>
  /// <param name="collider">The entered object</param>
  public void OnHugWall_Right_Enter(Collider2D collider){
    _wallhug_right_collider_set.Add(collider.gameObject.GetInstanceID());
    _is_wallhug_right = true;

    if(_TargetAnimatorMovement != null){
      _TargetAnimatorMovement.SetBool("is_wallhugging", true);
    }
  }

  /// <summary>
  /// Function to catch event for when an object exited in <b>WallHugRight</b> body collision (trigger).
  /// </summary>
  /// <param name="collider">The exited object</param>
  public void OnHugWall_Right_Exit(Collider2D collider){
    if(_wallhug_right_collider_set.Contains(collider.gameObject.GetInstanceID()))
      _wallhug_right_collider_set.Remove(collider.gameObject.GetInstanceID());

    if(_wallhug_right_collider_set.Count <= 0){
      _is_wallhug_right = false;

      if(_TargetAnimatorMovement != null){
        _TargetAnimatorMovement.SetBool("is_wallhugging", false);
      }
    }
  }


  /// <summary>
  /// Function to catch event for when this object collided with another object.
  /// </summary>
  /// <param name="collision">The colliding object</param>
  public void OnCollisionEnter2D(Collision2D collision){
    DEBUGModeUtils.Log(string.Format("enter collider {0}", collision.gameObject.layer));
    if(collision.gameObject.layer == _OneWayColliderMask)
      _one_way_collider_set.Add(collision.gameObject.GetInstanceID());
  }

  /// <summary>
  /// Function to catch event for when previously colliding object are not colliding with this object.
  /// </summary>
  /// <param name="collision">The colliding object</param>
  public void OnCollisionExit2D(Collision2D collision){
    DEBUGModeUtils.Log(string.Format("exit collider {0}", collision.gameObject.layer));
    if(_one_way_collider_set.Contains(collision.gameObject.GetInstanceID())){
      _one_way_collider_set.Remove(collision.gameObject.GetInstanceID());
    }
  }
}