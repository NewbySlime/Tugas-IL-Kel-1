using UnityEngine;


public class FadeUI: MonoBehaviour, ISequenceAsync{
  [SerializeField]
  private SpriteRenderer _SpriteRenderer;

  [SerializeField]
  private float _FadeTiming;

  [SerializeField]
  private bool _FadeActive;


  private float _target_fade_value = 0;
  private float _from_fade_value = 0;

  private float _current_value = 0;
  private float _current_fade_speed = 0;


  public void Start(){
    if(_SpriteRenderer == null){
      Debug.LogWarning("Sprite Renderer hasn't been assigned.");
      return;
    }
  }


  public void Update(){
    float _next_value = _target_fade_value;
    if(IsTriggering()){
      Mathf.SmoothDamp(_current_value, _target_fade_value, ref _current_fade_speed, _FadeTiming);

      _next_value = _current_value;
    }
    
    Color _new_alpha = _SpriteRenderer.color;
    _new_alpha.a = _next_value;

    _SpriteRenderer.color = _new_alpha;
  }


  public void StartTriggerAsync(){
    if(_FadeActive){
      _target_fade_value = 1;
      _from_fade_value = 0;
    }
    else{
      _target_fade_value = 0;
      _from_fade_value = 1;
    }

    _current_value = _from_fade_value;
    _current_fade_speed = 0;
  }

  public bool IsTriggering(){
    float _value = Mathf.Abs(_target_fade_value-_current_fade_speed);
    return _value < 0.05;
  }
}