using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;


[RequireComponent(typeof(ParticleSystem))]
/// <summary>
/// A class that triggers Unity's <b>ParticleSystem</b> when the object is being enabled.
/// 
/// This class uses Component(s);
/// - <b>ParticleSystem</b> Target object of Unity's Particle Handler for triggering. 
/// </summary>
public class OnEnabledParticleTrigger: MonoBehaviour, IEnableTrigger{
  private ParticleSystem _particle_system;

  public bool IsInitialized{private set; get;} = false;
  public bool EnableTrigger = true;


  private void _trigger_effect(){
    if(!EnableTrigger || !IsInitialized)
      return;

    _particle_system.Play();
  } 

  private IEnumerator _start_co_func(){
    yield return null;
    yield return new WaitForEndOfFrame();
    
    IsInitialized = true;
    _trigger_effect();
  }


  public void Start(){
    _particle_system = GetComponent<ParticleSystem>();

    StartCoroutine(_start_co_func());
  }

  /// <summary>
  /// Function used to stop the ongoing effect.
  /// </summary>
  public void CancelEffect(){
    _particle_system.Stop();
  }


  /// <summary>
  /// Function to catch Unity's "Enabled" event.
  /// </summary>
  public void OnEnable(){
    _trigger_effect();
  }

  /// <summary>
  /// Function to catch Unity's "Disabled" event.
  /// </summary>
  public void OnDisable(){
    CancelEffect();
  }


  public void TriggerSetOnEnable(bool flag){
    EnableTrigger = flag;
  }
}