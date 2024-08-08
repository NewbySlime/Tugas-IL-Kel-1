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


[RequireComponent(typeof(AudioSource))]
/// <summary>
/// Base class for handling dialogue animation, interactions and also processing dialogue data.
/// 
/// This class uses following component(s);
/// - <b>AudioSource</b> for giving auditorial interaction feedback.
/// 
/// This class uses external component(s);
/// - <b>Unity's TMP Text</b> used for showing the dialogue.
/// 
/// This class uses prefab(s);
/// - Prefab for sequence handling (<see cref="SequenceHandler"/>).
/// 
/// </summary>
public class DialogueUI: MonoBehaviour{
  [Serializable]
  /// <summary>
  /// Dialogue data to be used with <see cref="DialogueUI"/>
  /// </summary>
  public class DialogueData{
    /// <summary>
    /// The message used for a part of dialouge.
    /// </summary>
    public string Dialogue;

    /// <summary>
    /// The list of currently talking characters (identified using character's IDs).
    /// Not used in <see cref="DialogueUI"/> but can be used by another object like <see cref="DialogueCharacterUI"/>.
    /// </summary>
    public List<string> CharactersTalking = new();

    /// <summary>
    /// Sequence data used after this part of dialogue.
    /// </summary>
    public SequenceHandlerVS.SequenceInitializeData SequenceData = null;

    /// <summary>
    /// Extended data used by <see cref="DialogueCharacterUI"/>.
    /// </summary>
    public DialogueCharacterUI.ExtendedDialogue DialogueCharacterUIData = null;
  }
  
  
  [Serializable]
  /// <summary>
  /// A list of dialogues formed by <see cref="AddDialogue"/> with using Visual Scripting.
  /// </summary>
  public class DialogueSequence{
    public List<DialogueData> Sequence = new();
  }


  [Serializable]
  /// <summary>
  /// Data used for setting each character delay.
  /// </summary>
  public class DialogueCharacterSpeed{
    /// <summary>
    /// The list of character in form of string.
    /// </summary>
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

  [SerializeField]
  private AudioClip _DialogueAudio;


  private Dictionary<char, float> _character_delay_map = new();

  private bool _dialogue_finished = true;
  private bool _skip_dialogue = false;

  private SequenceHandlerVS _sequence_handler = null;

  private AudioSource _audio_source;


  /// <summary>
  /// For getting the last character in a string with RichText format.
  /// The result excludes the characters used for the formatting of the RichText.
  /// </summary>
  /// <param name="rt_str">The RichText string to be processed</param>
  /// <returns>The last character of the string</returns>
  private char _get_last_char_in_rt(string rt_str){
    int _last_idx = rt_str.Length-1;
    while(_last_idx >= 0){
      if(rt_str[_last_idx] != '>')
        return rt_str[_last_idx];
        
      _last_idx = rt_str.LastIndexOf('<', _last_idx);
      _last_idx--;
    }

    return '\0';
  }


  /// <summary>
  /// Function to show and play dialogue animation using the supplied data.
  /// </summary>
  /// <param name="dialogue">The data to be supplied</param>
  /// <returns>Coroutine helper object</returns>
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
      
      if(_DialogueAudio != null)
        _audio_source.PlayOneShot(_DialogueAudio);

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

    _audio_source = GetComponent<AudioSource>();
    
    GameObject _test_obj = Instantiate(_SequenceHandlerPrefab);
    if(_test_obj.GetComponent<SequenceHandlerVS>() == null){
      Debug.LogError("Prefab for SequenceHandler does not have SequenceHandlerVS.");
      throw new MissingComponentException();
    }

    Destroy(_test_obj);
  }


  /// <summary>
  /// Triggers sequence handler from the recently supplied dialouge data.
  /// </summary>
  public void TriggerSequenceAsync(){
    if(_sequence_handler == null)
      return;

    DEBUGModeUtils.Log("Dialogue start trigger");
    _sequence_handler.StartTriggerAsync();
  }

  /// <summary>
  /// Function to check if current sequence handler is still playing.
  /// </summary>
  /// <returns>Is the sequence playing</returns>
  public bool IsSequenceTriggering(){
    if(_sequence_handler == null)
    return false;
    
    return _sequence_handler.IsTriggering();
  }


  /// <summary>
  /// Function to add or change the dialogue data with the new dialouge.
  /// </summary>
  /// <param name="dialogue">The new dialogue data</param>
  /// <param name="skip_dialogue">Should the dialogue animation be skipped or not</param>
  public void ChangeDialogue(DialogueData dialogue, bool skip_dialogue = false){
    if(skip_dialogue)
      _TextContainer.text = dialogue.Dialogue;
    else
      StartCoroutine(_start_dialogue(dialogue));
  }

  /// <summary>
  /// Function to check if current dialogue is still playing or not.
  /// </summary>
  /// <returns>Is current dialogue finished or not</returns>
  public bool IsDialogueFinished(){
    return _dialogue_finished;
  }

  /// <summary>
  /// Prompt the UI to skip currently playing dialouge.
  /// </summary>
  public void SkipDialogueAnimation(){
    _skip_dialogue = true;
  }
}