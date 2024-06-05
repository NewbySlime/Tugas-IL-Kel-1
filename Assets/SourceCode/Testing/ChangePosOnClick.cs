using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;


namespace TESTING{
  public class ChangePosOnClick: MonoBehaviour{
    [SerializeField]
    private Transform _TargetTransform;


    public void OnTestingClick(InputValue value){
      transform.position = _TargetTransform.position;
    }
  }
}