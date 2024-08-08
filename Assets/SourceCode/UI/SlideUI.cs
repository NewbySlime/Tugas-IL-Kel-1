using UnityEngine;
using System;
using System.Reflection;


[RequireComponent(typeof(RectTransform))]
/// <summary>
/// Extended <see cref="TimingBaseUI"/> UI effect for giving slide animation to the target <b>RectTransform</b>.
/// </summary>
public class SlideUI: TimingBaseUI{
  [SerializeField]
  // Position for offsetting the position of the slide effect ("hiding" flag) from the initial position ("showing" flag).
  private Vector3 _SlideAnimationStartOffset;
  [SerializeField]
  private RectTransform _TargetSlideAnimation;

  [SerializeField]
  private bool _ShowOnStart = false;

  private Vector3 _slide_pos_start;
  private float _target_show_value = 0;

  [HideInInspector]
  /// <summary>
  /// Flag to show or hide the UI element on the next effect trigger.
  /// NOTE:
  /// - <b>Showing</b> means that the position will be in the initial position (still intact to the screen).
  /// - <b>Hiding</b> means that the position will be in the offset from the initial position.
  /// </summary>
  public bool ShowAnimation;


  protected override void _on_timer_started(){
    _target_show_value = ShowAnimation? 0: 1;

    float _max_dist = _SlideAnimationStartOffset.magnitude;
    Vector3 _target_show_pos = ShowAnimation? _slide_pos_start: (_slide_pos_start+_SlideAnimationStartOffset);

    float _current_dist = ((Vector2)_target_show_pos-_TargetSlideAnimation.anchoredPosition).magnitude;

    __Timing = _current_dist/_max_dist*__BaseTiming;
  }

  protected override void _on_timer_update(){
    float _current_value = Mathf.SmoothStep(1, 0, __Progress);
    float _val = Math.Abs(_current_value - _target_show_value);

    _TargetSlideAnimation.anchoredPosition = Vector3.Lerp(_slide_pos_start, _slide_pos_start + _SlideAnimationStartOffset, _val);
  }

  protected override void _on_timer_finished(){
    _TargetSlideAnimation.anchoredPosition = Vector3.Lerp(_slide_pos_start, _slide_pos_start + _SlideAnimationStartOffset, _target_show_value);
  }


  public void Start(){
    if(_TargetSlideAnimation == null){
      Debug.LogError("Target Slide Animation Rect is not assigned.");
      throw new MissingComponentException();
    }

    _slide_pos_start = _TargetSlideAnimation.anchoredPosition;
    ShowAnimation = _ShowOnStart;

    StartTimerAsync(true);
  }
}