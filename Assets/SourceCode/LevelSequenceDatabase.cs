using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class LevelSequenceDatabase: MonoBehaviour{
  [Serializable]
  private struct _SequenceMetadata{
    public string SequenceID;
    public SequenceHandler Handler;
  }

  [SerializeField]
  private List<_SequenceMetadata> _SequenceList;

  private Dictionary<string, _SequenceMetadata> _sequence_map = new Dictionary<string, _SequenceMetadata>();


  public void Start(){
    foreach(_SequenceMetadata _metadata in _SequenceList)
      _sequence_map[_metadata.SequenceID] = _metadata;
  }


  public void StartSequenceAsync(string sequence_id){
    if(!HasSequence(sequence_id))
      return;

    _SequenceMetadata _metadata = _sequence_map[sequence_id];
    _metadata.Handler.StartTriggerAsync();
  }

  public IEnumerator StartSequence(string sequence_id){
    if(!HasSequence(sequence_id))
      yield break;

    _SequenceMetadata _metadata = _sequence_map[sequence_id];
    yield return _metadata.Handler.StartTrigger();
  }

  public bool HasSequence(string sequence_id){
    return _sequence_map.ContainsKey(sequence_id);
  }
}