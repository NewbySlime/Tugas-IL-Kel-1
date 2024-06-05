using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class HurtUI: MonoBehaviour{
  [SerializeField]
  private Image _HurtImageUI;
  [SerializeField]
  private Material _Material;

  [SerializeField]
  private float _SmoothTime;


  private Material _current_material;

  private HealthComponent _health = null;

  private float _smooth_timer = -1;


  private void _health_damaged(int damage_points){
    _smooth_timer = _SmoothTime;
  }

  private void _update_ui_alpha(float alpha_val){
    _current_material.SetColor("_ColorMultiply", new(1,1,1, alpha_val));
  }


  public void Start(){
    _current_material = new(_Material);
    _HurtImageUI.material = _current_material;

    _update_ui_alpha(0);
  }

  public void Update(){
    if(_smooth_timer > 0){
      _smooth_timer -= Time.deltaTime;
      if(_smooth_timer <= 0)
        _smooth_timer = 0;

      _update_ui_alpha(_smooth_timer/_SmoothTime);
    }
  }


  public void BindHealthComponent(HealthComponent health){
    UnbindHealthComponent();
    if(health == null)
      return;

    health.OnDamagedEvent += _health_damaged;
  }

  public void UnbindHealthComponent(){
    if(_health == null)
      return;

    _health.OnDamagedEvent -= _health_damaged;
  }
}