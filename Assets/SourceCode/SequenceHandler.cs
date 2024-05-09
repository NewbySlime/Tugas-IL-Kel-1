using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SequenceHandler: MonoBehaviour, ISequenceAsync{
  [SerializeField]
  private List<SequenceInterface> _SequenceList = new List<SequenceInterface>();

  private bool _sequence_running = false;


  public IEnumerator StartTrigger(){
    _sequence_running = true;
    for(int i = 0; i < _SequenceList.Count; i++){
      SequenceInterface _seq = _SequenceList[i];
      yield return _seq.StartTrigger();
    }

    _sequence_running = false;
  }


  public void StartTriggerAsync(){
    StartCoroutine(StartTrigger());
  }

  public bool IsTriggering(){
    return _sequence_running;
  }
}