using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Component for automatically creating (if not yet created) autoload objects (Prefabs) using Prefabs in a fixed folder path.
/// NOTE: a scene should have one object that has this component in order for completed Game function (even with the entry-point scene has it, a developer might go into DEBUG context and runs a random scene).
/// </summary>
public class AutoloadHandler : MonoBehaviour{
  /// <summary>
  /// A static variable to let any <see cref="AutoloadHandler"/> object know that the autoload objects has been created.
  /// </summary>
  static private bool _is_initialized = false;

  /// <summary>
  /// Path to a folder containing prefabs of autoload objects in Resources folder.
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
