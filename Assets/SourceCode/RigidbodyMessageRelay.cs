using UnityEngine;



/// <summary>
/// Komponen sebagai alat untuk mempermudah komunikasi antar komponen dengan Rigidbody yang terpisah diantara GameObject.
/// Komponen penting yang diperlukan pada Objek Game:
///   - RigidbodyX dan/atau ColliderXX
/// </summary>
public class RigidbodyMessageRelay: MonoBehaviour{
  public delegate void CollidingEventFunction(Collision collision);
  public delegate void TriggerEventFunction(Collider collider);

  public event CollidingEventFunction OnCollisionEnteredEvent;
  public event CollidingEventFunction OnCollsiionExitedEvent;
  public event TriggerEventFunction OnTriggerEnteredEvent;
  public event TriggerEventFunction OnTriggerExitedEvent;


  public void OnCollisionEnter(Collision collision){
    OnCollisionEnteredEvent?.Invoke(collision);
  }

  public void OnCollisionExit(Collision collision){
    OnCollsiionExitedEvent?.Invoke(collision);
  }

  public void OnTriggerEnter(Collider collider){
    OnTriggerEnteredEvent?.Invoke(collider);
  }

  public void OnTriggerExit(Collider collider){
    OnTriggerExitedEvent?.Invoke(collider);
  }



  public delegate void CollidingEventFunction2D(Collision2D collision);
  public delegate void TriggerEventFunction2D(Collider2D collider);

  public event CollidingEventFunction2D OnCollisionEntered2DEvent;
  public event CollidingEventFunction2D OnCollsiionExited2DEvent;
  public event TriggerEventFunction2D OnTriggerEntered2DEvent;
  public event TriggerEventFunction2D OnTriggerExited2DEvent;

  public void OnCollisionEnter2D(Collision2D collision){
    OnCollisionEntered2DEvent?.Invoke(collision);
  }

  public void OnCollisionExit2D(Collision2D collision){
    OnCollsiionExited2DEvent?.Invoke(collision);
  }

  public void OnTriggerEnter2D(Collider2D collider){
    OnTriggerEntered2DEvent?.Invoke(collider);
  }

  public void OnTriggerExit2D(Collider2D collider){
    OnTriggerExited2DEvent?.Invoke(collider);
  }
}