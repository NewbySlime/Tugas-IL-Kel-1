using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public class SequenceHandlerVS: MonoBehaviour, ISequenceAsync{
  [Serializable]
  public class SequenceInitializeData{
    public class DataPart{
      public string SequenceID;
      public object SequenceData;
    }

    public List<List<DataPart>> SequenceList;
  }

  private SequenceInitializeData _init_data = null;
  private SequenceDatabase _seq_database = null;

  private List<SequenceInterface> _sequences_list = new List<SequenceInterface>();

  private bool _sequence_triggering = false;


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

      Debug.Log("Sequence add");
      _sequences_list.Add(_interface);
    }
  }


  private IEnumerator _sequence_start(){
    Debug.Log("Sequence trigger start");
    _sequence_triggering = true;
    foreach(SequenceInterface _seq_interface in _sequences_list){
      Debug.Log("Sequence interface trigger.");
      yield return _seq_interface.StartTrigger();
    }

    _sequence_triggering = false;
    Debug.Log("Sequence trigger finished");
  }


  public void Start(){
    _seq_database = FindAnyObjectByType<SequenceDatabase>();
    if(_seq_database == null){
      Debug.LogError("Cannot get database for Sequences.");
      throw new UnityEngine.MissingComponentException();
    }

    StartCoroutine(_update_sequence());
  }


  public IEnumerator StartTrigger(){
    yield return _sequence_start();
  }


  public void StartTriggerAsync(){
    StartCoroutine(_sequence_start());
  }

  public bool IsTriggering(){
    return _sequence_triggering;
  }


  public void SetInitData(SequenceInitializeData data){
    _init_data = data;
    StartCoroutine(_update_sequence());
  }
}