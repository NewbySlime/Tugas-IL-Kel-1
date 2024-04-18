using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


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

  [SerializeField]
  private float movement_speed = 100f;

  [SerializeField]
  private float movement_speed_midair = 10f;

  [SerializeField]
  private float jump_force = 100f;

  [SerializeField]
  private float walljump_force = 80f;

  [SerializeField]
  private Vector2 walljump_direction = new Vector2(0.5f, 0.5f);

  private Rigidbody2D _object_rigidbody;

  private RigidbodyMessageRelay _ground_check_relay;

  private RigidbodyMessageRelay _wh_right_check_relay;
  private RigidbodyMessageRelay _wh_left_check_relay;


  private HashSet<Rigidbody2D> _ground_collider_set = new HashSet<Rigidbody2D>();
  private bool _is_touching_ground;

  private HashSet<Rigidbody2D> _wallhug_left_collider_set = new HashSet<Rigidbody2D>();
  private bool _is_wallhug_left;
  
  private HashSet<Rigidbody2D> _wallhug_right_collider_set = new HashSet<Rigidbody2D>();
  private bool _is_wallhug_right;


  private float _walk_dir_x = 0;


  public void Start(){
    _object_rigidbody = gameObject.GetComponent<Rigidbody2D>();
    
    GameObject _ground_check = transform.Find("GroundCheck").gameObject;
    _ground_check_relay = _ground_check.GetComponent<RigidbodyMessageRelay>();
    _ground_check_relay.OnTriggerEntered2DEvent += OnGroundCheck_Enter;
    _ground_check_relay.OnTriggerExited2DEvent += OnGroundCheck_Exit;

    GameObject _wallhug_right_check = transform.Find("WallHugRight").gameObject;
    _wh_right_check_relay = _wallhug_right_check.GetComponent<RigidbodyMessageRelay>();
    _wh_right_check_relay.OnTriggerEntered2DEvent += OnHugWall_Right_Enter;
    _wh_right_check_relay.OnTriggerExited2DEvent += OnHugWall_Right_Exit;

    GameObject _wallhug_left_check = transform.Find("WallHugLeft").gameObject;
    _wh_left_check_relay = _wallhug_left_check.GetComponent<RigidbodyMessageRelay>();
    _wh_left_check_relay.OnTriggerEntered2DEvent += OnHugWall_Left_Enter;
    _wh_left_check_relay.OnTriggerExited2DEvent += OnHugWall_Left_Exit;
  }

  public void FixedUpdate(){
    if(_is_touching_ground)
      _object_rigidbody.velocity = new Vector2(_walk_dir_x * movement_speed, _object_rigidbody.velocity.y);
    else{
      Vector2 _result_vel = _object_rigidbody.velocity;
      _result_vel.x += _walk_dir_x * Time.fixedDeltaTime * movement_speed_midair;
      if(MathF.Abs(_result_vel.x) <= movement_speed)
        _object_rigidbody.velocity = _result_vel;
    }
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
    if(_is_touching_ground){
      _is_touching_ground = false;
      _object_rigidbody.AddForce(Vector3.up * jump_force);
    }
    else if(_is_wallhug_right){
      _is_wallhug_right = false;

      Vector2 _inv_dir = new Vector2(-walljump_direction.x, walljump_direction.y);
      _object_rigidbody.velocity = Vector2.zero;
      _object_rigidbody.AddForce(_inv_dir.normalized * walljump_force);
    }
    else if(_is_wallhug_left){
      _is_wallhug_left = false;
      
      _object_rigidbody.velocity = Vector2.zero;
      _object_rigidbody.AddForce(walljump_direction.normalized * walljump_force);
    }
  }


  /// <summary>
  /// Fungsi pengecekan ketika Player menyentuh sesuatu (dengan Collision "GroundCheck")
  /// </summary>
  /// <param name="collider">Object hasil Collision</param>
  public void OnGroundCheck_Enter(Collider2D collider){
    Debug.Log("ground check enter");
    _ground_collider_set.Add(collider.attachedRigidbody);
    _is_touching_ground = true;
  }

  /// <summary>
  /// Fungsi pengecekan ketika Collider pada Collision "GroundCheck" keluar
  /// </summary>
  /// <param name="collider">Object hasil Collisiondddd</param>
  public void OnGroundCheck_Exit(Collider2D collider){
    if(_ground_collider_set.Contains(collider.attachedRigidbody))
      _ground_collider_set.Remove(collider.attachedRigidbody);

    if(_ground_collider_set.Count <= 0)
      _is_touching_ground = false;
  }

  
  /// <summary>
  /// Fungsi pengecekan ketika Player menyentuh sesuatu (dengan Collision "WallHugLeft")
  /// </summary>
  /// <param name="collider">Object hasil Collision</param>
  public void OnHugWall_Left_Enter(Collider2D collider){
    _wallhug_left_collider_set.Add(collider.attachedRigidbody);
    _is_wallhug_left = true;
  }

  /// <summary>
  /// Fungsi pengecekan ketika Collider pada Collision "WallHugLeft" keluar
  /// </summary>
  public void OnHugWall_Left_Exit(Collider2D collider){
    if(_wallhug_left_collider_set.Contains(collider.attachedRigidbody))
      _wallhug_left_collider_set.Remove(collider.attachedRigidbody);

    if(_wallhug_left_collider_set.Count <= 0)
      _is_wallhug_left = false;
  }


  /// <summary>
  /// Fungsi pengecekan ketika Player menyentuh sesuatu (dengan Collision "WallHugRight")
  /// </summary>
  /// <param name="collider">Object hasil Collision</param>
  public void OnHugWall_Right_Enter(Collider2D collider){
    _wallhug_right_collider_set.Add(collider.attachedRigidbody);
    _is_wallhug_right = true;
  }

  /// <summary>
  /// Fungsi pengecekan ketika Collider pada Collision "WallHugRight" keluar
  /// </summary>
  public void OnHugWall_Right_Exit(Collider2D collider){
    if(_wallhug_right_collider_set.Contains(collider.attachedRigidbody))
      _wallhug_right_collider_set.Remove(collider.attachedRigidbody);

    if(_wallhug_right_collider_set.Count <= 0)
      _is_wallhug_right = false;
  }
}