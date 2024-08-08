using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


/// <summary>
/// OUTDATED
/// Class for console feature of the Game, any user can input any available command for testing/debugging purpose.
/// 
/// This class uses external component(s);
/// - <b>Unity's TMP Text UI</b> for both command input and console/debug log data.
/// - Wrapper <b>GameObject</b> containing all UI elements for the console.
/// 
/// This class uses following autoload(s);
/// - <see cref="InputFocusContext"/> for asking focus for using input.
/// 
/// NOTE: only works in DEBUG context. 
/// </summary>
public class ConsoleUIController: MonoBehaviour, ISelectHandler, IDeselectHandler{
  [SerializeField]
  private TextMeshProUGUI _CommandText;
  [SerializeField]
  private TextMeshProUGUI _ConsoleText;
  
  [SerializeField]
  private GameObject _WrapperContent;

  private InputFocusContext _input_context;

  private Selectable _selectable;

  private bool _console_toggled = false;

  private bool _is_selected = false;
  private string _current_line = "";
  private string _command_format = "> {0}";

  
  /// <summary>
  /// Function to split a sentence into array of word.
  /// </summary>
  /// <param name="command">Sentence to be processed</param>
  /// <returns>Resulting array of word</returns>
  private string[] _SplitCommandString(string command){
    string[] _splits = command.Split(' ');
    string[] _results = new string[0];

    string _combined_str = "";
    for(int i = 0; i < _splits.Length; i++){
      DEBUGModeUtils.Log("getting split");
      DEBUGModeUtils.Log(i);
      string str = _splits[i];

      DEBUGModeUtils.Log(string.Format("split str: {0}", str));
      if(str.Length <= 0)
        continue;

      if(str[0] == '"' || str[0] == '\'')
        _combined_str += str.Substring(1, str.Length);
      else if(_combined_str.Length > 0){
        _combined_str += str;
        if(str[str.Length-1] == '"' || str[str.Length-1] == '\''){
          _combined_str = _combined_str.Substring(0, _combined_str.Length-1);

          Array.Resize(ref _results, _results.Length+1);
          _results[_results.Length-1] = _combined_str;

          _combined_str = "";
        }
      }
      else{
        Array.Resize(ref _results, _results.Length+1);
        _results[_results.Length-1] = str;

        DEBUGModeUtils.Log(string.Format("result size {0}", _results.Length));
      }
    }

    return _results;
  }

  
  /// <summary>
  /// Function to update the UI.
  /// </summary>
  private void _UpdateCommandText(){
    _CommandText.text = string.Format(_command_format, _current_line);
  }


  /// <summary>
  /// Function to enable the console.
  /// </summary>
  /// <param name="enable">Flag for enabling/showing the console</param>
  private void _SetEnable(bool enable){
    _WrapperContent.SetActive(enable);
    _selectable.interactable = enable;
  }


  public void Start(){
#if DEBUG
    _input_context = FindObjectOfType<InputFocusContext>();
    _selectable = GetComponent<Selectable>();

    _console_toggled = true;
    ToggleConsole();

    _UpdateCommandText();
#endif // DEBUG
  }

  public void Update(){
#if DEBUG
    // brief explanation: every input key supplied from Unity's input system passed per character to be processed as a sentence.
    if(_is_selected){
      foreach(char c in Input.inputString){
        if(c == '\b' && _current_line.Length > 0)
          _current_line = _current_line.Substring(0, _current_line.Length-1);
        else if(c == '\n' || c == '\r'){
          SendCommand(_current_line);
          _current_line = "";
        }
        else
          _current_line += c;
      }

      if(Input.inputString.Length > 0)
        _UpdateCommandText();
    }
#endif // DEBUG
  }


  /// <summary>
  /// Function to show/hide the console by toggling.
  /// </summary>
  public void ToggleConsole(){
#if DEBUG
    _console_toggled = !_console_toggled;
    _SetEnable(_console_toggled);

    if(!_console_toggled){
      OnDeselect(null);
    }
#endif // DEBUG
  }


  /// <summary>
  /// Function to process the command supplied.
  /// </summary>
  /// <param name="command">The command sentence</param>
  public void SendCommand(string command){
#if DEBUG
    DEBUGModeUtils.Log(string.Format("command: {0}", command));
    string[] _command_split = _SplitCommandString(command);
    DEBUGModeUtils.Log(_command_split.Length);
    gameObject.SendMessage("ConsoleHandler_CommandAccept", _command_split,  SendMessageOptions.DontRequireReceiver);
#endif // DEBUG
  }

  /// <summary>
  /// To catch the Unity's "UI focus" event.
  /// </summary>
  /// <param name="_event_data">Event data from Unity</param>
  public void OnSelect(BaseEventData _event_data){
#if DEBUG
    _is_selected = true;
    _input_context.RegisterInputObject(this, InputFocusContext.ContextEnum.UI);
#endif // DEBUG
  }


  /// <summary>
  /// To catch the Unity's "UI unfocus" event.
  /// </summary>
  /// <param name="_event_data">Event data from Unity</param>
  public void OnDeselect(BaseEventData _event_data){
#if DEBUG
    _is_selected = false;
    _input_context.RemoveInputObject(this, InputFocusContext.ContextEnum.UI);
#endif // DEBUG
  }


  /// <summary>
  /// To catch Unity's input event for "ConsoleToggle" key input.
  /// </summary>
  /// <param name="value">Unity's input data</param>
  public void OnConsoleToggle(InputValue value){
#if DEBUG
    if(value.isPressed)
      ToggleConsole();
#endif // DEBUG
  }
}