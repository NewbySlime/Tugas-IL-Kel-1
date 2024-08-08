using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using JetBrains.Annotations;


/// <summary>
/// Base UI effect class that will handle the timing of the effect, which the inherited class can directly use the current effect timing.
/// </summary>
public class TimingBaseUI: MonoBehaviour{
  [SerializeField]
  private float _Timing;
  
  /// <summary>
  /// The initial time of the timing (immutable).
  /// </summary>
  protected float __BaseTiming{get => _Timing;}

  /// <summary>
  /// The time used for the effect.
  /// This timing can be modified by inheriting class in context of <see cref="_on_timer_started"/> virtual function is called.
  /// </summary>
  protected float __Timing;

  private float _Progress = 1f;

  /// <summary>
  /// The progress of current effect timer.
  /// This value is normalized based on the timing. (CurrentTimer/__BaseTiming)
  /// </summary>
  protected float __Progress{get => _Progress;}


  private bool _timer_finished = true;
  private Coroutine _timer_coroutine = null;

  public bool UseScaledTime = false;


  /// <summary>
  /// Virtual class for when the effect is starting.
  /// </summary>
  protected virtual void _on_timer_started(){}

  /// <summary>
  /// Virtual class called for each Unity's update whilst the effect timer is running. 
  /// </summary>
  protected virtual void _on_timer_update(){}

  /// <summary>
  /// Virtual class for when the effect is finishing.
  /// </summary>
  protected virtual void _on_timer_finished(){}


  private IEnumerator _StartTimer(bool skip_timing = false){
    if(!_timer_finished)
      yield break;

    _timer_finished = false;
    __Timing = _Timing;
    _on_timer_started();

    if(!skip_timing){
      float _current_timer = __Timing;
      while(_current_timer > 0){
        yield return new WaitForNextFrameUnit();

        _current_timer -= UseScaledTime? Time.deltaTime: Time.unscaledDeltaTime;
        _Progress = Mathf.Abs(1 - (_current_timer/__Timing));

        _on_timer_update();
      }
    }

    _timer_finished = true;
    _on_timer_finished();
    _timer_coroutine = null;
  }


  /// <summary>
  /// Function to asynchronously (using coroutine) start and handle the timer.
  /// </summary>
  /// <param name="skip_timing">Should the timer be skipped (triggers the effect to be instantly finished)</param>
  public void StartTimerAsync(bool skip_timing = false){
    SkipTimer();
    _timer_coroutine = StartCoroutine(_StartTimer(skip_timing));
  }

  /// <summary>
  /// Function to skip the timer so the effect will be instantly finished.
  /// </summary>
  public void SkipTimer(){
    if(TimerFinished() || _timer_coroutine == null)
      return;

    StopCoroutine(_timer_coroutine);

    //_on_timer_finished();
    _timer_finished = true;
    _timer_coroutine = null;
  }

  /// <summary>
  /// To check if the current effect is currently running.
  /// </summary>
  /// <returns>The finished flag</returns>
  public bool TimerFinished(){
    return _timer_finished;
  }


  /// <summary>
  /// Function to trigger all component that has <see cref="TimingBaseUI"/> to trigger all the effects at once.
  /// </summary>
  /// <param name="target">The target <b>GameObject</b></param>
  /// <param name="skip_timing">Should the timer be skipped</param>
  /// <returns>Coroutine helper object</returns>
  public static IEnumerator StartAllTimer(GameObject target, bool skip_timing = false){
    TimingBaseUI[] _timer_list = target.GetComponents<TimingBaseUI>();
    foreach(TimingBaseUI _timer in _timer_list)
      _timer.StartTimerAsync(skip_timing);

    yield return new WaitUntil(() => {
      foreach(TimingBaseUI _timer in _timer_list){
        if(!_timer.TimerFinished())
          return false;
      }

      return true;
    });
  }

  /// <inheritdoc cref="StartAllTimer"/>
  public static IEnumerator StartAllTimer(Component target, bool skip_timing = false){
    yield return StartAllTimer(target.gameObject, skip_timing);
  }


  /// <summary>
  /// <inheritdoc cref="StartAllTimer"/>
  /// This function starts all effect asynchronously (coroutine) by using <see cref="StartTimerAsync"/>.
  /// </summary>
  /// <param name="target">The target <b>GameObject</b></param>
  /// <param name="skip_timing">Shoudl the timer be skipped</param>
  public static void StartAsyncAllTimer(GameObject target, bool skip_timing = false){
    TimingBaseUI[] _timer_list = target.GetComponents<TimingBaseUI>();
    foreach(TimingBaseUI _timer in _timer_list)
      _timer.StartTimerAsync(skip_timing);
  }

  /// <inheritdoc cref="StartAsyncAllTimer"/>
  public static void StartAsyncAllTimer(Component target, bool skip_timing = false){
    StartAsyncAllTimer(target.gameObject, skip_timing);
  }


  /// <summary>
  /// Function to skip all effects (instantly finishing) that are currently running.
  /// </summary>
  /// <param name="target">The target <b>GameObject</b></param>
  /// <param name="skip_timing"></param>
  public static void SkipAllTimer(GameObject target){
    TimingBaseUI[] _timer_list = target.GetComponents<TimingBaseUI>();
    foreach(TimingBaseUI _timer in _timer_list)
      _timer.SkipTimer();
  }

  /// <inheritdoc cref="SkipAllTimer"/>
  public static void SkipAllTimer(Component target){
    SkipAllTimer(target.gameObject);
  }


  /// <summary>
  /// Function to check if all the effects on the target object are finished.
  /// </summary>
  /// <param name="target">The target <b>GameObject</b></param>
  /// <returns>Flag if all effects in the object are finished</returns>
  public static bool AllTimerFinished(GameObject target){
    TimingBaseUI[] _timer_list = target.GetComponents<TimingBaseUI>();
    foreach(TimingBaseUI _timer in _timer_list){
      if(!_timer.TimerFinished())
        return false;
    }

    return true;
  }

  /// <inheritdoc cref="AllTimerFinished"/>
  public static bool AllTimerFinished(Component target){
    return AllTimerFinished(target.gameObject);
  }
}