using UnityEngine;


/// <summary>
/// An Interface class as a reference to the Material being hold in the class.
/// </summary>
public interface IMaterialReference{
  /// <summary>
  /// Gets the Material used in the class.
  /// </summary>
  /// <returns>The Material Object</returns>
  public Material GetMaterial();
}