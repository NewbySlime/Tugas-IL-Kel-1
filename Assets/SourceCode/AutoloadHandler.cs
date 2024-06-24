using System.Collections;
using System.Collections.Generic;
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
  static private string autoload_prefab_folder = "Scenes/AutoloadObject";

  
  public void Awake(){
    if(_is_initialized){
      Destroy(this);
      return;
    }

    GameObject[] _prefab_list = Resources.LoadAll<GameObject>(autoload_prefab_folder);
    DEBUGModeUtils.Log(string.Format("autoload handler count prefab {0}", _prefab_list.Length));
    foreach(GameObject _prefab_obj in _prefab_list){
      GameObject _prefab_game_obj = Instantiate(_prefab_obj);
      DontDestroyOnLoad(_prefab_game_obj);
    }

    _is_initialized = true;
  }
}
