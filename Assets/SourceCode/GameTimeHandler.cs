using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameTimeHandler: MonoBehaviour{
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


  public void Start(){
    Time.timeScale = 1;

    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingReferenceException();
    }
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
      Debug.Log(string.Format("setting time scenario {0}, {1}", _scenario_name, time == _period));
    }
  }

  public GameTimePeriod GetTimePeriod(){
    return _current_time_period;
  }
}