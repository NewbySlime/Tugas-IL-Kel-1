using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


/// <summary>
/// Ini adalah controller (pengontrol UI) untuk membantu developer yang ingin menggunakan fungsi/mekanik game tanpa harus "trigger" yang menyebabkan mekanik itu terjadi.
/// </summary>
public class ConsoleUIController: MonoBehaviour, ISelectHandler, IDeselectHandler{
  [SerializeField]
  private TextMeshProUGUI _CommandText;
  [SerializeField]
  private TextMeshProUGUI _ConsoleText;
  
  [SerializeField]
  private GameObject _WrapperContent;

  private InputContext _input_context;

  private Selectable _selectable;

  private bool _console_toggled = false;

  private bool _is_selected = false;
  private string _current_line = "";
  private string _command_format = "> {0}";

  
  /// <summary>
  /// Fungsi untuk memisahkan kalimat menjadi kumpulan kata-kata.
  /// </summary>
  /// <param name="command">Kalimat yang ingin diproses</param>
  /// <returns>Kumpulan kata-kata yang sudah diproses</returns>
  private string[] _SplitCommandString(string command){
    string[] _splits = command.Split(' ');
    string[] _results = new string[0];

    string _combined_str = "";
    for(int i = 0; i < _splits.Length; i++){
      Debug.Log("getting split");
      Debug.Log(i);
      string str = _splits[i];

      Debug.Log(string.Format("split str: {0}", str));
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

        Debug.Log(string.Format("result size {0}", _results.Length));
      }
    }

    return _results;
  }

  
  /// <summary>
  /// Fungsi untuk mengupdate TextBox pada bagian Command.
  /// </summary>
  private void _UpdateCommandText(){
    _CommandText.text = string.Format(_command_format, _current_line);
  }


  /// <summary>
  /// Untuk mengaktifkan atau sebaliknya fungsi-fungsi yang terkait pada GameObject ini.
  /// </summary>
  /// <param name="enable">Apakah aktif?</param>
  private void _SetEnable(bool enable){
    _WrapperContent.SetActive(enable);
    _selectable.interactable = enable;
  }


  public void Start(){
    _input_context = FindObjectOfType<InputContext>();
    _selectable = GetComponent<Selectable>();

    _console_toggled = true;
    ToggleConsole();

    _UpdateCommandText();
  }

  public void Update(){
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
  }


  /// <summary>
  /// Fungsi untuk toggle (state menjadi sebaliknya) Console.
  /// </summary>
  public void ToggleConsole(){
    _console_toggled = !_console_toggled;
    _SetEnable(_console_toggled);

    if(!_console_toggled){
      OnDeselect(null);
    }
  }


  /// <summary>
  /// Fungsi untuk memberikan Command ke komponen lainnya yang mengggunakan fungsi console.
  /// </summary>
  /// <param name="command">Command yang mau dijalankan kepada Game</param>
  public void SendCommand(string command){
    Debug.Log(string.Format("command: {0}", command));
    string[] _command_split = _SplitCommandString(command);
    Debug.Log(_command_split.Length);
    gameObject.SendMessage("ConsoleHandler_CommandAccept", _command_split,  SendMessageOptions.DontRequireReceiver);
  }


  /// <summary>
  /// Ke-trigger ketika panel "Console" diselect (dipilih/dipencet).
  /// </summary>
  /// <param name="_event_data">Data event dari Unity</param>
  public void OnSelect(BaseEventData _event_data){
    _is_selected = true;
    _input_context.BindUIInputContext(gameObject);
  }


  /// <summary>
  /// Ke-trigger ketika panel "Console" di-deselect (Tidak di fokuskan ke panel tersebut).
  /// </summary>
  /// <param name="_event_data">Data event dari Unity</param>
  public void OnDeselect(BaseEventData _event_data){
    _is_selected = false;
    _input_context.RemoveUIInputContext(gameObject);
  }


  /// <summary>
  /// Ke-trigger dari Input "ConsoleTrigger" 
  /// </summary>
  /// <param name="value">Data event dari Unity</param>
  public void OnConsoleToggle(InputValue value){
    if(value.isPressed)
      ToggleConsole();
  }
}