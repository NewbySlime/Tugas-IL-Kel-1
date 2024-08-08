using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Class used for temporarily storing data in the Game as the data in the scene will be removed when changing scene.
/// This class uses two identifiers for a data, an ID for the data and the type of the data.
/// <seealso cref="TypeDataStorage"/>
/// </summary>
public class GameRuntimeData: MonoBehaviour{
  private Dictionary<string, TypeDataStorage> _runtime_data = new Dictionary<string, TypeDataStorage>();


  #nullable enable
  /// <summary>
  /// Get the data stored in this class based on the identifiers.
  /// Returns null if the data does not exist.
  /// </summary>
  /// <param name="data_id">ID of the data</param>
  /// <typeparam name="T">Type of the data</typeparam>
  /// <returns>The data stored</returns>
  public T? GetData<T>(string data_id){
    if(!_runtime_data.ContainsKey(data_id))
      return default;

    TypeDataStorage _data_storage = _runtime_data[data_id];
    return _data_storage.GetData<T>();
  }
  #nullable disable


  /// <summary>
  /// Store a data based on the identifiers. This will replace the existing data with similar identifiers.
  /// </summary>
  /// <param name="data_id">ID for the data</param>
  /// <param name="data">Type of the data</param>
  /// <typeparam name="T"></typeparam>
  public void SetData<T>(string data_id, T data){
    TypeDataStorage _data_storage;
    if(!_runtime_data.ContainsKey(data_id)){
      _data_storage = new TypeDataStorage();
      _runtime_data[data_id] = _data_storage;
    }
    else
      _data_storage = _runtime_data[data_id];

    _data_storage.AddData(data);
  }


  /// <summary>
  /// Clear all data stored in this class.
  /// </summary>
  public void ClearData(){
    _runtime_data.Clear();
  }
}