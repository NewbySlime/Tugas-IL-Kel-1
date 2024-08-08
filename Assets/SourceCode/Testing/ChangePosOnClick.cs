using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;


namespace TESTING{
  /// <summary>
  /// Testing class that teleports a target object to its position when (defined) click event is triggered.
  /// This seems like a useless class, but it is used together with mouse follower object.
  /// </summary>
  public class ChangePosOnClick: MonoBehaviour{
    [SerializeField]
    private Transform _TargetTransform;


    /// <summary>
    /// Input event catch function that will teleport the target object.
    /// </summary>
    /// <param name="value">Input data from Unity</param>
    public void OnTestingClick(InputValue value){
      transform.position = _TargetTransform.position;
    }
  }
}