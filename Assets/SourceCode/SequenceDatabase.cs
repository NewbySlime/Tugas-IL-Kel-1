using System.Collections.Generic;
using System.Linq;
using UnityEngine;



/// <summary>
/// Database class for loading and storing custom sequence handlers.
/// The database will find any prefab in a determined folder with the prefab that has <see cref="ISequenceData"/> and <see cref="ISequenceAsync"/>. Custom sequence handler objects have to inherit those two interface class with their own implementation for the interface.
/// When inheriting <see cref="ISequenceData"/>, an object has to determine its ID that is unique to each sequence handler.
/// 
/// This class uses prefab(s);
/// - Prefab that has <see cref="SequenceHandlerVS"/> for handling sequence system.
/// - Prefab that has <see cref="SequenceInterface"/> for handling sub sequences.
/// </summary>
public class SequenceDatabase: MonoBehaviour{
  /// <summary>
  /// Path to a folder containing prefabs of custom sequence handler object in Resources folder.
  /// </summary>
  private static string sequence_data_folder = "Scenes/PrefabObjects/Sequences";


  private struct _sequence_metadata{
    public string sequence_id;

    public GameObject sequence_prefab;
  }

  [SerializeField]
  private GameObject _SequenceHandlerBasePrefab;

  [SerializeField]
  private GameObject _InterfaceSequence;


  private Dictionary<string, _sequence_metadata> _sequence_map = new();


  /// <summary>
  /// Flag if this object has been initialized or not.
  /// </summary>
  public bool IsInitialized{get; private set;} = false;


  public void Start(){
    Debug.Log("SequenceDatabase Starting...");
    GameObject[] _prefab_list = Resources.LoadAll<GameObject>(sequence_data_folder);
    foreach(GameObject _prefab_obj in _prefab_list){
      GameObject _tmp_instantiate = Instantiate(_prefab_obj);
      while(true){
        ISequenceData[] _seq_data = _tmp_instantiate.GetComponents<ISequenceData>();
        if(_seq_data.Length <= 0){
          Debug.LogError(string.Format("Cannot find ISequenceData in Prefab ({0})", _prefab_obj.name));
          break;
        }
        else if(_seq_data.Length > 1){
          Debug.LogWarning(string.Format("Object has more than 1 ISequenceData in Prefab ({0})", _prefab_obj.name));
        }

        ISequenceData _data = _seq_data[0];
        _sequence_map[_data.GetSequenceID()] = new _sequence_metadata{
          sequence_id = _data.GetSequenceID(),
          sequence_prefab = _prefab_obj
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
  /// <summary>
  /// Get prefab for certain custom sequence handler. 
  /// </summary>
  /// <param name="sequence_id">The sequence ID</param>
  /// <returns>The sequence handler prefab</returns>
  public GameObject? GetSequencePrefab(string sequence_id){
    if(!_sequence_map.ContainsKey(sequence_id))
      return null;

    return _sequence_map[sequence_id].sequence_prefab;
  }
  #nullable disable


  /// <summary>
  /// Get a default prefab for <see cref="SequenceInterface"/>.
  /// </summary>
  /// <returns><see cref="SequenceInterface"/> prefab</returns>
  public GameObject GetSequenceInterface(){
    return _InterfaceSequence;
  }

  /// <summary>
  /// Get a default prefab for <see cref="SequenceHandlerVS"/>.
  /// </summary>
  /// <returns><see cref="SequenceHandlerVS"/> prefab</returns>
  public GameObject GetSequenceHandlerBasePrefab(){
    return _SequenceHandlerBasePrefab;
  }
}