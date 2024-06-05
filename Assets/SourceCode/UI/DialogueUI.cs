using System;
using UnityEngine;
using RichTextSubstringHelper;
using System.Collections;
using TMPro;
using System.Numerics;
using System.IO.Compression;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using System.Linq;


public class DialogueUI: MonoBehaviour{
  [Serializable]
  public class DialogueData{
    public string Dialogue;
    public List<string> CharactersTalking = new();

    public SequenceHandlerVS.SequenceInitializeData SequenceData = null;

    public DialogueCharacterUI.ExtendedDialogue DialogueCharacterUIData = null;
  }
  
  [Serializable]
  public class DialogueSequence{
    public List<DialogueData> Sequence = new();
  }


  [Serializable]
  public class DialogueCharacterSpeed{
    public string Characters;
    public float Delay;
  }

  
  [SerializeField]
  private TMP_Text _TextContainer;

  [SerializeField]
  private float _DefaultPerCharacterDelay = 0.1f;

  [SerializeField]
  private List<DialogueCharacterSpeed> _CharacterDelayList;

  [SerializeField]
  private bool _UseScaledTiming = true;

  [SerializeField]
  private GameObject _SequenceHandlerPrefab;


  private Dictionary<char, float> _character_delay_map = new();

  private bool _dialogue_finished = true;
  private bool _skip_dialogue = false;

  private SequenceHandlerVS _sequence_handler = null;


  private char _get_last_char_in_rt(string rt_str){
    int _last_idx = rt_str.Length-1;
    while(_last_idx >= 0){
      if(rt_str[_last_idx] != '>')
        return rt_str[_last_idx];

      _last_idx = rt_str.LastIndexOf('<', 0, _last_idx+1);
      _last_idx--;
    }

    return '\0';
  }

  private IEnumerator _start_dialogue(DialogueData dialogue){
    _dialogue_finished = false;
    _skip_dialogue = false;

    if(_SequenceHandlerPrefab != null){
      if(_sequence_handler != null){
        Destroy(_sequence_handler.gameObject);
        _sequence_handler = null;
      }

      GameObject _seq_obj = Instantiate(_SequenceHandlerPrefab);
      yield return new WaitForNextFrameUnit();

      _sequence_handler = _seq_obj.GetComponent<SequenceHandlerVS>();
      _sequence_handler.SetInitData(dialogue.SequenceData);
      yield return new WaitForNextFrameUnit();
    }

    string _rb_str = dialogue.Dialogue;
    for(int i = 0; i < _rb_str.RichTextLength(); i++){
      if(_skip_dialogue){
        _TextContainer.text = _rb_str;
        break;
      }

      float _timer = _DefaultPerCharacterDelay;

      _TextContainer.text = _rb_str.RichTextSubString(i+1);

      char _last_char = _get_last_char_in_rt(_TextContainer.text);
      if(_character_delay_map.ContainsKey(_last_char))
        _timer = _character_delay_map[_last_char];

      if(_UseScaledTiming)
        yield return new WaitForSeconds(_timer);
      else
        yield return new WaitForSecondsRealtime(_timer);
    }

    _dialogue_finished = true;
  }


  public void Start(){
    _TextContainer.text = "";

    foreach(DialogueCharacterSpeed char_data in _CharacterDelayList){
      foreach(char c in char_data.Characters)
        _character_delay_map[c] = char_data.Delay;
    }

    if(_SequenceHandlerPrefab == null){
      Debug.LogWarning("Prefab for SequenceHandler is null.");
      return;
    }
    
    GameObject _test_obj = Instantiate(_SequenceHandlerPrefab);
    if(_test_obj.GetComponent<SequenceHandlerVS>() == null){
      Debug.LogError("Prefab for SequenceHandler does not have SequenceHandlerVS.");
      throw new MissingComponentException();
    }

    Destroy(_test_obj);
  }


  public void TriggerSequenceAsync(){
    if(_sequence_handler == null)
      return;

    Debug.Log("Dialogue start trigger");
    _sequence_handler.StartTriggerAsync();
  }

  public bool IsSequenceTriggering(){
    if(_sequence_handler == null)
    return false;
    
    return _sequence_handler.IsTriggering();
  }


  public void ChangeDialogue(DialogueData dialogue, bool skip_dialogue = false){
    if(skip_dialogue)
      _TextContainer.text = dialogue.Dialogue;
    else
      StartCoroutine(_start_dialogue(dialogue));
  }

  public bool IsDialogueFinished(){
    return _dialogue_finished;
  }

  public void SkipDialogueAnimation(){
    _skip_dialogue = true;
  }
}