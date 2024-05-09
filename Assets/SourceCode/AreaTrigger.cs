using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;



public class AreaTrigger: MonoBehaviour{
  public struct TriggerContext{
    public GameObject EnteredObject;
  }


  [SerializeField]
  private List<AreaTriggerSequenceInterface> _TriggerSequence;

  [SerializeField]
  private RigidbodyMessageRelay _Rigidbody;

  public bool TriggerOnEnter = true;


  private IEnumerator _start_sequence(TriggerContext context){
    foreach(AreaTriggerSequenceInterface _interface in _TriggerSequence){
      _interface.SetContext(context);
      _interface.StartTriggerAsync();

      yield return new WaitUntil(() => !_interface.IsTriggering());
      _interface.SetContext(null);
    }
  }

  protected virtual void _OnObjectEnter(Collider2D collider){
    Debug.LogWarning("Area Entered");
    if(!TriggerOnEnter)
      return;

    TriggerContext _context = new TriggerContext{
      EnteredObject = collider.gameObject
    };

    StartCoroutine(_start_sequence(_context));
  }


  public void Start(){
    Debug.Log("Area trigger starting.");
    _Rigidbody.OnTriggerEntered2DEvent += _OnObjectEnter;
  }
}