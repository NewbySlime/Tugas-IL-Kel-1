using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class FadeUI: TimingBaseUI{
  [SerializeField]
  private GameObject _TargetAlphaReference;

  [SerializeField]
  private bool FadeStateOnStart = false;

  private IAlphaRendererReference _alpha_reference;

  private float _target_fade_value = 0;
  private bool _last_fade_to_cover;

  [HideInInspector]
  public bool FadeToCover = true;

  protected override void _on_timer_started(){
    _on_timer_start_call();

    _target_fade_value = FadeToCover? 1: 0;
  }

  protected override void _on_timer_update(){
    if(_last_fade_to_cover == FadeToCover)
      return;

    float _current_value = Mathf.SmoothStep(1, 0, __Progress);
    _alpha_reference.SetAlpha(Math.Abs(_current_value - _target_fade_value));
  }

  protected override void _on_timer_finished(){
    _last_fade_to_cover = FadeToCover;
    _alpha_reference.SetAlpha(_target_fade_value);
  }


  public void Start(){
    IAlphaRendererReference[] _list_references = _TargetAlphaReference.GetComponents<IAlphaRendererReference>();
    if(_list_references.Length <= 0){
      Debug.LogError("TargetAlphaReference does not have IAlphaRendererReference.");
      throw new MissingReferenceException();
    }
    else if(_list_references.Length > 1)
      Debug.LogWarning("IAlphaRendererReference instance more than one is not supported.");

    _alpha_reference = _list_references[0];

    _last_fade_to_cover = !FadeStateOnStart;
    FadeToCover = FadeStateOnStart;
    
    StartTimerAsync(true);
  }
}