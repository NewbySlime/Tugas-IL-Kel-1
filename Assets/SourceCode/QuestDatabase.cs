using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Database class for loading and storing custom quest handlers.
/// The database will find any prefab in a determined folder with the prefab that has <see cref="IQuestData"/> and <see cref="IQuestHandler"/>. Custom quest handler objects have to inherit those two interface class with their own implementation for the interface.
/// When inheriting <see cref="IQuestData"/>, an object has to determine its ID that is unique to each quest handler.
/// </summary>
public class QuestDatabase: MonoBehaviour{
  /// <summary>
  /// Path to a folder containing prefabs of custom quest handler object in Resources folder.
  /// </summary>
  private static string quest_data_folder = "Scenes/PrefabObjects/Quests";


  private struct _quest_metadata{
    public string quest_id;

    public GameObject quest_prefab;
  }


  private Dictionary<string, _quest_metadata> _quest_map = new();

  
  /// <summary>
  /// Flag if this object has been initialized or not.
  /// </summary>
  public bool IsInitialized{get; private set;} = false;


  public void Start(){
    Debug.Log("QuestDatabase Starting...");
    GameObject[] _prefab_list = Resources.LoadAll<GameObject>(quest_data_folder);
    foreach(GameObject _prefab_obj in _prefab_list){
      GameObject _tmp_instantiate = Instantiate(_prefab_obj);
      while(true){
        IQuestData[] _quest_data = _tmp_instantiate.GetComponents<IQuestData>();
        if(_quest_data.Length > 1)
          Debug.LogWarning(string.Format("Quest Object ({0}) has multiple Quest Data interface.", _prefab_obj.name));
        else if(_quest_data.Length <= 0){
          Debug.LogError(string.Format("Quest Object ({0}) does not have Quest Data interface.", _prefab_obj.name));
          break;
        }

        IQuestData _quest = _quest_data[0];
        _quest_map[_quest.GetQuestID()] = new _quest_metadata{
          quest_id = _quest.GetQuestID(),
          quest_prefab = _prefab_obj
        };

        break;
      }

      Destroy(_tmp_instantiate);
    }

    IsInitialized = true;
  }


  #nullable enable
  /// <summary>
  /// Get prefab for certain custom quest handler.
  /// </summary>
  /// <param name="quest_id">Which quest handler ID</param>
  /// <returns>The quest handler prefab</returns>
  public GameObject? GetQuestPrefab(string quest_id){
    if(!_quest_map.ContainsKey(quest_id))
      return null;

    return _quest_map[quest_id].quest_prefab;
  }
  #nullable disable
}