using System;
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


  private DialogueUI _dialogue_ui = null;
  private ShrinkUI _dialogue_shrinkui = null;

  private DialogueUI.DialogueSequence _dialogue_sequence;

  private GameHandler _game_handler;

  private int _dialogue_index = 0;


  private float _dialogue_timeout = 0;


  private void _popup_dialogue(DialogueUI.DialogueData data){
    _dialogue_timeout = _DialogueHideTimeout;

    TimingBaseUI.SkipAllTimer(_dialogue_shrinkui);

    _dialogue_shrinkui.DoShrink = false;
    TimingBaseUI.StartAsyncAllTimer(_dialogue_shrinkui);

    _dialogue_ui.ChangeDialogue(data, false);
  }

  private void _hide_dialogue(){
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

    if(_dialogue_index >= _dialogue_sequence.Sequence.Count && !_RandomizeDialogue){
      _hide_dialogue();
      _dialogue_index = 0;
    }
    else{
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



  public void Start(){
    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot get Game Handler.");
    }

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


  public void InteractableInterface_Interact(){
    if(_dialogue_sequence.Sequence.Count <= 0){
      Debug.LogWarning("No Dialogue to show.");
      return;
    }

    if(_RandomizeDialogue)
      _random_dialogue();
    else
      _next_dialogue();
  }

  public void InteractableInterface_InteractableExit(){
    _hide_dialogue();
  }


  public void DialogueData_SetInitData(DialogueUI.DialogueSequence dialogue){
    _dialogue_sequence = dialogue;
  }
}