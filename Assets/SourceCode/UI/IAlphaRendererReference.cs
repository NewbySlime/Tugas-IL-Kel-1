using UnityEngine;


/// <summary>
/// Interface class for handling alpha values in inheriting class.
/// </summary>
public interface IAlphaRendererReference{
  /// <summary>
  /// To set alpha value for the inheriting class.
  /// </summary>
  /// <param name="value">The alpha value</param>
  public void SetAlpha(float value);

  /// <summary>
  /// To get the alpha value from the inheriting class.
  /// </summary>
  /// <returns>The alpha value</returns>
  public float GetAlpha();
}