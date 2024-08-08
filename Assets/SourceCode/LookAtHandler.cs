using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Component for covering <see cref="ILookAtReceiver"/> interface functionality for handling another associated <see cref="ILookAtReceiver"/> to this component. In other words, it will re-transmit the interface functions to the associated object.
/// </summary>
public class LookAtHandler: MonoBehaviour, ILookAtReceiver{
  [SerializeField]
  private List<ILookAtReceiver> _LookAtList;


  public void LookAt(Vector2 direction){
    foreach(ILookAtReceiver _receiver in _LookAtList)
      _receiver.LookAt(direction);
  }
}