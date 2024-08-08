using System;
using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;


[RequireComponent(typeof(PersistanceContext))]
/// <summary>
/// UI Class for handling and giving interaction to modify the Game settings/configuration data.
/// NOTE: this class deattach its save with the one provided by <see cref="GameHandler"/> by using own <see cref="PersistanceContext"/>. The save file path can be modified in the editor under <see cref="PersistanceContext"/> component.
/// 
/// This class uses following component(s);
/// - <see cref="PersistanceContext"/> to use own save file different from the one provided by <see cref="GameHandler"/>.
/// 
/// This class uses external component(s);
/// - <b>AudioMixer</b> the target mixer to modify the audio settings.
/// - <b>Slider</b> interface interaction for modifiying settings.
/// - <see cref="ButtonBaseUI"/> as the "back" button to close this UI.
/// 
/// This class uses autoload(s);
/// - <see cref="GameHandler"/> for game events and such.
/// - <see cref="InputFocusContext"/> for asking focus to use input.
/// </summary>
public class SettingsUI: MonoBehaviour{
  [Serializable]
  /// <summary>
  /// The runtime data related to this class for use in <see cref="GameRuntimeData"/> or for saving to <see cref="PersistanceContext"/>.
  /// </summary>
  public class RuntimeData: PersistanceContext.IPersistance{
    public float MasterVolume = 0;
    public float SoundFXVolume = 0;
    public float MusicVolume = 0;


    public string GetDataID(){
      return "SettingsUI.Data";
    }


    public string GetData(){
      return ConvertExt.ToBase64String(JsonUtility.ToJson(this));
    }

    public void SetData(string data){
      JsonUtility.FromJsonOverwrite(ConvertExt.FromBase64String(data), this);
    }
  }


  [SerializeField]
  private AudioMixer _TargetMixer;

  [SerializeField]
  private Slider _MasterSlider;
  [SerializeField]
  private Slider _MusicSlider;
  [SerializeField]
  private Slider _SoundSlider;

  [SerializeField]
  private ButtonBaseUI _BackButton;

  [SerializeField]
  private float _AutoSaveDelay;


  private GameHandler _game_handler;
  private InputFocusContext _focus_context;

  private PersistanceContext _persistance_context;
  public PersistanceContext PersistanceHandler{get => _persistance_context;}

  private Coroutine _auto_save_coroutine = null;
  private float _auto_save_timer = 0;


  // Coroutine function for auto save. The timer delay can be restarted by resetting _auto_save_timer to the original time.
  // This function only giving "saving" trigger to PersistanceContext. To see how the data is given to PersistanceContext, see _persistance_saving function.
  private IEnumerator _auto_save_co_func(){
    while(_auto_save_timer > 0){
      yield return null;
      _auto_save_timer -= Time.unscaledDeltaTime;
      DEBUGModeUtils.Log(string.Format("timer {0}", _auto_save_timer));
    }

    _persistance_context.WriteSave();
    _auto_save_coroutine = null;
  }

  // Function to trigger auto save when a data has been changed.
  // This function won't instantly save the data, but it will start the delay or resets the delay until auto save.
  private void _trigger_auto_save(){
    _auto_save_timer = _AutoSaveDelay;
    if(_auto_save_coroutine != null)
      return;

    _auto_save_coroutine = StartCoroutine(_auto_save_co_func());
  }

  // Update the data inputted from slider or any interaction to target AudioMixer.
  private void _update_sound_volume(){
    _TargetMixer.SetFloat("master_volume", _MasterSlider.value);
    _TargetMixer.SetFloat("music_volume", _MusicSlider.value);
    _TargetMixer.SetFloat("sfx_volume", _SoundSlider.value);
  }


  private void _sound_slider_value_changed(){
    _update_sound_volume();
    _trigger_auto_save();
  }


  private void _on_back_button(){
    DEBUGModeUtils.Log("back button pressed.");
    _game_handler.CloseSettingsUI();
  }


  private void _persistance_saving(PersistanceContext context){
    // Self data
    context.ParseData(AsRuntimeData());
  }

  private void _persistance_loading(PersistanceContext context){
    // Self data
    RuntimeData _this_rdata = new();
    context.OverwriteData(_this_rdata);
    FromRuntimeData(_this_rdata);
  }



  public void Start(){
    _persistance_context = GetComponent<PersistanceContext>();
    _persistance_context.PersistanceSavingEvent += _persistance_saving;
    _persistance_context.PersistanceLoadingEvent += _persistance_loading;

    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingReferenceException();
    }

    _focus_context = FindAnyObjectByType<InputFocusContext>();
    if(_focus_context == null){
      Debug.LogError("Cannot find InputFocusContext.");
      throw new MissingReferenceException();
    }

    _MasterSlider.onValueChanged.AddListener(delegate {_sound_slider_value_changed();});
    _MusicSlider.onValueChanged.AddListener(delegate {_sound_slider_value_changed();});
    _SoundSlider.onValueChanged.AddListener(delegate {_sound_slider_value_changed();});

    _BackButton.OnButtonReleasedEvent += _on_back_button;

    _persistance_context.ReadSave();
    _update_sound_volume();
  }


  /// <summary>
  /// Get the data related to this object as <see cref="RuntimeData"/>.
  /// </summary>
  /// <returns>This data</returns>
  public RuntimeData AsRuntimeData(){
    return new(){
      MasterVolume = _MasterSlider.value,
      SoundFXVolume = _SoundSlider.value,
      MusicVolume = _MusicSlider.value
    };
  }

  /// <summary>
  /// To parse the data related to this object from <see cref="RuntimeData"/> to be applied to this object.
  /// </summary>
  /// <param name="data">This new data</param>
  public void FromRuntimeData(RuntimeData data){
    if(data == null)
      return;

    _MasterSlider.value = data.MasterVolume;
    _SoundSlider.value = data.SoundFXVolume;
    _MusicSlider.value = data.MusicVolume;
  }


  /// <summary>
  /// Function to catch "UICancel" input event.
  /// This function is used to close this UI by prompting it to <see cref="GameHandler"/>, not self.
  /// </summary>
  /// <param name="value">Unity's input data</param>
  public void OnUICancel(InputValue value){
    if(!_focus_context.InputAvailable(this))
      return;

    if(value.isPressed){
      _game_handler.CloseSettingsUI();
    }
  }
}