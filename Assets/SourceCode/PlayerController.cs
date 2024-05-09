using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;



/// <summary>
/// Komponen untuk mengontrol Objek Game berdasarkan input dari Player.
/// Komponen ini memerlukan komponen lain:
///   - MovementController
/// </summary>
public class PlayerController: MonoBehaviour{
  private const string _PlayerRuntimeDataID = "player_data";


  private GameHandler _game_handler;

  private MovementController _movement_controller;
  private InputContext _input_context;

  private InventoryData _inv_data;


  private void _scene_changed(string scene_id, GameHandler.GameContext context){
    GameRuntimeData _runtime_data = FindAnyObjectByType<GameRuntimeData>();
    if(_runtime_data == null){
      Debug.LogWarning("No Runtime data storage, Player cannot retain data from last scene.");
      return;
    }

    if(_inv_data != null){
      InventoryData.RuntimeData _rdata = _runtime_data.GetData<InventoryData.RuntimeData>(_PlayerRuntimeDataID);
      _inv_data.FromRuntimeData(_rdata);
    }
  }

  private void _scene_removing(){
    Debug.Log("player scen removing");
    GameRuntimeData _runtime_data = FindAnyObjectByType<GameRuntimeData>();
    if(_runtime_data == null){
      Debug.LogWarning("No Runtime Data Storage, Player data would not be transferred to next scene.");
      return;
    }

    if(_inv_data != null){
      InventoryData.RuntimeData _rdata = _inv_data.AsRuntimeData();
      _runtime_data.SetData(_PlayerRuntimeDataID, _rdata);
    }
  }


  ~PlayerController(){
    _game_handler.SceneChangedFinishedEvent -= _scene_changed;
    _game_handler.SceneRemovingEvent -= _scene_removing;
  }

  
  public void Start(){
    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find Game Handler.");
      throw new UnityEngine.MissingComponentException();
    }

    _game_handler.SceneChangedFinishedEvent += _scene_changed;
    _game_handler.SceneRemovingEvent += _scene_removing;

    _movement_controller = GetComponent<MovementController>();
    _input_context = FindAnyObjectByType<InputContext>();

    _inv_data = GetComponent<InventoryData>();
    if(_inv_data == null){
      Debug.LogWarning("Player doesn't have Inventory!");
    }
  }


  /// <summary>
  /// Input Handling ketika Player memberikan input untuk bergerak secara horizontal.
  /// </summary>
  /// <param name="value">Value yang diberikan Unity.</param>
  public void OnStrafe(InputValue value){
    if(_input_context.GetUIInputContext(gameObject))
      return;

    float _strafe_value = value.Get<float>();
    _movement_controller.DoWalk(_strafe_value);
  }

  /// <summary>
  /// Input Handling ketika Player memberikan input untuk melompat.
  /// </summary>
  /// <param name="value">Value yang diberikan Unity.</param>
  public void OnJump(InputValue value){
    if(_input_context.GetUIInputContext(gameObject))
      return;
      
    if(value.isPressed)
      _movement_controller.DoJump();
  }
}