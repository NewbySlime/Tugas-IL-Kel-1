using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// A class that handles interaction event with dialogue bubble. See <see cref="InteractionHandler"/> for interaction handling.
/// 
/// This class uses Component(s);
/// - <see cref="DialogueUI"/> in <b>DialogueBubblePrefab</b> for Dialogue presentation handling.
/// - <see cref="ShrinkUI"/> in <b>DialogueBubblePrefab</b> for "show" and "hide" effect to the UI.
/// 
/// Interlinked Component(s);
/// - <see cref="InteractionHandler"/> uses the class events for processing interaction for Dialogue.
/// </summary>
public class DialogueInteraction: MonoBehaviour{
  [SerializeField]
  private GameObject _DialogueBubblePrefab;

  [SerializeField]
  private GameObject _DialogueBubbleContainer;

  [SerializeField]
  private bool _RandomizeDialogue = false;
  
  [SerializeField]
  private bool _ResetOnHide = false;

  [SerializeField]
  private float _DialogueHideTimeout = 5f;

  [SerializeField]
  private bool _LoopDialogue = false;

  [SerializeField]
  private bool _AutoResume = false;
  [SerializeField]
  private float _AutoResumeDelay = 3;


  private DialogueUI _dialogue_ui = null;
  private ShrinkUI _dialogue_shrinkui = null;

  private DialogueUI.DialogueSequence _dialogue_sequence;

  private GameHandler _game_handler;

  private int _dialogue_index = 0;
  private float _dialogue_timeout = 0;

  private Coroutine _auto_resume_coroutine = null;
  private float _auto_resume_timer;

  private bool _is_interact_enter = false;

  /// <summary>
  /// Handles effect when showing the UI. Also handles Dialogue presentation later on with the supplied data.
  /// </summary>
  /// <param name="data">The dialogue data to use</param>
  private void _popup_dialogue(DialogueUI.DialogueData data){
    _dialogue_timeout = _DialogueHideTimeout;

    TimingBaseUI.SkipAllTimer(_dialogue_shrinkui);

    _dialogue_shrinkui.DoShrink = false;
    TimingBaseUI.StartAsyncAllTimer(_dialogue_shrinkui);

    _dialogue_ui.ChangeDialogue(data, false);
  }

  /// <summary>
  /// Handles effect when hiding the UI.
  /// </summary>
  private void _hide_dialogue(){
    if(_game_handler == null || !_game_handler.SceneInitialized)
      return;

    if(_auto_resume_coroutine != null){
      StopCoroutine(_auto_resume_coroutine);
      _auto_resume_coroutine = null;
    }

    if(!_dialogue_shrinkui.DoShrink){
      TimingBaseUI.SkipAllTimer(_dialogue_shrinkui);

      _dialogue_shrinkui.DoShrink = true;
      TimingBaseUI.StartAsyncAllTimer(_dialogue_shrinkui);
    }

    if(_ResetOnHide && !_RandomizeDialogue)
      _dialogue_index = 0;
  }


  /// <summary>
  /// Function for when the class wants to skip the dialogue or proceed with the next Dialogue.
  /// </summary>
  private void _next_dialogue(){
    if(!_dialogue_ui.IsDialogueFinished()){
      _dialogue_ui.SkipDialogueAnimation();
      return;
    }

    bool _do_dialogue = true;
    if(_dialogue_index >= _dialogue_sequence.Sequence.Count){
      _dialogue_index = 0;
      
      if(!_LoopDialogue){
        _hide_dialogue();
        _do_dialogue = false;
      }
    }

    if(_do_dialogue){
      _popup_dialogue(_dialogue_sequence.Sequence[_dialogue_index]);
      _dialogue_index++;
    }
  }

  /// <summary>
  /// Works the same as _next_dialogue() but it picks the next Dialogue randomly.
  /// </summary>
  private void _random_dialogue(){
    if(_dialogue_ui.IsDialogueFinished()){
      int _last_dialogue_index = _dialogue_index;
      if(_dialogue_sequence.Sequence.Count > 1){
        _dialogue_index = _last_dialogue_index;
        while(_last_dialogue_index == _dialogue_index)
          _dialogue_index = UnityEngine.Random.Range(0, _dialogue_sequence.Sequence.Count);
      }
      else
        _dialogue_index = 0;

      _popup_dialogue(_dialogue_sequence.Sequence[_dialogue_index]);
    }
    else
      _dialogue_ui.SkipDialogueAnimation();
  }


  /// <summary>
  /// Coroutine function to handle auto resuming the Dialogue with timings to be used as a timer.
  /// </summary>
  /// <returns>Coroutine helper object</returns>
  private IEnumerator _auto_resume_co_func(){
    while(true){
      if(!_dialogue_ui.IsDialogueFinished()){
        yield return null;
        continue;
      }

      if(_auto_resume_timer < 0){
        _auto_resume_timer = _AutoResumeDelay;
        _next_dialogue();
      }

      yield return null;
      _auto_resume_timer -= Time.deltaTime;
    }
  }


  private void _on_scene_initialized(string scene_id, GameHandler.GameContext context){
    // in case if an object already touched the Interaction border when the level is starting 
    TriggerDialogue();
  }

  private void _on_scene_removed(){
    _game_handler.SceneChangedFinishedEvent -= _on_scene_initialized;
    _game_handler.SceneRemovingEvent -= _on_scene_removed;
  }



  public void Start(){
    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot get Game Handler.");
    }

    _game_handler.SceneChangedFinishedEvent += _on_scene_initialized;
    _game_handler.SceneRemovingEvent += _on_scene_removed;

    GameObject _dialogue_bubble = Instantiate(_DialogueBubblePrefab);
    _dialogue_bubble.transform.SetParent(_DialogueBubbleContainer.transform);
    _dialogue_bubble.transform.localPosition = Vector2.zero;

    _dialogue_ui = _dialogue_bubble.GetComponent<DialogueUI>();
    if(_dialogue_ui == null)
      throw new MissingComponentException("Dialogue Bubble doesn't have DialogueUI.");

    _dialogue_shrinkui = _dialogue_bubble.GetComponent<ShrinkUI>();
    if(_dialogue_shrinkui == null)  
      throw new MissingComponentException("Dialogue Bubble doesn't have ShrinkUI.");
  }

  public void Update(){
    if(_dialogue_ui.IsDialogueFinished() && _dialogue_timeout > 0){
      _dialogue_timeout -= Time.deltaTime;
      if(_dialogue_timeout <= 0)
        _hide_dialogue();
    }
  }

  
  /// <summary>
  /// Function to start the dialogue or skip/next the dialogue. This function also handles the showing/hiding when appropriate.
  /// </summary>
  public void TriggerDialogue(){
    if(!gameObject.activeInHierarchy || _game_handler == null || !_game_handler.SceneInitialized || !_is_interact_enter)
      return;

    if(_dialogue_sequence.Sequence.Count <= 0){
      Debug.LogWarning("No Dialogue to show.");
      return;
    }

    if(_RandomizeDialogue)
      _random_dialogue();
    else
      _next_dialogue();

    if(_AutoResume && _auto_resume_coroutine == null){
      _auto_resume_coroutine = StartCoroutine(_auto_resume_co_func());
      _auto_resume_timer = _AutoResumeDelay;
    }
  }


  /// <summary>
  /// Interface function used by <see cref="InteractableInterface"/> for when an "Interact" event is triggered. 
  /// </summary>
  public void InteractableInterface_Interact(){
    _is_interact_enter = true;
    TriggerDialogue();
  }

  /// <summary>
  /// Interface function used by <see cref="InteractableInterface"/> for when an "InteractableExit" event is triggered, which means the Interaction border no longer register the entered object.
  /// </summary>
  public void InteractableInterface_InteractableExit(){
    _is_interact_enter = false;
    _hide_dialogue();
  }

  /// <summary>
  /// Interface function used by <see cref="SetDialogueToGameObject"/> Sequence Node to pass the data created in the Unity Visual Script. 
  /// </summary>
  /// <param name="dialogue"></param>
  public void DialogueData_SetInitData(DialogueUI.DialogueSequence dialogue){
    _dialogue_sequence = dialogue;
  }
}