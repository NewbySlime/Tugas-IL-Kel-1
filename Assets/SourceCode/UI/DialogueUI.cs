using System;
using UnityEngine;
using RichTextSubstringHelper;
using System.Collections;
using TMPro;
using System.Numerics;
using System.IO.Compression;
using System.Collections.Generic;


public class DialogueUI: MonoBehaviour{
  [Serializable]
  public class DialogueData{
    [Serializable]
    public class DialogueChoice{
      public List<DialogueData> NextDialogues;
    }


    public string Dialogue;

    public List<DialogueChoice> ChoicesList;

    public GameObject TriggerStartObject;
    public GameObject TriggerEndObject;
  }

  
  [SerializeField]
  private TextMeshPro _TextContainer;

  [SerializeField]
  private GameObject _DialoguePivot;
  
  [SerializeField]
  private float _HideAnimationTime = 3f;

  [SerializeField]
  private float _PerCharacterSpeed = 0.1f;

  
  private float _scale_value = 1;
  private float _scale_speed = 0;
  private bool _do_hiding = false;
  private bool _is_hide = false;

  private bool _dialogue_finished = true;
  private bool _skip_dialogue = false;

  private UnityEngine.Vector2 _pivot_base_scale = default;


  private IEnumerator _start_dialogue(DialogueData dialogue){
    _dialogue_finished = false;
    _skip_dialogue = false;

    string _rb_str = dialogue.Dialogue;
    for(int i = 0; i < _rb_str.RichTextLength(); i++){
      if(_skip_dialogue){
        _TextContainer.text = _rb_str;
        break;
      }

      _TextContainer.text = _rb_str.RichTextSubString(i+1);
      yield return new WaitForSeconds(_PerCharacterSpeed);
    }

    _dialogue_finished = true;
  }


  public void Start(){
    _TextContainer.text = "";

    _pivot_base_scale = _DialoguePivot.transform.localScale;
    _DialoguePivot.transform.localScale = UnityEngine.Vector3.zero;
    _scale_value = 0;
  }

  public void Update(){
    if(_do_hiding){
      float _target;
      if(_is_hide)
        _target = 0;
      else
        _target = 1;

      float _result = Mathf.SmoothDamp(
        _scale_value,
        _target,
        ref _scale_speed,
        _HideAnimationTime
      );

      if(Mathf.Abs(_result-_target) < 0.001){
        _do_hiding = false;
        _result = _target;
      }

      _DialoguePivot.transform.localScale = _pivot_base_scale * _result;
      _scale_value = _result;
    }
  }


  public void ChangeDialogue(DialogueData dialogue, bool skip_dialogue){
    if(skip_dialogue){
      _TextContainer.text = dialogue.Dialogue;
    }
    else
      StartCoroutine(_start_dialogue(dialogue));
  }

  public bool IsDialogueFinished(){
    return _dialogue_finished;
  }

  public void SkipDialogueAnimation(){
    _skip_dialogue = true;
  }


  public void ShowDialogue(){
    _do_hiding = true;
    _is_hide = false;
  }

  public void HideDialogue(){
    _do_hiding = true;
    _is_hide = true;
  }
}