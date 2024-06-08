using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;


[RequireComponent(typeof(ParticleSystem))]
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

  public void CancelEffect(){
    _particle_system.Stop();
  }


  public void OnEnable(){
    _trigger_effect();
  }

  public void OnDisable(){
    CancelEffect();
  }


  public void TriggerSetOnEnable(bool flag){
    EnableTrigger = flag;
  }
}