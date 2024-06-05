using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class only serve as an interface for creating minigame
// base class also handle what to do when losing or winning
// inheriting class should handle the conditional win or lose case of the minigame

public class MiniGameHandler: MonoBehaviour{
  public enum ResultCase{
    Win,
    Lose
  }
  
  private SequenceDatabase _sequence_database;

  private Dictionary<ResultCase, SequenceHandlerVS> _result_sequence_map = new();


  private IEnumerator _set_result_case_sequence(ResultCase result, SequenceHandlerVS.SequenceInitializeData data){
    if(_result_sequence_map.ContainsKey(result)){
      SequenceHandlerVS _handler = _result_sequence_map[result];
      Destroy(_handler.gameObject);
    }

    GameObject _handler_obj = Instantiate(_sequence_database.GetSequenceHandlerBasePrefab());
    _handler_obj.transform.SetParent(transform);
    yield return null;

    SequenceHandlerVS _sequence_handler = _handler_obj.GetComponent<SequenceHandlerVS>();
    _sequence_handler.SetInitData(data);

    _result_sequence_map[result] = _sequence_handler;
  }


  protected void _GameFinished(ResultCase result){
    if(!_result_sequence_map.ContainsKey(result)){
      Debug.LogWarning(string.Format("No Sequence for ResultCase: {0}.", result));
      return;
    }

    SequenceHandlerVS _result_sequence = _result_sequence_map[result];
    _result_sequence.StartTriggerAsync();
  }


  public void Start(){
    _sequence_database = FindAnyObjectByType<SequenceDatabase>();
    if(_sequence_database == null){
      Debug.LogError("Cannot find SequenceDatabase.");
      throw new MissingReferenceException();
    }
  }


  public void SetResultCaseSequence(ResultCase result, SequenceHandlerVS.SequenceInitializeData data){
    StartCoroutine(_set_result_case_sequence(result, data));
  }

  public virtual void TriggerGameStart(){
    Debug.LogWarning("MiniGame Start is not yet implemented?");
  }
}