using System.Collections.Generic;
using UnityEngine;


public class LookAtHandler: MonoBehaviour, ILookAtReceiver{
  [SerializeField]
  private List<ILookAtReceiver> _LookAtList;


  public void LookAt(Vector2 direction){
    foreach(ILookAtReceiver _receiver in _LookAtList)
      _receiver.LookAt(direction);
  }
}