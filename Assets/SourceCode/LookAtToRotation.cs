using UnityEngine;


/// <summary>
/// Component for covering <see cref="ILookAtReceiver"/> interface functionality for rotating an object based on the direction it should look.
/// </summary>
public class LookAtToRotation: MonoBehaviour, ILookAtReceiver{
  public void LookAt(Vector2 direction){
    if(!enabled)
      return;

    float _rotation = MathExt.DirectionToAngle(direction);
    transform.localEulerAngles = new Vector3(transform.rotation.x, transform.rotation.y, _rotation);
  }
}