using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


/// <summary>
/// Base class used for blocking screen with a UI element attached to this object. The UI also handles showing/hiding animation.
/// This class uses timer from <see cref="TimingBaseUI"/>.
///
/// This class uses external component(s);
/// - <b>GameObject</b> with <see cref="IAlphaRendererReference"/> to manipulate the alpha with. 
/// 
/// </summary>
public class FadeUI: TimingBaseUI, IObjectInitialized{
  [SerializeField]
  private GameObject _TargetAlphaReference;

  [SerializeField]
  private bool FadeStateOnStart = false;

  private IAlphaRendererReference _alpha_reference;

  private float _target_fade_value = 0;

  [HideInInspector]
  /// <summary>
  /// Should this object be shown or hidden in the next animation trigger.
  /// </summary>
  public bool FadeToCover = true;

  /// <summary>
  /// Is the object initialized or not yet?
  /// </summary>
  public bool IsInitialized{private set; get;} = false;

  protected override void _on_timer_started(){
    _target_fade_value = FadeToCover? 1: 0;
    __Timing = Mathf.Abs(_target_fade_value-_alpha_reference.GetAlpha()) * __BaseTiming;
  }

  protected override void _on_timer_update(){
    float _current_value = Mathf.SmoothStep(1, 0, __Progress);
    _alpha_reference.SetAlpha(Math.Abs(_current_value - _target_fade_value));
  }

  protected override void _on_timer_finished(){
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

    FadeToCover = FadeStateOnStart;
    
    StartTimerAsync(true);
  }

  public bool GetIsInitialized(){
    return IsInitialized;
  }
}