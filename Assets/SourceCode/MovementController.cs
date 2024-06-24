using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


/// <summary>
/// Komponen untuk mekanik movement. Karena ini hanya mekanik movement yang bisa dipakai Komponen lain, bisa dipakai oleh Player, Musuh, dan lainnya.
/// Komponen ini memerlukan komponen lain:
///   - Rigidbody2D
/// 
/// Komponen ini memerlukan Child Object sebagai berikut:
///   - RigidbodyMessageRelay (GroundCheck)
///   - RigidbodyMessageRelay (WallHugRight)
///   - RigidbodyMessageRelay (WallHugLeft)
/// </summary>
public class MovementController: MonoBehaviour{
  public const string AudioID_Jump = "jump";


  [SerializeField]
  private float WalkNormalizeClamp = 0.4f;

  [SerializeField]
  private bool ClampWalkDir = true;

  [SerializeField]
  private float movement_speed = 100f;
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
  private float forcejump_y_exaggeration = 1.5f;
  
  [SerializeField]
  private float forcejump_startdelay = 0.5f;
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


  public bool ToggleWalk = false;


  [HideInInspector]
  public bool FixedLookAt = false;

  [HideInInspector]
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


  public void LookAt(Vector2 direction){
    if(_TargetAnimatorMovement == null)
      return;

    if(Mathf.Abs(direction.x) > 0.1)
      _TargetAnimatorMovement.SetFloat("speed_horizontal", direction.x < 0? -1: 1);
  }


  /// <summary>
  /// Fungsi untuk membuat bergerak secara horizontal Objek Game.
  /// </summary>
  /// <param name="walk_dir_x">Arah pergerakan dalam horizontal.</param>
  public void DoWalk(float walk_dir_x){
    _walk_dir_x = walk_dir_x;
  } 


  /// <summary>
  /// Fungsi untuk melakukan lompatan kepada Objek Game. Lompatan bisa berupa lompat saat diatas tanah, atau saat "Wall-Hugging" (Wall Jump).
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


  public void SetEnableMovement(bool enabled){
    _is_movement_enabled = enabled;
  }


  public bool IsWallHuggingLeft(){
    return _is_wallhug_left;
  }

  public bool IsWallHuggingRight(){
    return _is_wallhug_right;
  }

  public Rigidbody2D GetRigidbody(){
    return _object_rigidbody;
  }


  public void SetIgnoreOneWayCollision(bool flag){
    _set_ignore_one_way_flag = flag;

    if(flag)
      _object_rigidbody.excludeLayers |= 1 << _OneWayColliderMask;
  }


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
  /// Fungsi pengecekan ketika Player menyentuh sesuatu (dengan Collision "GroundCheck")
  /// </summary>
  /// <param name="collider">Object hasil Collision</param>
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
  /// Fungsi pengecekan ketika Collider pada Collision "GroundCheck" keluar
  /// </summary>
  /// <param name="collider">Object hasil Collision</param>
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
  /// Fungsi pengecekan ketika Player menyentuh sesuatu (dengan Collision "WallHugLeft")
  /// </summary>
  /// <param name="collider">Object hasil Collision</param>
  public void OnHugWall_Left_Enter(Collider2D collider){
    _wallhug_left_collider_set.Add(collider.gameObject.GetInstanceID());
    _is_wallhug_left = true;

    if(_TargetAnimatorMovement != null){
      _TargetAnimatorMovement.SetBool("is_wallhugging", true);
    }
  }

  /// <summary>
  /// Fungsi pengecekan ketika Collider pada Collision "WallHugLeft" keluar
  /// </summary>
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
  /// Fungsi pengecekan ketika Player menyentuh sesuatu (dengan Collision "WallHugRight")
  /// </summary>
  /// <param name="collider">Object hasil Collision</param>
  public void OnHugWall_Right_Enter(Collider2D collider){
    _wallhug_right_collider_set.Add(collider.gameObject.GetInstanceID());
    _is_wallhug_right = true;

    if(_TargetAnimatorMovement != null){
      _TargetAnimatorMovement.SetBool("is_wallhugging", true);
    }
  }

  /// <summary>
  /// Fungsi pengecekan ketika Collider pada Collision "WallHugRight" keluar
  /// </summary>
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


  public void OnCollisionEnter2D(Collision2D collision){
    DEBUGModeUtils.Log(string.Format("enter collider {0}", collision.gameObject.layer));
    if(collision.gameObject.layer == _OneWayColliderMask)
      _one_way_collider_set.Add(collision.gameObject.GetInstanceID());
  }

  public void OnCollisionExit2D(Collision2D collision){
    DEBUGModeUtils.Log(string.Format("exit collider {0}", collision.gameObject.layer));
    if(_one_way_collider_set.Contains(collision.gameObject.GetInstanceID())){
      _one_way_collider_set.Remove(collision.gameObject.GetInstanceID());
    }
  }
}