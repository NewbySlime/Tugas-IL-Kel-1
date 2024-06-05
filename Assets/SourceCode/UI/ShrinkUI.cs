using System;
using UnityEngine;


public class ShrinkUI: TimingBaseUI{
  [SerializeField]
  private GameObject _DialoguePivot;

  [SerializeField]
  private bool _ShrinkOnStart = true;


  private float _target_value = 0;
  private float _start_value = 1;

  private bool _last_shrink_state;

  private Vector2 _pivot_base_scale;

  [HideInInspector]
  public bool DoShrink = true;


  protected override void _on_timer_started(){
    _start_value = DoShrink? 1: 0;
    _target_value = DoShrink? 0: 1;
  }

  protected override void _on_timer_update(){
    float _val = Mathf.SmoothStep(_start_value, _target_value, __Progress);
    _DialoguePivot.transform.localScale = _pivot_base_scale * _val;
  }

  protected override void _on_timer_finished(){
    _DialoguePivot.transform.localScale = _pivot_base_scale * _target_value;
  }


  public void Start(){
    _pivot_base_scale = _DialoguePivot.transform.localScale;

    _last_shrink_state = !_ShrinkOnStart;
    DoShrink = _ShrinkOnStart;

    StartTimerAsync(true);
  }
}