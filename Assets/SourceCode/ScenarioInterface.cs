using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;


public class ScenarioInterface: MonoBehaviour{
  [Serializable]
  private struct _ScenarioFilter{
    public string ScenarioID;
    public string SubScenarioID;

    public bool IsScenarioFinished;
  }


  [SerializeField]
  private _ScenarioFilter _Filter;

  [SerializeField]
  private bool _ReverseFilter;


  private GameHandler _handler;
  private ScenarioDiagramVS _scenario_diagram;


  #nullable enable
  private void _check_current_scenario(){
    bool _activate = !_ReverseFilter;
    while(true){
      if(!_scenario_diagram.GetEnableScenario(_Filter.ScenarioID)){
        _activate = _ReverseFilter;
        break;
      }

      ScenarioHandlerVS? _scenario = _scenario_diagram.GetScenario(_Filter.ScenarioID);
      if(_scenario == null){
        _activate = _ReverseFilter;
        break;
      }

      if(_Filter.IsScenarioFinished && _scenario.IsScenarioFinished())
        break;

      if(_Filter.SubScenarioID.Length <= 0)
        break;

      string _subscenario = _scenario.GetCurrentSubScenario();
      if(_subscenario != _Filter.SubScenarioID){
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

  private void _scenario_finished(string scenario){
    Debug.Log("scenario interface finished");

    if(_Filter.ScenarioID != scenario)
      return;

    _check_current_scenario();
  }


  private void _game_scene_changed(string scene_id, GameHandler.GameContext context){
    Debug.Log("Game Handler scene changed");
    _check_current_scenario();
    _scenario_diagram.ScenarioChangedStateEvent += _scenario_state_changed;

    ScenarioHandlerVS _this_scenario = _scenario_diagram.GetScenario(_Filter.ScenarioID);
    if(_this_scenario == null){
      Debug.LogWarning(string.Format("Scenario not found {0}", _Filter.ScenarioID));
      return;
    }

    _this_scenario.ScenarioSubscenarioChangedEvent += _subscenario_state_changed;
    _this_scenario.ScenarioFinishedevent += _scenario_finished;
  }

  private void _game_scene_removed(){
    _handler.SceneChangedFinishedEvent -= _game_scene_changed;
    _handler.SceneRemovingEvent -= _game_scene_removed;
    _scenario_diagram.ScenarioChangedStateEvent -= _scenario_state_changed;

    ScenarioHandlerVS _this_scenario = _scenario_diagram.GetScenario(_Filter.ScenarioID);
    if(_this_scenario == null){
      Debug.LogWarning(string.Format("Scenario not found {0}", _Filter.ScenarioID));
      return;
    }

    _this_scenario.ScenarioSubscenarioChangedEvent -= _subscenario_state_changed;
    _this_scenario.ScenarioFinishedevent -= _scenario_finished;
  }


  ~ScenarioInterface(){
    _game_scene_removed();
  }


  public void Start(){
    Debug.Log("ScenarioInterface Changed.");
    _handler = FindAnyObjectByType<GameHandler>();
    if(_handler == null){
      Debug.LogError("Cannot get GameHandler.");
      throw new MissingComponentException();
    }

    _handler.SceneChangedFinishedEvent += _game_scene_changed;
    _handler.SceneRemovingEvent += _game_scene_removed;

    _scenario_diagram = FindAnyObjectByType<ScenarioDiagramVS>();
    if(_scenario_diagram == null){
      Debug.LogError("Cannot get Diagrams for Scenario.");
      throw new MissingComponentException();
    }

    if(_handler.SceneInitialized)
      _game_scene_changed(_handler.GetCurrentSceneID(), _handler.GetCurrentSceneContext());
  }

  public void SetInterfaceActive(bool active){
    for(int i = 0; i < transform.childCount; i++)
      transform.GetChild(i).gameObject.SetActive(active);
  }
}