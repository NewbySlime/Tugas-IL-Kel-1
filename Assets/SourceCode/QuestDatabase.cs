using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class QuestDatabase: MonoBehaviour{
  private static string[] quest_data_folder = {
    "Assets/Scenes/PrefabObjects/Quests"
  };


  private struct _quest_metadata{
    public string quest_id;

    public GameObject quest_prefab;
    public string guid;
  }


  private Dictionary<string, _quest_metadata> _quest_map = new();

  
  public bool IsInitialized{get; private set;} = false;


  public void Start(){
    Debug.Log("Quest Database start");
    string[] _prefab_guid_list = AssetDatabase.FindAssets("t:prefab", quest_data_folder);
    foreach(string _guid in _prefab_guid_list){
      string _path = AssetDatabase.GUIDToAssetPath(_guid);
      GameObject _prefab_obj = AssetDatabase.LoadAssetAtPath<GameObject>(_path);

      GameObject _tmp_instantiate = Instantiate(_prefab_obj);
      while(true){
        IQuestData[] _quest_data = _tmp_instantiate.GetComponents<IQuestData>();
        if(_quest_data.Length > 1)
          Debug.LogWarning(string.Format("Quest Object (GUID: {0}) has multiple Quest Data interface.", _guid));
        else if(_quest_data.Length <= 0){
          Debug.LogError(string.Format("Quest Object (GUID: {0}) does not have Quest Data interface.", _guid));
          break;
        }

        IQuestData _quest = _quest_data[0];
        _quest_map[_quest.GetQuestID()] = new _quest_metadata{
          quest_id = _quest.GetQuestID(),
          quest_prefab = _prefab_obj,
          guid = _guid
        };

        break;
      }

      Destroy(_tmp_instantiate);
    }

    IsInitialized = true;
  }


  #nullable enable
  public GameObject? GetQuestPrefab(string quest_id){
    if(!_quest_map.ContainsKey(quest_id))
      return null;

    return _quest_map[quest_id].quest_prefab;
  }
  #nullable disable
}