using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// Class that handle any inputs coming from user and redirects it to trigger sequence in <see cref="SequenceHandlerVS"/> of this class.
/// Not only redirects it to trigger sequence, the interaction can also be used to skip the sequence by using a sequence for "finished" state.
/// NOTE: this class uses input focus context at the start of the scene with "UI" privilege, above "Player" privilege. So it is better to not use this in a scene with an object that has "Player" privilege.
/// 
/// This class uses external component(s);
/// - <see cref="SequenceHandlerVS"/> for "starting" and "finished" state sequence.
/// 
/// This class uses autoload(s);
/// - <see cref="GameHandler"/> for game events and such.
/// - <see cref="InputFocusContext"/> for asking focus for input.
/// </summary>
public class AnyInputTriggerSequence: MonoBehaviour{
  [SerializeField]
  private SequenceHandlerVS _StartSequence;

  [SerializeField]
  private SequenceHandlerVS _FinishingSequence;

  [SerializeField]
  private float _SkipDelay = 0.5f;

  private GameHandler _game_handler;

  private InputFocusContext _input_context;


  private bool _finished = false;
  private bool _triggered = false;
  private bool _allow_skip = false;

  private bool _trigger_skip = false;


  
  // Coroutine function for delaying until giving a valid use of skipping to a flag.
  private IEnumerator _skip_delay(){
    _allow_skip = false;

    yield return new WaitForSeconds(_SkipDelay);

    _allow_skip = true;
  }

  // Coroutine function to handle triggering sequences used in this class.
  // This class also handles a "skipping" function by setting _trigger_skip flag to true.
  private IEnumerator _trigger_start(){
    _triggered = true;
    _trigger_skip = false;

    StartCoroutine(_skip_delay());

    _StartSequence.StartTriggerAsync();
    yield return new WaitUntil(() => !_StartSequence.IsTriggering() || _trigger_skip);

    if(_StartSequence.IsTriggering())
      _StartSequence.StopAllCoroutines();

    _FinishingSequence.StartTriggerAsync();

    _finished = true;
    _input_context.RemoveInputObject(this, InputFocusContext.ContextEnum.UI);
  }


  private void _on_scene_changed(string scene_id, GameHandler.GameContext context){
    _input_context.RegisterInputObject(this, InputFocusContext.ContextEnum.UI);
  }

  private void _on_scene_removing(){
    _input_context.RemoveInputObject(this, InputFocusContext.ContextEnum.UI);

    _game_handler.SceneChangedFinishedEvent -= _on_scene_changed;
    _game_handler.SceneRemovingEvent -= _on_scene_removing;
  }


  public void Start(){
    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingReferenceException();
    }

    _game_handler.SceneChangedFinishedEvent += _on_scene_changed;
    _game_handler.SceneRemovingEvent += _on_scene_removing;

    _input_context = FindAnyObjectByType<InputFocusContext>();
    if(_input_context == null){
      Debug.LogError("Cannot find InputFocusContext.");
      throw new MissingReferenceException();
    }
  }


  /// <summary>
  /// Function to catch "AnyKeyPressed" input event.
  /// </summary>
  /// <param name="value">Unity's input data</param>
  public void OnAnyKeyPressed(InputValue value){
    if(_triggered || !_input_context.InputAvailable(this))
      return;

    if(value.isPressed)
      StartCoroutine(_trigger_start());
  }

  /// <summary>
  /// Function to catch "SkipKeyPressed" input event.
  /// </summary>
  /// <param name="value">Unity's input data</param>
  public void OnSkipKeyPressed(InputValue value){
    if(_finished || !_allow_skip || !_input_context.InputAvailable(this))
      return;

    if(value.isPressed)
      _trigger_skip = true;
  }
}