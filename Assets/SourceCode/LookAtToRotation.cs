using UnityEngine;


public class LookAtToRotation: MonoBehaviour, ILookAtReceiver{
  public void LookAt(Vector2 direction){
    if(!enabled)
      return;

    float _rotation = MathExt.DirectionToAngle(direction);
    transform.localScale = new Vector3(transform.rotation.x, transform.rotation.y, _rotation);
  }
}