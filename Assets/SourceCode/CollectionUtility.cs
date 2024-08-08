using System.Collections.Generic;
using TMPro;


/// <summary>
/// Static class containing functions for extending functionalities of C# collection classes.
/// </summary>
public static class CollectionUtility{
  /// <summary>
  /// Create inverse dictionary from a source. Further explanation, the generics used in source dictionary will be inversed (key as value, value as key).
  /// </summary>
  /// <param name="dict">The source dictionary</param>
  /// <typeparam name="TKey">Key type of source dictionary</typeparam>
  /// <typeparam name="TVal">Value type of source dictionary</typeparam>
  /// <returns>Resulting inverse dictionary</returns>
  public static Dictionary<TVal, TKey> CreateInverseDictionary<TKey, TVal>(Dictionary<TKey, TVal> dict){
    Dictionary<TVal, TKey> _result = new();
    foreach(TKey _key in dict.Keys)
      _result[dict[_key]] = _key;

    return _result;
  }
}