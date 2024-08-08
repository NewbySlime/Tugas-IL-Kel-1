using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


/// <summary>
/// Class for handling data, storing and loading the data with the OS' filesystem. The path for save file can be readjusted using <b>_SaveFileLocation</b>.
/// For any objects that wanted to store or load the data, use <see cref="PersistanceContext.IPersistance"/> and also use the events used in this class.
/// For further explanation, see <b>Reference/Diagrams/SaveSystem.drawio</b>
/// </summary>
public class PersistanceContext: MonoBehaviour{
  /// <summary>
  /// Saving trigger event. To store a data, use <see cref="ParseData"/>.
  /// </summary>
  public event PersistanceSaving PersistanceSavingEvent;
  public delegate void PersistanceSaving(PersistanceContext context);

  /// <summary>
  /// Loading trigger event. To get a data from <see cref="PersistanceContext"/>, use <see cref="OverwriteData"/>.
  /// </summary>
  public event PersistanceLoading PersistanceLoadingEvent;
  public delegate void PersistanceLoading(PersistanceContext context);

  
  /// <summary>
  /// Interface class for parsing data to format that <see cref="PersistanceContext"/> use for storing/loading data.
  /// </summary>
  public interface IPersistance{
    /// <summary>
    /// Get the data identifier.
    /// Inheriting class should create a unique ID for its data.
    /// </summary>
    /// <returns>The data ID</returns>
    public string GetDataID();

    /// <summary>
    /// Get the data contained in inheriting class using string format.
    /// NOTE: the data should not contain double quotes (") and single quote (') due to JSON format used by <see cref="PersistanceContext"/>. If it contains the prohibited character, the data should be encoded in other type.
    /// </summary>
    /// <returns>The data in string format</returns>
    public string GetData();

    /// <summary>
    /// Overwrites data of inheriting class based on the data supplied.
    /// </summary>
    /// <param name="data">Data in string format</param>
    public void SetData(string data);
  }



  [Serializable]
  // Data structure for storing IPersistance data.
  private struct _JSONData{
    public string data_type_id;
    public string data;
  }

  [Serializable]
  // Data structure for storing a list of _JSONData.
  private class _JSONDataCollection{
    public _JSONData[] data_list = new _JSONData[0];
  }


  [SerializeField]
  // Based on appdata/localdata folder.
  private string _SaveFileLocation = "save.dat";


  private Dictionary<string, _JSONData> _loaded_data = new();


  private bool _IsInitialized = false;
  /// <summary>
  /// Flag if the object is ready or not yet.
  /// </summary>
  public bool IsInitialized{get => _IsInitialized;}


  public void Start(){
    _IsInitialized = true;
  }


  /// <summary>
  /// Trigger a save event and handles writing to filesystem. This will also invokes signal for event that another object can use (<see cref="PersistanceSavingEvent"/>) to store its data.
  /// </summary>
  public void WriteSave(){
    FileStream _save_file = File.Open(string.Format("{0}/{1}", Application.persistentDataPath, _SaveFileLocation), FileMode.Create);

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
    string _filedata = ConvertExt.ToBase64String(_jsondata);
    byte[] _filedata_raw = Encoding.UTF8.GetBytes(_filedata);

    _save_file.Write(_filedata_raw);
    _save_file.Close();
  }


  /// <summary>
  /// Trigger a load event and handles reading from filesystem. After reading the data, the function will invoke signal for event that another object can use (<see cref="PersistanceLoadingEvent"/>) to get its data.
  /// </summary>
  /// <returns>Is the loading process successful or not</returns>
  public bool ReadSave(){
    try{
      DEBUGModeUtils.Log("save read test");
      FileStream _save_file = File.Open(string.Format("{0}/{1}", Application.persistentDataPath, _SaveFileLocation), FileMode.Open);
      DEBUGModeUtils.Log("save read test");

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

      DEBUGModeUtils.Log("save read test");
      _save_file.Close();

      string _filedata = Encoding.UTF8.GetString(_filedata_raw);
      string _jsondata = ConvertExt.FromBase64String(_filedata);

      DEBUGModeUtils.Log("save read test");
      _JSONDataCollection _col = new _JSONDataCollection();
      JsonUtility.FromJsonOverwrite(_jsondata, _col);

      DEBUGModeUtils.Log("save read test");
      _loaded_data.Clear();
      for(int i = 0; i < _col.data_list.Length; i++){
        _JSONData _data = _col.data_list[i];
        _loaded_data[_data.data_type_id] = _data;
      }

      DEBUGModeUtils.Log("save read test");
      PersistanceLoadingEvent?.Invoke(this);
    }
    catch(FileNotFoundException){
      // pass when file not found
      return false;
    }
    catch(Exception e){
      Debug.LogError(e);
      return false;
    }

    return true;
  }


  /// <summary>
  /// Manually sets the file location. The path will be appended to appdata/localdata folder path in Unity's default.
  /// </summary>
  /// <param name="file_name">Path to the save file</param>
  public void SetSaveFileName(string file_name){
    _SaveFileLocation = file_name;
  }

  /// <summary>
  /// Checks if the save file is valid (existed) and can be read.
  /// </summary>
  /// <returns>Is the save file valid</returns>
  public bool IsSaveValid(){
    try{
      FileStream _save_file = File.Open(string.Format("{0}/{1}", Application.persistentDataPath, _SaveFileLocation), FileMode.Open);
      long _file_length = _save_file.Length; 

      _save_file.Close();
      return _file_length > 0;
    }
    catch(FileNotFoundException){
      // pass when file not found;
    }
    catch(Exception e){
      Debug.LogError(e);
    }

    return false;
  }

  
  /// <summary>
  /// Parse and store the data from <see cref="IPersistance"/> to a format this component can use. This function only store the data to temporary storage in this component, not actually write a part of the data in the filesystem.
  /// For actually write the data to filesystem, see <see cref="WriteSave"/>.
  /// </summary>
  /// <param name="target">The source of data</param>
  public void ParseData(IPersistance target){
    string data_type_id = target.GetDataID();
    _JSONData _data = new _JSONData();

    _data.data_type_id = data_type_id;
    _data.data = target.GetData();

    _loaded_data[data_type_id] = _data;
  }

  /// <summary>
  /// Gets and parse the data from a format that this component use to a format that inheriting <see cref="IPersistance"/> class use. This function only gets the data from temporary storage in this component, not getting the data from filesystem.
  /// For actually reading the data from filesystem, see <see cref="ReadSave"/>. 
  /// </summary>
  /// <param name="data">The target data to overwrite</param>
  /// <returns>Is the overwriting process successful or not</returns>
  public bool OverwriteData(IPersistance data){
    string data_type_id = data.GetDataID();
    if(!_loaded_data.ContainsKey(data_type_id))
      return false;

    _JSONData _data = _loaded_data[data_type_id];
    data.SetData(_data.data);

    return true;
  }


  /// <summary>
  /// Clear all the stored data from supplied <see cref="IPersistance"/>.
  /// </summary>
  public void ClearData(){
    _loaded_data.Clear();
  }
}