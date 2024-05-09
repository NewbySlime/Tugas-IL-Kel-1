using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class ScenarioInterface: MonoBehaviour{
  [Serializable]
  private struct _ScenarioFilter{
    public string ScenarioID;
    public string SubScenarioID;
  }


  [SerializeField]
  private _ScenarioFilter _Filter;

  [SerializeField]
  private bool _ReverseFilter;


  private GameHandler _handler;


  #nullable enable
  private void _check_current_scenario(){
    bool _activate = !_ReverseFilter;
    while(true){
      if(!_handler.GetEnableScenario(_Filter.ScenarioID)){
        Debug.Log("Scenario not enabled");
        _activate = _ReverseFilter;
        break;
      }

      ScenarioHandler? _scenario = _handler.GetScenario(_Filter.ScenarioID);
      if(_scenario == null){
        Debug.Log("Scenario not found");
        _activate = _ReverseFilter;
        break;
      }

      if(_Filter.SubScenarioID.Length <= 0)
        break;

      string _subscenario = _scenario.GetCurrentSubScenario();
      if(_subscenario != _Filter.SubScenarioID){
        Debug.Log("Subscenario false");
        _activate = _ReverseFilter;
        break;
      }

      break;
    }

    SetInterfaceActive(_activate);
  }
  #nullable disable

  private void _scenario_state_changed(string scenario){
    if(_Filter.ScenarioID != scenario)
      return;
    
    _check_current_scenario();
  }

  private void _subscenario_state_changed(string scenario, string last_subid, string next_subid){
    if(_Filter.ScenarioID != scenario)
      return;

    _check_current_scenario();
  }


  private void _game_scene_changed(string scene_id, GameHandler.GameContext context){
    Debug.Log("Game Handler scene changed");
    _handler.ScenarioStateChangedEvent += _scenario_state_changed;
    _handler.SubScenarioStateChangedEvent += _subscenario_state_changed;

    _check_current_scenario();
  }

  private void _game_scene_removing(){
    Debug.Log("Game Handler removing scene");
    _handler.ScenarioStateChangedEvent -= _scenario_state_changed;
    _handler.SubScenarioStateChangedEvent -= _subscenario_state_changed;

    _handler.SceneChangedFinishedEvent -= _game_scene_changed;
    _handler.SceneRemovingEvent -= _game_scene_removing;
  }


  public void Start(){
    Debug.Log("ScenarioInterface Changed.");
    _handler = FindAnyObjectByType<GameHandler>();
    if(_handler == null){
      Debug.LogError("Cannot get GameHandler.");
      throw new UnityEngine.MissingComponentException();
    }

    _handler.SceneChangedFinishedEvent += _game_scene_changed;
    _handler.SceneRemovingEvent += _game_scene_removing;
  }

  public void SetInterfaceActive(bool active){
    for(int i = 0; i < transform.childCount; i++)
      transform.GetChild(i).gameObject.SetActive(active);
  }
}