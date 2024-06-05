using System;
using System.Text;


public static class ConvertExt{
  public static string ToBase64String(string from){
    byte[] _from_bytes = Encoding.UTF32.GetBytes(from);
    return Convert.ToBase64String(_from_bytes);
  }

  public static string FromBase64String(string from){
    byte[] _result_bytes = Convert.FromBase64String(from);
    return Encoding.UTF32.GetString(_result_bytes);
  }
}