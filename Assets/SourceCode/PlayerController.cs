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
  private MovementController _movement_controller;

  private InputContext _input_context;

  
  public void Start(){
    _movement_controller = GetComponent<MovementController>();
    _input_context = FindAnyObjectByType<InputContext>();
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