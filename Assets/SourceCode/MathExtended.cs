using Unity.Collections;
using UnityEngine;



public static class MathExt{
  public static Vector2 AngleToDirection(Transform transform){
    return AngleToDirection(transform.eulerAngles.z);
  }

  public static Vector2 AngleToDirection(float angle){
    return new Vector2(
      Mathf.Sin(-angle * Mathf.Deg2Rad),
      Mathf.Cos(-angle * Mathf.Deg2Rad)
    );
  }


  public static float DirectionToAngle(Vector2 dir){
    dir.Normalize();
    return NormalizeAngle(360 + (Mathf.Atan2(-dir.x, dir.y) * Mathf.Rad2Deg));
  }


  public static float NormalizeAngle(float angle){
    return Mathf.Repeat(Mathf.Abs(angle), 360);
  }

  public static float Range(float min, float max, float val){
    return (max-min)*val+min;
  }
}