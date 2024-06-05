using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;


public class CharacterDatabase: MonoBehaviour{
  public delegate void OnInitialized();
  public event OnInitialized OnInitializedEvent;

  private static string[] character_data_folder = {
    "Assets/Characters"
  };


  private struct _character_metadata{
    public string character_id;
    public string character_name;

    public TypeDataStorage data_storage;

    public GameObject _this;
    public string guid;
  }

  private Dictionary<string, _character_metadata> _character_map = new();


  public bool IsInitialized{get; private set;} = false;


  public void Start(){
    string[] _prefab_guid_list = AssetDatabase.FindAssets("t:prefab", character_data_folder);
    foreach(string _guid in _prefab_guid_list){
      string _object_path = AssetDatabase.GUIDToAssetPath(_guid);
      GameObject _prefab_obj = AssetDatabase.LoadAssetAtPath<GameObject>(_object_path);

      GameObject _tmp_gameobj = Instantiate(_prefab_obj);
      CharacterMetadata _metadata = _tmp_gameobj.GetComponent<CharacterMetadata>();
      if(_metadata == null){
        Debug.LogError(string.Format("GUID: '{0}' is not an Character.", _guid));
        continue;
      }

      TypeDataStorage _character_data = new();
      _tmp_gameobj.SendMessage("CharacterDatabase_LoadData", _character_data, SendMessageOptions.DontRequireReceiver);

      Destroy(_tmp_gameobj);

      _character_map.Add(_metadata.GetCharacterID(), new _character_metadata{
        character_id = _metadata.GetCharacterID(),
        character_name = _metadata.GetCharacterName(),

        data_storage = _character_data,

        _this = _prefab_obj,
        guid = _guid
      });
    }

    IsInitialized = true;
    OnInitializedEvent?.Invoke();
  }


  #nullable enable
  public TypeDataStorage? GetDataStorage(string character_id){
    if(!_character_map.ContainsKey(character_id))
      return null;

    return _character_map[character_id].data_storage;
  }
  #nullable disable
}