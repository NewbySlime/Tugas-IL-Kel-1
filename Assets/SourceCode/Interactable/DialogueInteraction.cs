using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



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


  private void _popup_dialogue(DialogueUI.DialogueData data){
    _dialogue_timeout = _DialogueHideTimeout;

    TimingBaseUI.SkipAllTimer(_dialogue_shrinkui);

    _dialogue_shrinkui.DoShrink = false;
    TimingBaseUI.StartAsyncAllTimer(_dialogue_shrinkui);

    _dialogue_ui.ChangeDialogue(data, false);
  }

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


  public void InteractableInterface_Interact(){
    _is_interact_enter = true;
    TriggerDialogue();
  }

  public void InteractableInterface_InteractableExit(){
    _is_interact_enter = false;
    _hide_dialogue();
  }


  public void DialogueData_SetInitData(DialogueUI.DialogueSequence dialogue){
    _dialogue_sequence = dialogue;
  }
}