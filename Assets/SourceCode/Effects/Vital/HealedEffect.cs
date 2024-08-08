using System.Collections;
using UnityEngine;


[RequireComponent(typeof(HealthComponent))]
/// <summary>
/// An extended <see cref="HealthComponent"/> feature that will give visual feedback to the object when the used character are "healed".
/// 
/// This class uses Component(s);
/// - <b>SpriteRenderer</b> The target renderer to manipulate the visual.
/// </summary>
public class HealedEffect: MonoBehaviour{
  [SerializeField]
  private SpriteRenderer _TargetSpriteManipulation;
  
  [SerializeField]
  private Color _StartEffectColor;

  private HealthComponent _health_component;

  private Coroutine _effect_coroutine = null;

  public float TimeEffect;


  /// <summary>
  /// Coroutine function that handles "healed" effect lifetime.
  /// </summary>
  /// <returns>Coroutine helper object</returns>
  private IEnumerator _on_healed(){
    float _timer = TimeEffect;
    while(_timer > 0){
      float _val = _timer/TimeEffect;
      _TargetSpriteManipulation.material.SetColor("_ColorAdditiveAfter", Color.Lerp(new Color(0,0,0,0), _StartEffectColor, _val));

      yield return null;

      _timer -= Time.deltaTime;
    }

    _on_effect_finished();
  }

  /// <summary>
  /// Resets everything to a finished state to completely to make sure that everything is in finished state.
  /// This is used when the effect is finished or an actor wants to cancel the effect.
  /// </summary>
  private void _on_effect_finished(){
    _TargetSpriteManipulation.material.SetColor("_ColorAdditiveAfter", new Color(0,0,0,0));

    _effect_coroutine = null;
  }


  private void _on_healed_async(int heal_points){
    if(_effect_coroutine != null)
      return;

    _effect_coroutine = StartCoroutine(_on_healed());
  }

  private void _on_damaged(int damage_points){
    CancelEffect();
  }


  public void Start(){
    _health_component = GetComponent<HealthComponent>();
    _health_component.OnDamagedEvent += _on_damaged;
    _health_component.OnHealedEvent += _on_healed_async;
  }


  /// <summary>
  /// Function used to stop the ongoing effect.
  /// </summary>
  public void CancelEffect(){
    if(_effect_coroutine == null)
      return;

    StopCoroutine(_effect_coroutine);
    _on_effect_finished();
  }
}