using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;


[RequireComponent(typeof(Light2D))]
/// <summary>
/// Class for handling lighting object used for Game world's light.
/// This handler class also changes the configuration of the light based on the state of the Game (ex. the time period handled by <see cref="GameTimeHandler"/>).
/// 
/// This class uses following component(s);
/// - <b>Light2D</b> the light object used as the Game world's light.
/// 
/// This class uses autoload(s);
/// - <see cref="GameHandler"/> for game events and such.
/// - <see cref="GameTimeHandler"/> for configuring Game's world time and watching its events.
/// </summary>
public class GlobalLightHandler: MonoBehaviour{
  [SerializeField]
  private float _LightDaytimeValue = 1;

  [SerializeField]
  private float _LightNighttimeValue = 0.1f;

  private Light2D _light;

  private GameHandler _game_handler;
  private GameTimeHandler _time_handler;


  /// <summary>
  /// Flag if the object is ready or not yet.
  /// </summary>
  public bool IsInitialized{private get; set;} = false;


  // Function to catch event for when the period of time has changed by the GameTimeHandler.
  private void _time_period_changed(){
    if(!gameObject.activeInHierarchy || _game_handler == null || !_game_handler.SceneInitialized || !IsInitialized)
      return;

    float _light_value = 1;

    GameTimeHandler.GameTimePeriod _current_period = _time_handler.GetTimePeriod();
    switch(_current_period){
      case GameTimeHandler.GameTimePeriod.Daytime:{
        _light_value = _LightDaytimeValue;
      }break;

      case GameTimeHandler.GameTimePeriod.Nighttime:{
        _light_value = _LightNighttimeValue;
      }break;
    }

    _light.intensity = _light_value;
  }


  private void _on_scene_changed(string scene_id, GameHandler.GameContext context){
    _time_period_changed();
  }

  private void _on_scene_removed(){
    _game_handler.SceneChangedFinishedEvent -= _on_scene_changed;
    _game_handler.SceneRemovingEvent -= _on_scene_removed;

    _time_handler.OnTimePeriodChangedEvent -= _time_period_changed;
  }


  /// <summary>
  /// Function to catch "Object Destroyed" event by Unity.
  /// </summary>
  public void OnDestroy(){
    _on_scene_removed();
  }

  public void Start(){
    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingReferenceException();
    }

    _game_handler.SceneChangedFinishedEvent += _on_scene_changed;
    _game_handler.SceneRemovingEvent += _on_scene_removed;

    _time_handler = FindAnyObjectByType<GameTimeHandler>();
    if(_time_handler == null){
      Debug.LogError("Cannot find GameTimeHandler.");
      throw new MissingReferenceException();
    }

    _time_handler.OnTimePeriodChangedEvent += _time_period_changed;

    _light = GetComponent<Light2D>();
    _light.lightType = Light2D.LightType.Global;
    _light.intensity = 1;

    IsInitialized = true;
    _time_period_changed();
  }


  /// <summary>
  /// Function to catch "Object Enabled" event by Unity.
  /// </summary>
  public void OnEnable(){
    _time_period_changed();
  }
}