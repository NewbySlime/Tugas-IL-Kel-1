using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


public class DialogueCharacterUI: MonoBehaviour{
  public class ExtendedDialogue{
    public struct CharacterInitData{
      public string CharacterID;
      public ShowLayoutPosition UIPosition;
    }

    public struct CharacterRemoveData{
      public string CharacterID;
      public bool SkipAnimation;
    }

    public List<CharacterInitData> AddedCharacters = new();
    public List<CharacterRemoveData> RemovedCharacters = new();
  }

  public enum ShowLayoutPosition{
    Main,
    Secondary
  }

  public static ObjectReference.ObjRefID ObjectRef = new(){
    ID = "dialogue_character_ui"
  };

  public static InputFocusContext.ContextEnum InputContext = InputFocusContext.ContextEnum.UI;


  private class _show_character_data{
    public CharacterFocusUI character_ui;
    public ShowLayoutPosition position;
  }


  [SerializeField]
  private GameObject _CharacterShowMainParent;

  [SerializeField]
  private GameObject _CharacterMainNameContainer;
  [SerializeField]
  private TMP_Text _CharacterMainNameText;

  [SerializeField]
  private GameObject _CharacterShowSecondaryParent;

  [SerializeField]
  private GameObject _CharacterSecondaryNameContainer;
  [SerializeField]
  private TMP_Text _CharacterSecondaryNameText;

  [SerializeField]
  private GameObject _CharacterShowPrefabTemplate;
  [SerializeField]
  private GameObject _SequenceHandlerPrefab;

  [SerializeField]
  private DialogueUI _DialogueBase;

  [SerializeField]
  private uint _MaxCharacterShow = 3;


  private Dictionary<string, _show_character_data> _character_ui_instance_list = new();

  private GameTimeHandler _time_handler;

  private InputFocusContext _input_context;
  private CharacterDatabase _character_database;

  private bool _dialogue_finished = true;
  private Coroutine _dialogue_coroutine = null;

  private bool _next_dialogue_flag = false;


  private void _check_max_character_show_parallel(GameObject target_parent){
    int _c_idx = 0;
    for(int i = target_parent.transform.childCount-1; i >= 0; i--){
      GameObject _current_child = target_parent.transform.GetChild(i).gameObject;
      CharacterFocusUI _character_ui = _current_child.GetComponent<CharacterFocusUI>();
      if(_character_ui == null){
        Debug.LogWarning(string.Format("'{0}' does not have CharacterFocusUI Component in child idx: {0}.", target_parent.name, i));
        continue;
      }

      if(_c_idx < _MaxCharacterShow)
        _character_ui.ShowCharacter();
      else
        _character_ui.HideCharacter();

      _c_idx++;
    }
  }

  private void _check_max_character_show(){
    _check_max_character_show_parallel(_CharacterShowMainParent);
    _check_max_character_show_parallel(_CharacterShowSecondaryParent);
  }


  private string _add_character_name(string character_name, string before_list){
    if(before_list.Length > 0)
      before_list += ", ";
    
    before_list += character_name;
    return before_list;
  }


  private IEnumerator _character_remove(string character_id, bool skip_animation = false){
    if(!_character_ui_instance_list.ContainsKey(character_id))
      yield break;

    _show_character_data _data = _character_ui_instance_list[character_id];
    _character_ui_instance_list.Remove(character_id);
    if(_data.character_ui == null)
      yield break;

    FadeUI _fadeui = _data.character_ui.GetComponent<FadeUI>();
    if(!skip_animation && _fadeui != null){
      _fadeui.FadeToCover = false;
      _fadeui.StartTimerAsync();
      yield return new WaitUntil(_fadeui.TimerFinished);
    }

    Destroy(_data.character_ui.gameObject);
  }

  private void _character_unfocus_all(){
    foreach(_show_character_data _data in _character_ui_instance_list.Values){
      if(_data.character_ui == null)
        continue;

      StartCoroutine(_data.character_ui.UnfocusCharacter());
    }
  }


  private IEnumerator _character_create(ShowLayoutPosition position, string character_id, bool force_focus = false){
    if(_character_ui_instance_list.ContainsKey(character_id))
      yield break;

    GameObject _new_ui = Instantiate(_CharacterShowPrefabTemplate);
    switch(position){
      case ShowLayoutPosition.Main:{
        _new_ui.transform.SetParent(_CharacterShowMainParent.transform);
      }break;

      case ShowLayoutPosition.Secondary:{
        _new_ui.transform.SetParent(_CharacterShowSecondaryParent.transform);
      }break;
    }

    _new_ui.transform.SetAsLastSibling();
    CharacterFocusUI _character_ui = _new_ui.GetComponent<CharacterFocusUI>();

    _show_character_data _new_data = new _show_character_data{
      character_ui = _character_ui,
      position = position
    };

    _character_ui_instance_list[character_id] = _new_data;

    yield return new WaitForNextFrameUnit();
    yield return new WaitForEndOfFrame();

    if(_character_ui.SetCharacter(character_id)){
      yield return _character_ui.UnfocusCharacter();

      _check_max_character_show();
      
      RectTransform _rt_transform = _character_ui.GetComponent<RectTransform>();
      _rt_transform.localScale = Vector3.one;

      if(force_focus)
        yield return _character_ui.FocusCharacter();
    }
    else{
      Destroy(_new_ui);
      _new_data.character_ui = null;
    }
  }


  private IEnumerator _character_show(string character_id, bool reorder = true){
    if(!_character_ui_instance_list.ContainsKey(character_id)){
      Debug.LogWarning(string.Format("Character is not yet instantiated in UI. (Character ID: '{0}')", character_id));
      yield break;
    }

    _show_character_data _data = _character_ui_instance_list[character_id];
    if(_data.character_ui == null)
      yield break;

    if(reorder)
      _data.character_ui.transform.SetAsLastSibling();

    _check_max_character_show();

    UIUtility.RefreshLayoutGroupsImmediateAndRecursive(gameObject);

    yield return _data.character_ui.FocusCharacter();
  }


  private void _trigger_next_dialogue(){
    if(_DialogueBase.IsDialogueFinished())
      _next_dialogue_flag = true;
    else
      _DialogueBase.SkipDialogueAnimation();
  }

  private IEnumerator _dialogue_start(DialogueUI.DialogueSequence dialogue){
    DEBUGModeUtils.Log("dialogue start"); 
    
    _dialogue_finished = false;
    _input_context.RegisterInputObject(this, InputFocusContext.ContextEnum.UI);

    DEBUGModeUtils.Log("dialogue reset highlight character"); 
    ClearCharacterUIFrom(ShowLayoutPosition.Main, true);
    ClearCharacterUIFrom(ShowLayoutPosition.Secondary, true);
    
    DEBUGModeUtils.Log("dialogue start looping");
    DEBUGModeUtils.Log(string.Format("Dialogue Count {0}", dialogue.Sequence.Count));
    for(int i = 0; i < dialogue.Sequence.Count; i++){
      DialogueUI.DialogueData _current_data = dialogue.Sequence[i];

      DEBUGModeUtils.Log("dialogue unfocus all");
      // set focus and unfocus on certain character ui
      _character_unfocus_all();
      if(_current_data.DialogueCharacterUIData != null){
        ExtendedDialogue _this_data = _current_data.DialogueCharacterUIData;
        foreach(var _remove_data in _this_data.RemovedCharacters)
          StartCoroutine(_character_remove(_remove_data.CharacterID, _remove_data.SkipAnimation));

        foreach(var _init_data in _this_data.AddedCharacters)
          StartCoroutine(_character_create(_init_data.UIPosition, _init_data.CharacterID));
      }

      yield return new WaitForNextFrameUnit();
      yield return new WaitForEndOfFrame();

      DEBUGModeUtils.Log("dialogue highlight characters");
      string _main_name = "";
      string _secondary_name = "";

      bool _main_reorder = true;
      bool _secondary_reorder = true;
      foreach(string _talk_id in _current_data.CharactersTalking){
        if(!_character_ui_instance_list.ContainsKey(_talk_id))
          continue;

        DEBUGModeUtils.Log("dialogue highlight another character");
        
        _show_character_data _data = _character_ui_instance_list[_talk_id];
        bool _trigger_reorder = true;
        switch(_data.position){
          case ShowLayoutPosition.Main:{
            _trigger_reorder = _main_reorder;
            _main_reorder = false;
          }break;

          case ShowLayoutPosition.Secondary:{
            _trigger_reorder = _secondary_reorder;
            _secondary_reorder = false;
          }break;
        }

        StartCoroutine(_character_show(_talk_id, _trigger_reorder));

        TypeDataStorage _character_data = _character_database.GetDataStorage(_talk_id);
        if(_character_data == null){
          Debug.LogWarning(string.Format("Cannot get Character (ID: '{0}') data", _talk_id));
          continue;
        }

        CharacterMetadata.CharacterData _metadata = _character_data.GetData<CharacterMetadata.CharacterData>();
        switch(_data.position){
          case ShowLayoutPosition.Main:{
            _main_name = _add_character_name(_metadata.CharacterName, _main_name);
          }break;

          case ShowLayoutPosition.Secondary:{
            _secondary_name = _add_character_name(_metadata.CharacterName, _secondary_name);
          }break;
        }
      }

      DEBUGModeUtils.Log("dialogue setting names");
      // set names
      _CharacterMainNameContainer.SetActive(_main_name.Length > 0);
      _CharacterMainNameText.text = _main_name;

      _CharacterSecondaryNameContainer.SetActive(_secondary_name.Length > 0);
      _CharacterSecondaryNameText.text = _secondary_name;

      DEBUGModeUtils.Log("dialogue start dialogue");
      // start dialogue
      _DialogueBase.ChangeDialogue(_current_data);
      yield return _DialogueBase.IsDialogueFinished();
      yield return new WaitUntil(() => _next_dialogue_flag);

      _DialogueBase.TriggerSequenceAsync();
      yield return new WaitUntil(() => !_DialogueBase.IsSequenceTriggering());
      _next_dialogue_flag = false;

      DEBUGModeUtils.Log("dialogue sequence finished");
    }

    _dialogue_finished_function();
  }

  private void _dialogue_finished_function(){
    _input_context.RemoveInputObject(this, InputFocusContext.ContextEnum.UI);
    _dialogue_finished = true;
  }


  private void _game_scene_changed(string scene_id, GameHandler.GameContext context){
    ObjectReference.SetReferenceObject(ObjectRef, gameObject);
  }


  public void Start(){
    GameObject _test_obj = Instantiate(_CharacterShowPrefabTemplate);
    if(_test_obj.GetComponent<CharacterFocusUI>() == null){
      Debug.LogError("CharacterShow Prefab does not have CharacterFocusUI Component.");
      throw new MissingComponentException();
    }

    Destroy(_test_obj);

    GameHandler _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingReferenceException();
    }

    _game_handler.SceneChangedFinishedEvent += _game_scene_changed;


    _time_handler = FindAnyObjectByType<GameTimeHandler>();
    if(_time_handler == null){
      Debug.LogError("Cannot find GameTimeHandler.");
      throw new MissingComponentException();
    }

    _input_context = FindAnyObjectByType<InputFocusContext>();
    if(_input_context == null){
      Debug.LogError("Cannot find InputFocusContext.");
      throw new MissingComponentException();
    }

    _character_database = FindAnyObjectByType<CharacterDatabase>();
    if(_character_database == null){
      Debug.LogError("Cannot find database for Characters.");
      throw new MissingReferenceException();
    }

    _CharacterMainNameContainer.SetActive(false);
    _CharacterMainNameText.text = "";

    _CharacterSecondaryNameContainer.SetActive(false);
    _CharacterSecondaryNameText.text = "";
  
    
    if(_game_handler.SceneInitialized)
      _game_scene_changed(_game_handler.GetCurrentSceneID(), _game_handler.GetCurrentSceneContext());
  }


  public void StartDialogue(DialogueUI.DialogueSequence dialogue){
    if(!IsDialogueFinished())
      CancelDialogue();

    DEBUGModeUtils.Log("triggering dialogue async");
    _dialogue_coroutine = StartCoroutine(_dialogue_start(dialogue));
  }

  public bool IsDialogueFinished(){
    return _dialogue_finished;
  }


  public void CancelDialogue(){
    if(_dialogue_finished)
      return;

    StopCoroutine(_dialogue_coroutine);
    _dialogue_finished_function();
  }


  public void AddCharacterIn(ShowLayoutPosition position, string character_id, bool force_focus = false){
    StartCoroutine(_character_create(position, character_id, force_focus));
  }

  public IEnumerator RemoveCharacter(string character_id, bool skip_animation = false){
    yield return _character_remove(character_id, skip_animation);
  }

  public void ClearCharacterUIFrom(ShowLayoutPosition position, bool skip_animation = false){
    List<string> _list_remove = new();
    foreach(string id in _character_ui_instance_list.Keys){
      _show_character_data _data = _character_ui_instance_list[id];
      if(_data.position != position)
        continue;

      _list_remove.Add(id);
    }

    foreach(string id in _list_remove)
      StartCoroutine(_character_remove(id, skip_animation));
  }


  public void OnUIAccept(InputValue value){
    if(!_input_context.InputAvailable(this))
      return;

    if(value.isPressed)
      _trigger_next_dialogue();
  }

  public void OnMouseClicked(InputValue value){
    if(!_input_context.InputAvailable(this))
      return;

    if(value.isPressed)
      _trigger_next_dialogue();
  }

  public void OnInteractKey(InputValue value){
    if(!_input_context.InputAvailable(this))
      return;

    if(value.isPressed)
      _trigger_next_dialogue();
  }
}