using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


public class PersistanceContext: MonoBehaviour{
  private static string save_file_location = "save.dat";

  public delegate void PersistanceSaving(PersistanceContext context);
  public event PersistanceSaving PersistanceSavingEvent;

  public delegate void PersistanceLoading(PersistanceContext context);
  public event PersistanceLoading PersistanceLoadingEvent;

  public interface IPersistance{
    public string GetDataID();

    public string GetData();
    public void SetData(string data);
  }


  [Serializable]
  private struct _JSONData{
    public string data_type_id;
    public string data;
  }

  [Serializable]
  private struct _JSONDataCollection{
    public _JSONData[] data_list;
  }


  private Dictionary<string, _JSONData> _loaded_data;


  private bool _IsInitialized = false;
  public bool IsInitialized{get => _IsInitialized;}


  public void Start(){
    _IsInitialized = true;
  }


  public void WriteSave(){
    FileStream _save_file = File.Open(string.Format("{0}/{1}", Application.persistentDataPath, save_file_location), FileMode.Create);

    PersistanceSavingEvent?.Invoke(this);

    _JSONDataCollection _col = new _JSONDataCollection();
    _col.data_list = new _JSONData[_loaded_data.Count];

    int _i = 0;
    foreach(var key in _loaded_data.Keys){
      _JSONData _value = _loaded_data[key];
      _col.data_list[_i] = _value;

      _i++;
    }


    string _jsondata = JsonUtility.ToJson(_col);
    byte[] _jsondata_raw = Encoding.UTF32.GetBytes(_jsondata);
    string _filedata = Convert.ToBase64String(_jsondata_raw);
    byte[] _filedata_raw = Encoding.UTF8.GetBytes(_filedata);

    _save_file.Write(_filedata_raw);
    _save_file.Close();
  }

  public bool ReadSave(){
    try{
      FileStream _save_file = File.Open(string.Format("{0}/{1}", Application.persistentDataPath, save_file_location), FileMode.Open);

      byte[] _filedata_raw = new byte[_save_file.Length];
      int _left_bytes_iter = (int)_save_file.Length;
      int _bytes_iter = 0;
      while(_left_bytes_iter > 0){
        int _read_i = _save_file.Read(_filedata_raw, _bytes_iter, _left_bytes_iter);
        if(_read_i <= 0)
          break;

        _left_bytes_iter -= _read_i;
        _bytes_iter += _read_i;
      }

      string _filedata = Encoding.UTF8.GetString(_filedata_raw);
      byte[] _jsondata_raw = Convert.FromBase64String(_filedata);
      string _jsondata = Encoding.UTF32.GetString(_jsondata_raw);

      _JSONDataCollection _col = new _JSONDataCollection();
      JsonUtility.FromJson<_JSONDataCollection>(_jsondata);

      _loaded_data.Clear();
      for(int i = 0; i < _col.data_list.Length; i++){
        _JSONData _data = _col.data_list[i];
        _loaded_data[_data.data_type_id] = _data;
      }

      PersistanceLoadingEvent?.Invoke(this);
    }
    catch{
      return false;
    }

    return true;
  }

  
  public void ParseData(IPersistance target){
    string data_type_id = target.GetDataID();
    _JSONData _data = new _JSONData();

    _data.data_type_id = data_type_id;
    _data.data = target.GetData();

    _loaded_data[data_type_id] = _data;
  }

  public bool OverwriteData(IPersistance data){
    string data_type_id = data.GetDataID();
    if(!_loaded_data.ContainsKey(data_type_id))
      return false;

    _JSONData _data = _loaded_data[data_type_id];
    data.SetData(_data.data);

    return true;
  }
}