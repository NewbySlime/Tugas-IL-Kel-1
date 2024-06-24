using System.Collections.Generic;
using UnityEngine;


public class QuestDatabase: MonoBehaviour{
  private static string quest_data_folder = "Scenes/PrefabObjects/Quests";


  private struct _quest_metadata{
    public string quest_id;

    public GameObject quest_prefab;
  }


  private Dictionary<string, _quest_metadata> _quest_map = new();

  
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
  public GameObject? GetQuestPrefab(string quest_id){
    if(!_quest_map.ContainsKey(quest_id))
      return null;

    return _quest_map[quest_id].quest_prefab;
  }
  #nullable disable
}