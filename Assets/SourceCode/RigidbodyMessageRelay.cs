using UnityEngine;



/// <summary>
/// Component to relaying/retransmit any physics events from Unity's event system to C# event system.
/// </summary>
public class RigidbodyMessageRelay: MonoBehaviour{
  // MARK: 3D Physics Objects

  public delegate void CollidingEventFunction(Collision collision);
  public delegate void TriggerEventFunction(Collider collider);

  /// <summary>
  /// Event for when a collision happened.
  /// </summary>
  public event CollidingEventFunction OnCollisionEnteredEvent;
  /// <summary>
  /// Event for when an object is no longer colliding
  /// </summary>
  public event CollidingEventFunction OnCollisionExitedEvent;
  /// <summary>
  /// Event for when a physics object has entered this object's body (in trigger body context).
  /// </summary>
  public event TriggerEventFunction OnTriggerEnteredEvent;
  /// <summary>
  /// Event for when a physics object has exited this object's body (in trigger body context).
  /// </summary>
  public event TriggerEventFunction OnTriggerExitedEvent;


  public void OnCollisionEnter(Collision collision){
    OnCollisionEnteredEvent?.Invoke(collision);
  }

  public void OnCollisionExit(Collision collision){
    OnCollisionExitedEvent?.Invoke(collision);
  }

  public void OnTriggerEnter(Collider collider){
    OnTriggerEnteredEvent?.Invoke(collider);
  }

  public void OnTriggerExit(Collider collider){
    OnTriggerExitedEvent?.Invoke(collider);
  }



  // MARK: 2D Physics Objects
  
  public delegate void CollidingEventFunction2D(Collision2D collision);
  public delegate void TriggerEventFunction2D(Collider2D collider);

  /// <summary>
  /// (For 2D space) <inheritdoc cref="OnCollisionEnteredEvent"/>
  /// </summary>
  public event CollidingEventFunction2D OnCollisionEntered2DEvent;
  /// <summary>
  /// (For 2D space) <inheritdoc cref="OnCollisionExitedEvent"/>
  /// </summary>
  public event CollidingEventFunction2D OnCollisionExited2DEvent;
  /// <summary>
  /// (For 2D space) <inheritdoc cref="OnTriggerEnteredEvent"/>
  /// </summary>
  public event TriggerEventFunction2D OnTriggerEntered2DEvent;
  /// <summary>
  /// (For 2D space) <inheritdoc cref="OnTriggerExitedEvent"/>
  /// </summary>
  public event TriggerEventFunction2D OnTriggerExited2DEvent;

  public void OnCollisionEnter2D(Collision2D collision){
    OnCollisionEntered2DEvent?.Invoke(collision);
  }

  public void OnCollisionExit2D(Collision2D collision){
    OnCollisionExited2DEvent?.Invoke(collision);
  }

  public void OnTriggerEnter2D(Collider2D collider){
    OnTriggerEntered2DEvent?.Invoke(collider);
  }

  public void OnTriggerExit2D(Collider2D collider){
    OnTriggerExited2DEvent?.Invoke(collider);
  }
}