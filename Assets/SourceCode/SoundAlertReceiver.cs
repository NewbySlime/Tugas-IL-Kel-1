using UnityEngine;

/// <summary>
/// Class to "receive sound" transmitted by <see cref="SoundAlertTransceiver"/>. Another component/object can use the events use in this class to receive the "sound" transmitted from a source.
/// For further explanation, see <b>Reference/Diagrams/SoundAlert.drawio</b>
/// 
/// NOTE: this will not actually capture audio from the Unity's Sound system, instead it uses "imaginary sound" in a form of area of effect to trigger "hearing" events.
/// 
/// <seealso cref="SoundAlertTransceiver"/>
/// </summary>
public class SoundAlertReceiver: MonoBehaviour{
  /// <summary>
  /// Event for when an "imaginary sound" has been received.
  /// </summary>
  public event SoundReceived SoundReceivedEvent;
  public delegate void SoundReceived(GameObject source);


  /// <summary>
  /// Trigger a "sound received" event.
  /// </summary>
  /// <param name="source">The source of the transmitter</param>
  public void TriggerHear(GameObject source){
    SoundReceivedEvent?.Invoke(source);
  }
}