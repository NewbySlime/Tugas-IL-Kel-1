using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using JetBrains.Annotations;


public class TimingBaseUI: MonoBehaviour{
  [SerializeField]
  private float _Timing;
  protected float __Timing{get => _Timing;}

  private float _Progress = 1f;
  // always between 0 - 1
  protected float __Progress{get => _Progress;}


  private bool _timer_finished = true;
  private Coroutine _timer_coroutine = null;

  private float _last_timer = 0;

  protected bool _AllowSmoothCancelTiming = true;

  protected virtual void _on_timer_start_call(){}

  protected virtual void _on_timer_started(){}
  protected virtual void _on_timer_update(){}
  protected virtual void _on_timer_finished(){}

  private IEnumerator _StartTimer(bool skip_timing = false){
    if(!_timer_finished)
      yield break;

    _timer_finished = false;
    _on_timer_started();

    if(!skip_timing){
      float _current_timer = _Timing;
      Debug.Log(string.Format("current timing timer {0}", _current_timer));
      if(_AllowSmoothCancelTiming && _current_timer > 0)
        _current_timer = _Timing - _last_timer;

      Debug.Log(string.Format("next current timing timer {0}", _current_timer));
      while(_current_timer > 0){
        yield return new WaitForNextFrameUnit();

        Debug.Log(string.Format("timer {0}", _current_timer));
        _current_timer -= Time.unscaledDeltaTime;
        _last_timer = _current_timer;

        _Progress = Mathf.Abs(1 - (_current_timer/_Timing));

        _on_timer_update();
      }
    }

    _timer_finished = true;
    _on_timer_finished();
  }


  public void StartTimerAsync(bool skip_timing = false){
    _on_timer_start_call();
    
    _timer_coroutine = StartCoroutine(_StartTimer(skip_timing));
  }

  public void SkipTimer(){
    if(TimerFinished() || _timer_coroutine == null)
      return;

    StopCoroutine(_timer_coroutine);

    _timer_finished = true;
    _on_timer_finished();
  }

  public bool TimerFinished(){
    return _timer_finished;
  }


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

  public static IEnumerator StartAllTimer(Component target, bool skip_timing = false){
    yield return StartAllTimer(target.gameObject, skip_timing);
  }


  public static void StartAsyncAllTimer(GameObject target, bool skip_timing = false){
    TimingBaseUI[] _timer_list = target.GetComponents<TimingBaseUI>();
    foreach(TimingBaseUI _timer in _timer_list)
      _timer.StartTimerAsync(skip_timing);
  }

  public static void StartAsyncAllTimer(Component target, bool skip_timing = false){
    StartAsyncAllTimer(target.gameObject, skip_timing);
  }


  public static void SkipAllTimer(GameObject target, bool skip_timing = false){
    TimingBaseUI[] _timer_list = target.GetComponents<TimingBaseUI>();
    foreach(TimingBaseUI _timer in _timer_list)
      _timer.SkipTimer();
  }

  public static void SkipAllTimer(Component target, bool skip_timing = false){
    SkipAllTimer(target.gameObject, skip_timing);
  }


  public static bool AllTimerFinished(GameObject target){
    TimingBaseUI[] _timer_list = target.GetComponents<TimingBaseUI>();
    foreach(TimingBaseUI _timer in _timer_list){
      if(!_timer.TimerFinished())
        return false;
    }

    return true;
  }

  public static bool AllTimerFinished(Component target){
    return AllTimerFinished(target.gameObject);
  }
}