using System.Collections.Generic;
using UnityEngine;



public class GameRuntimeData: MonoBehaviour{
  private Dictionary<string, TypeDataStorage> _runtime_data = new Dictionary<string, TypeDataStorage>();


  #nullable enable
  public T? GetData<T>(string data_id){
    if(!_runtime_data.ContainsKey(data_id))
      return default;

    TypeDataStorage _data_storage = _runtime_data[data_id];
    return _data_storage.GetData<T>();
  }
  #nullable disable


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


  public void ClearData(){
    _runtime_data.Clear();
  }
}