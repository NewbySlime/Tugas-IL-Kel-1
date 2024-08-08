using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


/// <summary>
/// UI Class for handling Loading animation based on the supplied <b>AsyncOperation</b> fron any source, but preferably from level loading of Unity.
/// This class uses effect from <see cref="FadeUI"/> for showing/hiding itself.
/// 
/// This class uses external component(s);
/// - <b>Slider</b> for progress bar for loading operations.
/// </summary>
public class LoadingUI: FadeUI{
  /// <summary>
  /// Event when the operation from <b>AsyncOperation</b> finished.
  /// </summary>
  public event UIFinished UIFinishedEvent;
  public delegate void UIFinished();

  [SerializeField]
  private Slider _LoadingBar;

  [SerializeField]
  private float _LoadingSmoothTiming;


  private float _current_speed = 0;
  private float _current_value = 0;
  private float _target_value = 1;


  /// <summary>
  /// If the loading animation finished or not.
  /// </summary>
  public bool UIAnimationFinished{get; private set;} = false;



  private AsyncOperation _async_op;


  // If the loading operation is finished.
  private void _operation_finished(AsyncOperation async_op){
    _target_value = 1.1f;
  }


  // Overriding the Start from base class.
  public new void Start(){
    base.Start();
    if(_LoadingBar == null)
      Debug.LogWarning("No Loading Bar UI Attached.");
  }

  
  // Handles the loading animation.
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


  /// <summary>
  /// Set loading progress value externally (not from AsyncOperation).
  /// </summary>
  /// <param name="progress">The progress value</param>
  public void SetLoadingProgress(float progress){
    _LoadingBar.value = (_LoadingBar.maxValue - _LoadingBar.minValue) * progress + _LoadingBar.minValue;
  }

  /// <summary>
  /// Get current progress value.
  /// </summary>
  /// <returns>The progress value</returns>
  public float GetLoadingProgress(){
    return (_LoadingBar.value - _LoadingBar.minValue) / (_LoadingBar.maxValue - _LoadingBar.minValue); 
  }


  /// <summary>
  /// Function to bind <b>AsyncOperation</b> for the loading animation.
  /// </summary>
  /// <param name="async_op">Current progress source, can be null to unbind</param>
  public void BindAsyncOperation(AsyncOperation async_op = null){
    if(async_op != null){
      UIAnimationFinished = false;

      _LoadingBar.value = _LoadingBar.minValue;
      _current_speed = 0;
      _current_value = 0;
      _target_value = 0;

      async_op.completed += _operation_finished;
    }
    
    if(_async_op != null){
      _async_op.completed -= _operation_finished;
    }

    _async_op = async_op;
  }

  /// <summary>
  /// Unbinding the progress source, using <see cref="BindAsyncOperation"/> but with null value.
  /// </summary>
  public void UnbindAsyncOperation(){
    BindAsyncOperation();
  }
}