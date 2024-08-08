using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Class as a part of Sequencing system in which this class handles subsequences that contains in a part of sequence data. All subsequences contained in this class will be triggered simultaneously.
/// All subsequence objects are stored as a child object of this component.
/// <see cref="SequenceHandlerVS"/> uses this class as a way to split each subsequence data and handles it in this class.
/// </summary>
public class SequenceInterface: MonoBehaviour{
  private HashSet<ISequenceAsync> _currently_played_sequence = new HashSet<ISequenceAsync>();

  // Check all subsequences if they are still running.
  private bool _check_sequences(){
    List<ISequenceAsync> _done_seq_list = new List<ISequenceAsync>();
    foreach(ISequenceAsync seq in _currently_played_sequence){
      if(!seq.IsTriggering())
        _done_seq_list.Add(seq);
    }

    foreach(ISequenceAsync seq in _done_seq_list){
      Component _comp_obj = (Component)seq;
      DEBUGModeUtils.Log(string.Format("Sequence Finished {0}", _comp_obj.transform));

      _currently_played_sequence.Remove(seq);
    }
    
    return _currently_played_sequence.Count <= 0;
  }


  /// <summary>
  /// Start all subsequences simultaneously (synchronous).
  /// </summary>
  /// <returns>Coroutine helper object</returns>
  public IEnumerator StartTrigger(){
    if(IsTriggering())
      yield break;

    DEBUGModeUtils.Log(string.Format("SequenceInterface start trigger {0}", ObjectUtility.GetObjHierarchyPath(transform)));
    {ISequenceAsync[] sequences = GetComponents<ISequenceAsync>();
      foreach(ISequenceAsync seq in sequences){
        _currently_played_sequence.Add(seq);

        seq.StartTriggerAsync();
      }
    }

    {ISequenceAsync[] sequences = transform.GetComponentsInChildren<ISequenceAsync>();
      foreach(ISequenceAsync seq in sequences){
        Component _comp_obj = (Component)seq;
        DEBUGModeUtils.Log(string.Format("Starting Sequence {0}", _comp_obj.transform));

        _currently_played_sequence.Add(seq);

        seq.StartTriggerAsync();
      }
    }

    yield return new WaitUntil(_check_sequences);
    DEBUGModeUtils.Log(string.Format("SequenceInterface finish trigger {0}", ObjectUtility.GetObjHierarchyPath(transform)));
  }

  /// <summary>
  /// Check if all subsequences are still running or not.
  /// </summary>
  /// <returns>Is the subsequences still running</returns>
  public bool IsTriggering(){
    return _currently_played_sequence.Count > 0;
  }
}