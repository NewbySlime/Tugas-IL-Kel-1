using System;
using System.Collections.Generic;
using Unity.VisualScripting;


/// <summary>
/// Storage class that stores any object based on the type of the data (the data type are for C# Dictionary's key). The class only stores one data for a type.
/// </summary>
public class TypeDataStorage{
  private Dictionary<Type, object> _data = new Dictionary<Type, object>();
  

  /// <summary>
  /// Add a data to the storage. Previous data with the same type will be replaced with the new one.
  /// </summary>
  /// <param name="data">The data to store</param>
  /// <typeparam name="T">The type of the data</typeparam>
  public void AddData<T>(T data){
    _data[typeof(T)] = data;
  }

  /// <summary>
  /// Remove certain data based on the type from the storage system.
  /// </summary>
  /// <typeparam name="T">The type of the data</typeparam>
  public void RemoveData<T>(){
    _data.Remove(typeof(T));
  }

  #nullable enable
  /// <summary>
  /// (C# Generic) <inheritdoc cref="GetData"/>
  /// </summary>
  /// <typeparam name="T">The type of the data</typeparam>
  /// <returns>The stored data</returns>
  public T? GetData<T>(){
    if(_data.ContainsKey(typeof(T)))
      return (T)_data[typeof(T)];
    else
      return default;
  }
  #nullable disable


  #nullable enable
  /// <summary>
  /// Get the data stored based on the type of the fetched data.
  /// </summary>
  /// <param name="type">The type of the data</param>
  /// <returns>The stored data</returns>
  public object? GetData(Type type){
    if(_data.ContainsKey(type))
      return _data[type];
    else
      return null;
  }
  #nullable disable
}