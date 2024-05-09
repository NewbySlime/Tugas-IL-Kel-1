using UnityEngine;


public class HealthComponent: MonoBehaviour{
  public delegate void OnHealthChanged(int new_health);
  public event OnHealthChanged OnHealthChangedEvent;

  public delegate void OnDead();
  public event OnDead OnDeadEvent;


  public struct HealthContext{
    public int health_point;
  }


  [SerializeField]
  private int MaxHealth;

  private int _current_health;


  private void _check_health(){
    if(_current_health <= 0)
      OnDeadEvent?.Invoke();
    else
      OnHealthChangedEvent?.Invoke(_current_health);
  }
  

  public void Start(){
    _current_health = MaxHealth;
  }


  public void SetHealth(HealthContext context){
    _current_health = context.health_point;
    _check_health();
  }


  public void DoDamage(DamagerComponent.DamageData damage_data){
    _current_health -= (int)damage_data.damage_points;
    _check_health();
  }
}