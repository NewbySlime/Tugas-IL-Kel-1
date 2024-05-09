using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.TextCore.Text;


public class DamagerComponent: MonoBehaviour{
  [Serializable]
  public struct DamageData{
    public uint damage_points;
  }

  [Serializable]
  public struct DamagerContext{
    public enum OnCollisionAction{
      Nothing,
      EraseOnCollision,
      BounceOnCollision
    }

    public enum ProjectileType{
      Raycast,
      FloatingProjectile,
      NormalProjectile
    }

    [Serializable]
    public struct ItemDeplete{
      public string ItemID;
      public uint count;
    }


    public ProjectileType _ProjectileType;

    public OnCollisionAction OnCollisionEffect; 
    public bool DamageBasedOnSpeed;

    public float ProjectileWeight;
    public float ProjectileSize;

    public Vector3 ProjectileStartDirection;
    public float ProjectileSpeed;

    public float DamagerLifetime;


    public List<ItemDeplete> ListDepleteItem;
  }
  

  private DamageData _damage_data = new DamageData{
    damage_points = 1
  };

  private DamagerContext _damager_context = new DamagerContext{
    _ProjectileType = DamagerContext.ProjectileType.FloatingProjectile,
    OnCollisionEffect = DamagerContext.OnCollisionAction.Nothing,
    DamageBasedOnSpeed = false,
    DamagerLifetime = float.PositiveInfinity
  };


  private Rigidbody2D _rigidbody_projectile;
  private CircleCollider2D _projectile_hitbox;


  private Vector3 _direction;

  private float _object_lifetime = -1;

  private bool _is_triggered = false;


  private void _check_object(GameObject target_object){
    HealthComponent _health = target_object.GetComponent<HealthComponent>();
    if(_health == null)
      return;

    _health.DoDamage(_damage_data);
  }


  public void Start(){
    _rigidbody_projectile = GetComponent<Rigidbody2D>();
    if(_rigidbody_projectile == null){
      Debug.LogError("Cannot get Projectile's Rigidbody.");
      throw new UnityEngine.MissingComponentException();
    }

    _projectile_hitbox = GetComponent<CircleCollider2D>();
    if(_projectile_hitbox == null){
      Debug.LogError("Cannot get Projectile's Hitbox.");
      throw new UnityEngine.MissingComponentException();
    }
  }

  public void FixedUpdate(){
    if(!_is_triggered)
      return;

    switch(_damager_context._ProjectileType){
      case DamagerContext.ProjectileType.FloatingProjectile:{
        Vector2 _direction2 = _direction;
        _rigidbody_projectile.MovePosition(_direction2 * _damager_context.ProjectileSpeed * Time.fixedDeltaTime);
      }break;
    }


    if(float.IsFinite(_object_lifetime) && _object_lifetime > 0){
      _object_lifetime -= Time.fixedDeltaTime;
      if(_object_lifetime <= 0)
        Destroy(gameObject);
    }
  }


  public void SetDamageData(DamageData damage_data){
    _damage_data = damage_data;
  }

  public void SetDamagerContext(DamagerContext context){
    _damager_context = context;

    _projectile_hitbox.radius = _damager_context.ProjectileSize;
  }


  public void TriggerDamager(){
    _is_triggered = true;
    _direction = _damager_context.ProjectileStartDirection;

    switch(_damager_context._ProjectileType){
      case DamagerContext.ProjectileType.NormalProjectile:{
        _rigidbody_projectile.isKinematic = false;
        _rigidbody_projectile.mass = _damager_context.ProjectileWeight;
        _rigidbody_projectile.velocity = _direction * _damager_context.ProjectileSpeed;
      }break;

      case DamagerContext.ProjectileType.FloatingProjectile:{
        _rigidbody_projectile.isKinematic = true;
      }break;

      case DamagerContext.ProjectileType.Raycast:{
        RaycastHit _raycast_result;
        Physics.Raycast(
          gameObject.transform.position,
          _direction,
          out _raycast_result,
          _damager_context.ProjectileSpeed,
          gameObject.layer
        );

        _check_object(_raycast_result.collider.gameObject);
        Destroy(gameObject);
      }break;
    }
  }



  public void OnCollisionEnter2D(Collision2D collision){
    _check_object(collision.gameObject);

    switch(_damager_context.OnCollisionEffect){
      case DamagerContext.OnCollisionAction.EraseOnCollision:{
        Destroy(gameObject);
      }break;

      case DamagerContext.OnCollisionAction.BounceOnCollision:{
        Vector2 _bounce_direction = Vector2.zero;
        Vector2 _bounce_speed = Vector2.zero;

        int _contacts_count = collision.contactCount;
        for(int i = 0; i < _contacts_count; i++){
          ContactPoint2D _contact_point = collision.GetContact(i);
          _bounce_direction += _contact_point.normal;
          _bounce_speed += _contact_point.relativeVelocity;
        }

        _bounce_direction *= -1;
        switch(_damager_context._ProjectileType){
          case DamagerContext.ProjectileType.NormalProjectile:{
            _rigidbody_projectile.velocity += _bounce_direction.normalized * _bounce_speed.magnitude;
          }break;

          case DamagerContext.ProjectileType.FloatingProjectile:{
            _direction = _bounce_direction.normalized;
          }break;
        }
      }break;
    }
  }
}