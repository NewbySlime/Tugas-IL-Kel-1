using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;



public class SequenceDatabase: MonoBehaviour{
  private static string[] sequence_data_folder = {
    "Assets/Scenes/PrefabObjects/Sequences"
  };


  private struct _sequence_metadata{
    public string sequence_id;

    public GameObject sequence_prefab;
    public string guid;
  }

  [SerializeField]
  private GameObject _SequenceHandlerBasePrefab;

  [SerializeField]
  private GameObject _InterfaceSequence;


  private Dictionary<string, _sequence_metadata> _sequence_map = new();


  public bool IsInitialized{get; private set;} = false;


  public void Start(){
    Debug.Log("Sequence Database start");
    string[] _prefab_guid_list = AssetDatabase.FindAssets("t:prefab", sequence_data_folder);
    foreach(string _guid in _prefab_guid_list){
      string _path = AssetDatabase.GUIDToAssetPath(_guid);
      GameObject _prefab_obj = AssetDatabase.LoadAssetAtPath<GameObject>(_path);

      GameObject _tmp_instantiate = Instantiate(_prefab_obj);
      while(true){
        ISequenceData[] _seq_data = _tmp_instantiate.GetComponents<ISequenceData>();
        if(_seq_data.Length <= 0){
          Debug.LogError(string.Format("Cannot find ISequenceData for GUID ({0})", _guid));
          break;
        }
        else if(_seq_data.Length > 1){
          Debug.LogWarning(string.Format("Object has more than 1 ISequenceData in GUID ({0})", _guid));
        }

        ISequenceData _data = _seq_data[0];
        _sequence_map[_data.GetSequenceID()] = new _sequence_metadata{
          sequence_id = _data.GetSequenceID(),
          sequence_prefab = _prefab_obj,
          guid = _guid
        };

        break;
      }

      Destroy(_tmp_instantiate);
    }

    {GameObject _test_obj = Instantiate(_SequenceHandlerBasePrefab);
      if(_test_obj.GetComponent<SequenceHandlerVS>() == null){
        Debug.LogError("SequenceHandler Base Prefab does not have SequenceHandlerVS.");
        throw new MissingComponentException();
      }
    Destroy(_test_obj);}

    IsInitialized = true;
  }


  #nullable enable
  public GameObject? GetSequencePrefab(string sequence_id){
    if(!_sequence_map.ContainsKey(sequence_id))
      return null;

    return _sequence_map[sequence_id].sequence_prefab;
  }
  #nullable disable


  public GameObject GetSequenceInterface(){
    return _InterfaceSequence;
  }

  public GameObject GetSequenceHandlerBasePrefab(){
    return _SequenceHandlerBasePrefab;
  }
}