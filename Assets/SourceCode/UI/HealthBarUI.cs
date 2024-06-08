using System;
using System.Collections;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.UI;


public class HealthBarUI: MonoBehaviour{
  [SerializeField]
  private GameObject _HealthBarMaterialReference;

  [SerializeField]
  private float _HealthBarSmoothTime;

  [SerializeField]
  private Color _FGColor;
  [SerializeField]
  private Color _BGColor;

  [SerializeField]
  private float _HealthBarDelayChange;


  private GameHandler _game_handler;

  private HealthComponent _health_component = null;

  private IMaterialReference _material_reference;

  private float _bg_val;

  private float _fg_val;
  private float _target_fg_val;

  private float _bg_update_timer = -1;

  private float _fg_smooth_speed_ref;
  private float _bg_smooth_speed_ref;


  private void _update_bar_ui(){
    Material _current_material = _material_reference.GetMaterial();
    _current_material.SetFloat("_ProgressFG", _fg_val);
    _current_material.SetFloat("_ProgressBG", _bg_val);

    _current_material.SetColor("_FGMultiply", _FGColor);
    _current_material.SetColor("_BGMultiply", _BGColor);
  }


  private void _update_bar(bool skip_animation = false){
    int _current_health = _health_component.GetHealth();
    int _max_health = _health_component.MaxHealth;
    Debug.Log(string.Format("health {0}", _current_health));
    
    _target_fg_val = _current_health > 0? (float)_current_health/_max_health: 0;
    if(skip_animation){
      _fg_val = _target_fg_val;
      _bg_val = _target_fg_val;

      _update_bar_ui();
    }

    _bg_update_timer = _HealthBarDelayChange;
  }

  private void _on_health_dead(){
    _health_component.OnDeadEvent -= _on_health_dead;
    _health_component.OnHealthChangedEvent -= _on_health_changed;
  }

  private void _on_health_changed(int health){
    _update_bar();
  }

  private void _on_health_set_runtime_data(){
    _update_bar(true);
  }

  private IEnumerator _start_co_func(){
    yield return null;
    yield return new WaitForEndOfFrame();

    _target_fg_val = 1;
    _fg_val = 1;
    _bg_val = 1;

    _update_bar_ui();
  }


  public void Start(){
    Debug.Log("health bar started");
    IMaterialReference[] _list_material_ref = _HealthBarMaterialReference.GetComponents<IMaterialReference>();
    if(_list_material_ref.Length <= 0){
      Debug.LogError("HealthBar material reference does not have IMaterialReference.");
      throw new MissingReferenceException();
    }
    else if(_list_material_ref.Length > 1)
      Debug.LogWarning("IMaterialReference instance more than one is not supported.");

    _material_reference = _list_material_ref[0];

    StartCoroutine(_start_co_func());
  }

  public void Update(){
    bool _update_bar = false; 
    if(Mathf.Abs(_target_fg_val-_fg_val) > 0.01){
      _fg_val = Mathf.SmoothDamp(_fg_val, _target_fg_val, ref _fg_smooth_speed_ref, _HealthBarSmoothTime);
      
      if(_bg_val < _fg_val)
        _bg_val = _fg_val;

      _update_bar = true;      
    }

    if(_bg_update_timer > 0)
      _bg_update_timer -= Time.deltaTime;
    else if(Mathf.Abs(_fg_val-_bg_val) > 0.01){
      _bg_val = Mathf.SmoothDamp(_bg_val, _fg_val, ref _bg_smooth_speed_ref, _HealthBarSmoothTime);

      _update_bar = true;
    }

    if(_update_bar)
      _update_bar_ui();
  }


  public void BindHealthComponent(HealthComponent health){
    UnbindHealthComponent();
    if(health == null)
      return;

    _health_component = health;

    _update_bar(true);
    _health_component.OnHealthChangedEvent += _on_health_changed;
    _health_component.OnRuntimeDataSetEvent += _on_health_set_runtime_data;
  }

  public void UnbindHealthComponent(){
    if(_health_component == null)
      return;

    _health_component.OnHealthChangedEvent -= _on_health_changed;
    _health_component.OnRuntimeDataSetEvent -= _on_health_set_runtime_data;
  }
}