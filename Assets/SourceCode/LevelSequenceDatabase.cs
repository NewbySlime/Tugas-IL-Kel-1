using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Database for storing and handling sequence for certain scene.
/// This database can exist (optionally) in every scene. Since the data stored can be different each scene.
/// </summary>
public class LevelSequenceDatabase: MonoBehaviour{
  [Serializable]
  private struct _SequenceMetadata{
    public string SequenceID;
    public SequenceHandlerVS Handler;
  }

  [SerializeField]
  private List<_SequenceMetadata> _SequenceList;

  private Dictionary<string, _SequenceMetadata> _sequence_map = new Dictionary<string, _SequenceMetadata>();


  public void Start(){
    foreach(_SequenceMetadata _metadata in _SequenceList)
      _sequence_map[_metadata.SequenceID] = _metadata;
  }


  /// <summary>
  /// Start certain sequence asynchronously.
  /// </summary>
  /// <param name="sequence_id">Sequence ID to start</param>
  public void StartSequenceAsync(string sequence_id){
    if(!HasSequence(sequence_id))
      return;

    _SequenceMetadata _metadata = _sequence_map[sequence_id];
    _metadata.Handler.StartTriggerAsync();
  }

  /// <summary>
  /// Start certain sequence synchronously (controlled coroutine).
  /// </summary>
  /// <param name="sequence_id">Sequence ID to start</param>
  /// <returns>Coroutine helper object</returns>
  public IEnumerator StartSequence(string sequence_id){
    if(!HasSequence(sequence_id))
      yield break;

    _SequenceMetadata _metadata = _sequence_map[sequence_id];
    yield return _metadata.Handler.StartTrigger();
  }

  /// <summary>
  /// To check if certain sequence exist in this database
  /// </summary>
  /// <param name="sequence_id">The target sequence ID to check</param>
  /// <returns>Is the sequence exist</returns>
  public bool HasSequence(string sequence_id){
    return _sequence_map.ContainsKey(sequence_id);
  }
}