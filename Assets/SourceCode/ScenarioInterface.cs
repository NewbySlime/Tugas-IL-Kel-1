using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// Class to handles activation of its child objects based on the condition of the current scenario in the Game.
/// For further explanation, see <b>Reference/Diagrams/Scenario.drawio</b>
/// 
/// This class uses autoload(s);
/// - <see cref="GameHandler"/> for game events and such.
/// - <see cref="ScenarioDiagramVS"/> as the Game's scenario handling.
/// </summary>
public class ScenarioInterface: MonoBehaviour{
  [Serializable]
  /// <summary>
  /// Filter data used as a condition for activation of child objects.
  /// </summary>
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

  public bool IsInitialized{private set; get;} = false;


  #nullable enable
  private void _check_current_scenario(){
    if(!gameObject.activeInHierarchy)
      return;

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
      DEBUGModeUtils.Log(string.Format("subscenario check {0}/{1}", _subscenario, _Filter.SubScenarioID));
      if(_subscenario != _Filter.SubScenarioID){
        _activate = _ReverseFilter;
        break;
      }

      break;
    }

    SetInterfaceActive(_activate);
  }
  #nullable disable

  private IEnumerator _set_interface_active(bool flag){
    yield return null;
    yield return new WaitForEndOfFrame();

    for(int i = 0; i < transform.childCount; i++)
      transform.GetChild(i).gameObject.SetActive(flag);
  }

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
    DEBUGModeUtils.Log("scenario interface finished");

    if(_Filter.ScenarioID != scenario)
      return;

    _check_current_scenario();
  }


  private void _game_scene_initializing(string scene_id, GameHandler.GameContext context){
    DEBUGModeUtils.Log("Game Handler scene changed");
    _check_current_scenario();
    _scenario_diagram.ScenarioChangedStateEvent += _scenario_state_changed;

    ScenarioHandlerVS _this_scenario = _scenario_diagram.GetScenario(_Filter.ScenarioID);
    if(_this_scenario == null){
      Debug.LogWarning(string.Format("Scenario not found {0}", _Filter.ScenarioID));
      return;
    }

    DEBUGModeUtils.Log(string.Format("scenario {0} idx {1}", _Filter.ScenarioID, _this_scenario.GetCurrentSubScenario()));
    _this_scenario.ScenarioSubscenarioChangedEvent += _subscenario_state_changed;
    _this_scenario.ScenarioFinishedevent += _scenario_finished;
  }

  private void _game_scene_removed(){
    _handler.SceneChangedInitializingEvent -= _game_scene_initializing;
    _handler.SceneRemovingEvent -= _game_scene_removed;
    _scenario_diagram.ScenarioChangedStateEvent -= _scenario_state_changed;

    ScenarioHandlerVS _this_scenario = _scenario_diagram.GetScenario(_Filter.ScenarioID);
    if(_this_scenario == null){
      DEBUGModeUtils.LogWarning(string.Format("Scenario not found {0}", _Filter.ScenarioID));
      return;
    }

    _this_scenario.ScenarioSubscenarioChangedEvent -= _subscenario_state_changed;
    _this_scenario.ScenarioFinishedevent -= _scenario_finished;
  }


  /// <summary>
  /// Function to catch Unity's "Object Destroyed" event.
  /// </summary>
  public void OnDestroy(){
    DEBUGModeUtils.Log("scenario interface destroy");
    if(IsInitialized)
      _game_scene_removed();
  }


  public void Start(){
    DEBUGModeUtils.Log("ScenarioInterface Changed.");
    _handler = FindAnyObjectByType<GameHandler>();
    if(_handler == null){
      Debug.LogError("Cannot get GameHandler.");
      throw new MissingComponentException();
    }

    _handler.SceneChangedInitializingEvent += _game_scene_initializing;
    _handler.SceneRemovingEvent += _game_scene_removed;

    _scenario_diagram = FindAnyObjectByType<ScenarioDiagramVS>();
    if(_scenario_diagram == null){
      Debug.LogError("Cannot get Diagrams for Scenario.");
      throw new MissingComponentException();
    }

    if(_handler.SceneInitializing)
      _game_scene_initializing(_handler.GetCurrentSceneID(), _handler.GetCurrentSceneContext());

    IsInitialized = true;
  }

  /// <summary>
  /// Set all associated objects to the scenario to be activated or deactivated.
  /// </summary>
  /// <param name="active">Flag for activation</param>
  public void SetInterfaceActive(bool active){
    if(!gameObject.activeInHierarchy)
      return;

    StartCoroutine(_set_interface_active(active));
  }

  /// <summary>
  /// Function to catch Unity's "Object Enabled" event.
  /// </summary>
  public void OnEnable(){
    if(!IsInitialized)
      return;
      
    _check_current_scenario();
  }
}