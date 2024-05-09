using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;



public class AreaTriggerSequenceInterface: MonoBehaviour, ISequenceAsync, IAreaTriggerHandle{
  private List<ISequenceAsync> _list_sequences = new List<ISequenceAsync>();
  private List<IAreaTriggerHandle> _list_handles = new List<IAreaTriggerHandle>();


  private HashSet<ISequenceAsync> _list_not_finished = new HashSet<ISequenceAsync>();


  private void _check_finished_list(){
    List<ISequenceAsync> _list_delete = new List<ISequenceAsync>();
    foreach(ISequenceAsync _sequence in _list_not_finished){
      if(!_sequence.IsTriggering())
        _list_delete.Add(_sequence);
    }

    foreach(ISequenceAsync _delete in _list_delete)
      _list_not_finished.Remove(_delete);
  }


  public void Start(){
    ISequenceAsync[] _sequences = GetComponents<ISequenceAsync>();
    _list_sequences = _sequences.ToList();

    IAreaTriggerHandle[] _handles = GetComponents<IAreaTriggerHandle>();
    _list_handles = _handles.ToList();
  }


  public void StartTriggerAsync(){
    if(IsTriggering())
      return;

    foreach(ISequenceAsync _sequence in _list_sequences){
      _sequence.StartTriggerAsync();
      _list_not_finished.Add(_sequence);
    }
  }

  public bool IsTriggering(){
    _check_finished_list();
    return _list_not_finished.Count > 0;
  }


  public void SetContext(AreaTrigger.TriggerContext? context){
    foreach(IAreaTriggerHandle _handle in _list_handles)
      _handle.SetContext(context);
  }
}