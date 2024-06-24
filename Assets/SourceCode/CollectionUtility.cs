using System.Collections.Generic;
using TMPro;


public static class CollectionUtility{
  public static Dictionary<TVal, TKey> CreateInverseDictionary<TKey, TVal>(Dictionary<TKey, TVal> dict){
    Dictionary<TVal, TKey> _result = new();
    foreach(TKey _key in dict.Keys)
      _result[dict[_key]] = _key;

    return _result;
  }
}