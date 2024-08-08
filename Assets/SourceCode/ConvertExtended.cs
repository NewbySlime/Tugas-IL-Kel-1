using System;
using System.Text;


/// <summary>
/// Static class containing functions for extending funcitonalities of C# Convert functions.
/// </summary>
public static class ConvertExt{
  /// <summary>
  /// Encodes normal string using Base64 encoder.
  /// </summary>
  /// <param name="from">Normal string</param>
  /// <returns>Encoded string</returns>
  public static string ToBase64String(string from){
    byte[] _from_bytes = Encoding.UTF32.GetBytes(from);
    return Convert.ToBase64String(_from_bytes);
  }

  /// <summary>
  /// Decodes a string that encoded in Base64 string.
  /// </summary>
  /// <param name="from">Base64 encoded string</param>
  /// <returns>Decoded string</returns>
  public static string FromBase64String(string from){
    byte[] _result_bytes = Convert.FromBase64String(from);
    return Encoding.UTF32.GetString(_result_bytes);
  }
}