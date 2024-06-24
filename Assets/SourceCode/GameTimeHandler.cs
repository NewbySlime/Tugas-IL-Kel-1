using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;


public class GameTimeHandler: MonoBehaviour{
  public delegate void OnTimePeriodChanged();
  public event OnTimePeriodChanged OnTimePeriodChangedEvent;

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


  public void StopTime(){
    Time.timeScale = 0;
  }

  public void ResumeTime(){
    Time.timeScale = 1;
  }

  public bool IsPausing(){
    return Time.timeScale < 0.05; 
  }


  public void SetTimeScale(float scale){
    Time.timeScale = scale;
  }

  public float GetTimeScale(){
    return Time.timeScale;
  }


  public void SetTimePeriod(GameTimePeriod time){
    _current_time_period = time;

    foreach(GameTimePeriod _period in _GameTimePeriod_ScenarioName.Keys){
      string _scenario_name = _GameTimePeriod_ScenarioName[_period];

      _game_handler._ScenarioDiagram.SetEnableScenario(_scenario_name, _period == time);
    }

    OnTimePeriodChangedEvent?.Invoke();
  }

  public GameTimePeriod GetTimePeriod(){
    return _current_time_period;
  }
}