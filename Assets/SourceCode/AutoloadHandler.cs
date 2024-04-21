using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


/// <summary>
/// Ini adalah komponen untuk secara otomatis membuat semua Prefab yang ada di folder yang di register.
/// NOTE: Setidaknya ada satu GameObject yang menggunakan AutoloadHandler agar bekerja
/// </summary>
public class AutoloadHandler : MonoBehaviour{
  static private bool _is_initialized = false;

  /// <summary>
  /// List folder teregister untuk autoload
  /// </summary>
  static private string[] autoload_prefab_list = {
    "Assets/Scenes/AutoloadObject"
  };

  
  public void Awake(){
    if(_is_initialized){
      Destroy(this);
      return;
    }

    string[] _prefab_guid_list = AssetDatabase.FindAssets("t:prefab", autoload_prefab_list);
    foreach(string _guid in _prefab_guid_list){
      string _object_path = AssetDatabase.GUIDToAssetPath(_guid);
      GameObject _prefab_obj = AssetDatabase.LoadAssetAtPath<GameObject>(_object_path);

      GameObject _prefab_game_obj = Instantiate(_prefab_obj);
      DontDestroyOnLoad(_prefab_game_obj);
    }

    _is_initialized = true;
  }
}
