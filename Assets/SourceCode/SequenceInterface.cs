using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SequenceInterface: MonoBehaviour{
  private HashSet<ISequenceAsync> _currently_played_sequence = new HashSet<ISequenceAsync>();

  private bool _check_sequences(){
    List<ISequenceAsync> _done_seq_list = new List<ISequenceAsync>();
    foreach(ISequenceAsync seq in _currently_played_sequence){
      if(!seq.IsTriggering())
        _done_seq_list.Add(seq);
    }

    foreach(ISequenceAsync seq in _done_seq_list)
      _currently_played_sequence.Remove(seq);
    
    return _currently_played_sequence.Count <= 0;
  }


  public IEnumerator StartTrigger(){
    if(IsTriggering())
      yield break;

    ISequenceAsync[] sequences = GetComponents<ISequenceAsync>();
    foreach(ISequenceAsync seq in sequences){
      _currently_played_sequence.Add(seq);

      seq.StartTriggerAsync();
    }

    yield return new WaitUntil(_check_sequences);
  }

  public bool IsTriggering(){
    return _currently_played_sequence.Count > 0;
  }
}