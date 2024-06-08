using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Komponen penting ketika akan melakukan "MiniGame" yang dimana pemakaiannya cukup luas. Seperti contoh, pada scene "base_level" ada dua "ekstensi" dari MiniGameHandler yang dipakai. Contohnya, Komponen EnemyFightMG dan BossFightMG.
// Komponen ini juga memberikan fungsi yang bisa dipakai sebagai ekstensi dari objek ini, seperti contoh memberikan fungsi menjalankan Sequence ketika ResultCase tercapai.
public class MiniGameHandler: MonoBehaviour{
  public enum ResultCase{
    Win,
    Lose
  }

  public struct ResultSequenceData{
    public ResultCase Case;
    public SequenceHandlerVS SequenceHandler;
  }

  private SequenceDatabase _sequence_database;

  private Dictionary<ResultCase, SequenceHandlerVS> _result_sequence_map = new();

  public bool IsMiniGameRunning{private set; get;} = false;


  private IEnumerator _set_result_case_sequence(ResultCase result, SequenceHandlerVS.SequenceInitializeData data){
    yield return null;
    yield return new WaitForEndOfFrame();

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

  protected virtual void _OnGameFinished(ResultCase result){}

  protected void _GameFinished(ResultCase result){
    Debug.Log("mini game finished");
    if(!IsMiniGameRunning)
      return;

    Debug.Log("mini game finished done");
    IsMiniGameRunning = false;

    _OnGameFinished(result);
    
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
    Debug.Log("mini game started");
    IsMiniGameRunning = true;
  }
}