using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;


/// <summary>
/// Class for handling the Game's time also handling scenario for period of time.
/// Period of time defined in this class;  (period type | Scenario ID used)
/// - Daytime | "daytime_scenario"
/// - Nighttime | "nighttime_scenario"
/// 
/// This class uses autoload(s);
/// - <see cref="GameHandler"/> for using scenario used by this class for changing period of time.
/// </summary>
public class GameTimeHandler: MonoBehaviour{
  /// <summary>
  /// Event for when the period of time has been changed.
  /// </summary>
  public event OnTimePeriodChanged OnTimePeriodChangedEvent;
  public delegate void OnTimePeriodChanged();

  /// <summary>
  /// Type of Game's period of time.
  /// </summary>
  public enum GameTimePeriod{
    Daytime,
    Nighttime
  }


  private static Dictionary<GameTimePeriod, string> _GameTimePeriod_ScenarioName = new(){
    {GameTimePeriod.Daytime, "daytime_scenario"},
    {GameTimePeriod.Nighttime, "nighttime_scenario"}
  };


  private GameTimePeriod _current_time_period = GameTimePeriod.Daytime;
  private GameHandler _game_handler;


  private void _on_scene_initializing(string scene_id, GameHandler.GameContext context){
    GameTimePeriod _time_period = _current_time_period;
    foreach(GameTimePeriod time in _GameTimePeriod_ScenarioName.Keys){
      string scenario_id = _GameTimePeriod_ScenarioName[time];
      if(_game_handler._ScenarioDiagram.GetEnableScenario(scenario_id)){
        _time_period = time;
        break;
      }
    }

    SetTimePeriod(_time_period);
  }


  public void Start(){
    Time.timeScale = 1;

    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingReferenceException();
    }

    _game_handler.SceneChangedInitializingEvent += _on_scene_initializing;
  }


  /// <summary>
  /// Stops the Game's time by using Unity's time scale.
  /// </summary>
  public void StopTime(){
    Time.timeScale = 0;
  }

  /// <summary>
  /// Resumes the Game's time by using Unity's time scale.
  /// </summary>
  public void ResumeTime(){
    Time.timeScale = 1;
  }

  /// <summary>
  /// Check if the Game is still paused.
  /// </summary>
  /// <returns>Is the Game still in pausing state</returns>
  public bool IsPausing(){
    return Time.timeScale < 0.05; 
  }


  /// <summary>
  /// Set the timing scale used by Unity's engine.
  /// </summary>
  /// <param name="scale">The new timing scale</param>
  public void SetTimeScale(float scale){
    Time.timeScale = scale;
  }

  /// <summary>
  /// Get the scale in Unity's timing.
  /// </summary>
  /// <returns>Current timing scale</returns>
  public float GetTimeScale(){
    return Time.timeScale;
  }


  /// <summary>
  /// Change the period of time. This function uses Game's scenario to change the time period.
  /// </summary>
  /// <param name="time">Type of the period</param>
  public void SetTimePeriod(GameTimePeriod time){
    _current_time_period = time;

    foreach(GameTimePeriod _period in _GameTimePeriod_ScenarioName.Keys){
      string _scenario_name = _GameTimePeriod_ScenarioName[_period];

      _game_handler._ScenarioDiagram.SetEnableScenario(_scenario_name, _period == time);
    }

    OnTimePeriodChangedEvent?.Invoke();
  }

  /// <summary>
  /// Get the current time period.
  /// </summary>
  /// <returns>Current time period</returns>
  public GameTimePeriod GetTimePeriod(){
    return _current_time_period;
  }
}