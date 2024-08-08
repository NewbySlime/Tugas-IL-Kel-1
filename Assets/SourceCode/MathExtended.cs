using Unity.Collections;
using UnityEngine;



/// <summary>
/// Static utility class for extending functionality of <b>System.Math</b> class. This class contains more math functions used for this Game.
/// </summary>
public static class MathExt{
  /// <summary>
  /// Converts angle of 2D rotation (Z-Axis) to normalized vector for the direction.
  /// </summary>
  /// <param name="transform">The target transform object</param>
  /// <returns>Resulting normalized direction</returns>
  public static Vector2 AngleToDirection(Transform transform){
    return AngleToDirection(transform.eulerAngles.z);
  }

  /// <summary>
  /// <inheritdoc cref="AngleToDirection"/>
  /// </summary>
  /// <param name="angle">The angle to convert from (in degrees)</param>
  /// <returns><inheritdoc cref="AngleToDirection"/></returns>
  public static Vector2 AngleToDirection(float angle){
    return new Vector2(
      Mathf.Sin(-angle * Mathf.Deg2Rad),
      Mathf.Cos(-angle * Mathf.Deg2Rad)
    );
  }


  /// <summary>
  /// Converts normalized vector of direction to angle of 2D rotation (Z-Axis).
  /// </summary>
  /// <param name="dir">The normalized direction to convert from</param>
  /// <returns>Resulting angle (in degrees)</returns>
  public static float DirectionToAngle(Vector2 dir){
    dir.Normalize();
    return NormalizeAngle(360 + (Mathf.Atan2(-dir.x, dir.y) * Mathf.Rad2Deg));
  }


  /// <summary>
  /// This repeats the angle of a supplied value to be clamped to 360 degrees. 
  /// </summary>
  /// <param name="angle">The target angle (in degrees)</param>
  /// <returns>Repeated angle</returns>
  public static float NormalizeAngle(float angle){
    return Mathf.Repeat(Mathf.Abs(angle), 360);
  }

  /// <summary>
  /// Calculates a normalized value for readjusting it to the supplied range (max - min).
  /// </summary>
  /// <param name="min">The minimum range</param>
  /// <param name="max">The maximum range</param>
  /// <param name="val">The normalized value</param>
  /// <returns>Resulting value from a range (within max-min)</returns>
  public static float Range(float min, float max, float val){
    return (max-min)*val+min;
  }
}