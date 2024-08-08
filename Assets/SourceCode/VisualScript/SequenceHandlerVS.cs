using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// Class for handling sequencing system. This class uses <see cref="SequenceInterface"/> for handling subsequences.
/// For further explanation on how sequencing system works in the Game, read the diagram contained in <b>Reference/Diagrams/Sequence.drawio</b>
/// 
/// This class uses autoload(s);
/// - <see cref="SequenceDatabase"/> to create sequence object.
/// </summary>
public class SequenceHandlerVS: MonoBehaviour, ISequenceAsync{
  [Serializable]
  /// <summary>
  /// Initializing data for creating sequence in <see cref="SequenceHandlerVS"/>.
  /// </summary>
  public class SequenceInitializeData{
    /// <summary>
    /// Data for each sequence.
    /// </summary>
    public class DataPart{
      /// <summary>
      /// The ID of what the sequence object should be.
      /// </summary>
      public string SequenceID;

      /// <summary>
      /// The data specific to certain sequence object.
      /// </summary>
      public object SequenceData;
    }

    /// <summary>
    /// List of sequence data.
    /// </summary>
    public List<List<DataPart>> SequenceList;
  }

  private SequenceInitializeData _init_data = null;
  private SequenceDatabase _seq_database = null;

  private List<SequenceInterface> _sequences_list = new List<SequenceInterface>();

  private bool _sequence_triggering = false;

  /// <summary>
  /// Flag for if sequence data has been supplied.
  /// </summary>
  public bool SequenceInitializeDataSet{get => _init_data != null;}


  /// <summary>
  /// This function is an extension from <see cref="SetInitdata"/> function which to update the sequence list and create and initialize sequence object using coroutine for yielding functions.
  /// </summary>
  /// <returns>Coroutine helper object</returns>
  private IEnumerator _update_sequence(){
    if(_init_data == null || _seq_database == null)
      yield break;

    yield return new WaitUntil(() => _seq_database.IsInitialized);

    _sequences_list.Clear();
    int _child_count = transform.childCount;
    for(int i = 0; i < _child_count; i++)
      Destroy(transform.GetChild(i).gameObject);

    GameObject _interface_prefab = _seq_database.GetSequenceInterface();
    if(_interface_prefab == null){
      Debug.LogError("Cannot get prefab for SequenceInterface");
      yield break;
    }

    foreach(var _list in _init_data.SequenceList){
      GameObject _interface_obj = Instantiate(_interface_prefab);
      SequenceInterface _interface = _interface_obj.GetComponent<SequenceInterface>();
      if(_interface == null){
        Debug.LogError("Interface prefab used doens't have SequenceInterface Component.");
        Destroy(_interface_obj);

        yield break;
      }

      _interface.transform.SetParent(transform);
      _interface.name = "SequenceInterface" + _sequences_list.Count.ToString();

      foreach(SequenceInitializeData.DataPart _data in _list){
        GameObject _prefab = _seq_database.GetSequencePrefab(_data.SequenceID);
        if(_prefab == null){
          Debug.LogError(string.Format("Cannot get Sequence Prefab for ID: '{0}'", _data.SequenceID));
        }

        GameObject _inst_obj = Instantiate(_prefab);
        _inst_obj.transform.SetParent(_interface.transform);

        ISequenceData _seq_data = _inst_obj.GetComponent<ISequenceData>();
        if(_seq_data == null){
          Debug.LogWarning(string.Format("Sequence Prefab (ID: {0}) doesn't have ISequenceData interface.", _data.SequenceID));
          continue;
        }

        _seq_data.SetSequenceData(_data.SequenceData);
      }

      DEBUGModeUtils.Log("Sequence add");
      _sequences_list.Add(_interface);
    }
  }


  // Trigger each sequence in order.
  private IEnumerator _sequence_start(){
    DEBUGModeUtils.Log("Sequence trigger start");
    _sequence_triggering = true;
    foreach(SequenceInterface _seq_interface in _sequences_list){
      DEBUGModeUtils.Log("Sequence interface trigger.");
      yield return _seq_interface.StartTrigger();
    }

    _sequence_triggering = false;
    DEBUGModeUtils.Log("Sequence trigger finished");
  }


  public void Start(){
    _seq_database = FindAnyObjectByType<SequenceDatabase>();
    if(_seq_database == null){
      Debug.LogError("Cannot get database for Sequences.");
      throw new UnityEngine.MissingComponentException();
    }

    StartCoroutine(_update_sequence());
  }


  /// <summary>
  /// Trigger to start every sequence in order using Coroutine.
  /// </summary>
  /// <returns>Coroutine helper object</returns>
  public IEnumerator StartTrigger(){
    yield return _sequence_start();
  }


  /// <summary>
  /// Trigger to start every sequence in order without blocking (asynchronously).
  /// </summary>
  public void StartTriggerAsync(){
    StartCoroutine(_sequence_start());
  }

  /// <summary>
  /// To check if the handler still running the sequencing system or not.
  /// </summary>
  /// <returns>The resulting flag</returns>
  public bool IsTriggering(){
    return _sequence_triggering;
  }


  /// <summary>
  /// To set the prepping data for this class to create and prep a list of sequences.
  /// This function can be used by Visual Scripting using normal calls.
  /// The function does preparation asynchronously, that redirects to <see cref="_update_sequence"/>.
  /// </summary>
  /// <param name="data"></param>
  public void SetInitData(SequenceInitializeData data){
    _init_data = data;
    StartCoroutine(_update_sequence());
  }
}