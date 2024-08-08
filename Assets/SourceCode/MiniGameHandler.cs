using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Class that handles Minigame system and as a base for inherting class.
/// The Minigame also handles sequencing supplied based on the outcomes of the conditions.
/// Since this is a base class, the rule for the outcome of the conditions controlled by the inheriting class.
/// 
/// Needed Autoload(s);
/// - <see cref="SequenceDatabase"/> needed for getting a prefab for sequencing handler.
/// </summary>
public class MiniGameHandler: MonoBehaviour{
  /// <summary>
  /// Enums used to determine which outcome it produced in certain time.
  /// </summary>
  public enum ResultCase{
    Win,
    Lose
  }

  /// <summary>
  /// Data for sequencing an outcome that it triggers.
  /// </summary>
  public struct ResultSequenceData{
    public ResultCase Case;
    public SequenceHandlerVS SequenceHandler;
  }

  private SequenceDatabase _sequence_database;

  private Dictionary<ResultCase, SequenceHandlerVS> _result_sequence_map = new();

  /// <summary>
  /// A check if the Minigame is currently running.
  /// </summary>
  /// <value></value>
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


  /// <summary>
  /// Virtual function as an event when the Minigame is finished.
  /// </summary>
  /// <param name="result">The outcome of the Minigame</param>
  protected virtual void _OnGameFinished(ResultCase result){}


  /// <summary>
  /// Function used by inheriting class to trigger the outcome or finished state.
  /// </summary>
  /// <param name="result">The outcome of the Minigame</param>
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


  /// <summary>
  /// Function used by Sequencing system to set the sequence data based on the outcome.
  /// </summary>
  /// <param name="result">The target outcome</param>
  /// <param name="data">The sequence data</param>
  public void SetResultCaseSequence(ResultCase result, SequenceHandlerVS.SequenceInitializeData data){
    StartCoroutine(_set_result_case_sequence(result, data));
  }

  
  /// <summary>
  /// Function to start the Minigame.
  /// Also used as a virtual function that the inheriting class can override to change how the Minigame should started.
  /// </summary>
  public virtual void TriggerGameStart(){
    DEBUGModeUtils.Log("mini game started");
    IsMiniGameRunning = true;
  }
}