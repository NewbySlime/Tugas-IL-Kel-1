using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LoadingUI: FadeUI{
  public delegate void UIFinished();
  public event UIFinished UIFinishedEvent;

  [SerializeField]
  private Slider _LoadingBar;

  [SerializeField]
  private float _LoadingSmoothTiming;


  private float _current_speed = 0;
  private float _current_value = 0;
  private float _target_value = 1;


  public bool UIAnimationFinished{get; private set;} = false;



  private AsyncOperation _async_op;


  private void _operation_finished(AsyncOperation async_op){
    _target_value = 1.1f;
  }


  public new void Start(){
    base.Start();
    if(_LoadingBar == null)
      Debug.LogWarning("No Loading Bar UI Attached.");
  }

  
  public void Update(){
    if(_async_op != null && _current_value < 1){
      if(_target_value < 1)
        _target_value = _async_op.progress;

      _current_value = Mathf.SmoothDamp(_current_value, _target_value, ref _current_speed, _LoadingSmoothTiming, float.PositiveInfinity, Time.unscaledDeltaTime);
      SetLoadingProgress(_current_value);

      if(_current_value >= 1){
        UIAnimationFinished = true;
        UIFinishedEvent?.Invoke();
      }
    }
  }


  public void SetLoadingProgress(float progress){
    _LoadingBar.value = (_LoadingBar.maxValue - _LoadingBar.minValue) * progress + _LoadingBar.minValue;
  }

  public float GetLoadingProgress(){
    return (_LoadingBar.value - _LoadingBar.minValue) / (_LoadingBar.maxValue - _LoadingBar.minValue); 
  }


  public void BindAsyncOperation(AsyncOperation async_op = null){
    if(async_op != null){
      UIAnimationFinished = false;

      _LoadingBar.value = _LoadingBar.minValue;
      _current_speed = 0;
      _current_value = 0;
      _target_value = 0;

      async_op.completed += _operation_finished;
    }
    else if(_async_op != null){
      _async_op.completed -= _operation_finished;
    }

    _async_op = async_op;
  }

  public void UnbindAsyncOperation(){
    BindAsyncOperation();
  }
}