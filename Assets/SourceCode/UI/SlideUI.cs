using UnityEngine;
using System;
using System.Reflection;


[RequireComponent(typeof(RectTransform))]
public class SlideUI: TimingBaseUI{
  [SerializeField]
  private Vector3 _SlideAnimationStartOffset;
  [SerializeField]
  private RectTransform _TargetSlideAnimation;

  [SerializeField]
  private bool _ShowOnStart = false;

  private Vector3 _slide_pos_start;
  private float _target_show_value = 0;

  private bool _last_show_state;

  [HideInInspector]
  public bool ShowAnimation;


  protected override void _on_timer_started(){
    if(_last_show_state == ShowAnimation)
      return;

    _target_show_value = ShowAnimation? 0: 1;
  }

  protected override void _on_timer_update(){
    if(_last_show_state == ShowAnimation)
      return;

    float _current_value = Mathf.SmoothStep(0.5f, 0, __Progress);
    float _val = Math.Abs(_current_value - _target_show_value);

    _TargetSlideAnimation.anchoredPosition = Vector3.Lerp(_slide_pos_start, _slide_pos_start + _SlideAnimationStartOffset, _val);
  }

  protected override void _on_timer_finished(){
    _TargetSlideAnimation.anchoredPosition = Vector3.Lerp(_slide_pos_start, _slide_pos_start + _SlideAnimationStartOffset, _target_show_value);

    _last_show_state = ShowAnimation;
  }


  public void Start(){
    if(_TargetSlideAnimation == null){
      Debug.LogError("Target Slide Animation Rect is not assigned.");
      throw new MissingComponentException();
    }

    _slide_pos_start = _TargetSlideAnimation.anchoredPosition;

    _last_show_state = !_ShowOnStart;
    ShowAnimation = _ShowOnStart;

    StartTimerAsync(true);
  }
}