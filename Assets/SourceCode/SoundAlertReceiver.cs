using UnityEngine;


public class SoundAlertReceiver: MonoBehaviour{
  public delegate void SoundReceived(GameObject source);
  public event SoundReceived SoundReceivedEvent;


  public void TriggerHear(GameObject source){
    SoundReceivedEvent?.Invoke(source);
  }
}