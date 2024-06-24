using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Komponen penting ketika akan melakukan "MiniGame" yang dimana pemakaiannya cukup luas. Seperti contoh, pada scene "base_level" ada dua "ekstensi" dari MiniGameHandler yang dipakai. Contohnya, Komponen EnemyFightMG dan BossFightMG.
// Komponen ini juga memberikan fungsi yang bisa dipakai sebagai ekstensi dari objek ini, seperti contoh memberikan fungsi menjalankan Sequence ketika ResultCase tercapai.
public class MiniGameHandler: MonoBehaviour{
  // Enum yang dipakai untuk menentukan apa ResultCase dari akhir MiniGame tersebut.
  public enum ResultCase{
    Win,
    Lose
  }

  // Ini Sequence data yang digunakan ketika ResultCase tercapai.
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

  // PENTING: ini adalah fungsi yang dipakai sebagai Callback untuk ekstensi kelas.
  protected virtual void _OnGameFinished(ResultCase result){}


  // PENTING: ini fungsi yang dipakai ketika komponen ekstensi memberi tahu bahwa "Game ini sudah selesai, dan hasilnya: X".
  protected void _GameFinished(ResultCase result){
    DEBUGModeUtils.Log("mini game finished");
    if(!IsMiniGameRunning)
      return;

    DEBUGModeUtils.Log("mini game finished done");
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


  // Fungsi yang dipakai di Visual Scripting untuk menambahakan Sequence saat terjadi setelah MiniGame selesai dengan ResultCase tertentu.
  public void SetResultCaseSequence(ResultCase result, SequenceHandlerVS.SequenceInitializeData data){
    StartCoroutine(_set_result_case_sequence(result, data));
  }

  
  // PENTING: Funsgi yang dipakai oleh MiniGame ataupun komponen ekstensi ketika ada objek atau actor yang menjalankan/mentrigger MiniGame ini.
  public virtual void TriggerGameStart(){
    DEBUGModeUtils.Log("mini game started");
    IsMiniGameRunning = true;
  }
}