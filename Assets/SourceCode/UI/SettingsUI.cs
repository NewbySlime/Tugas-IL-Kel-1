using System;
using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;


[RequireComponent(typeof(PersistanceContext))]
public class SettingsUI: MonoBehaviour{
  [Serializable]
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


  private IEnumerator _auto_save_co_func(){
    while(_auto_save_timer > 0){
      yield return new WaitForFixedUpdate();

      _auto_save_timer -= Time.fixedDeltaTime;
    }

    _persistance_context.WriteSave();
  }

  private void _trigger_auto_save(){
    _auto_save_timer = _AutoSaveDelay;
    if(_auto_save_coroutine != null)
      return;

    _auto_save_coroutine = StartCoroutine(_auto_save_co_func());
  }

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
    Debug.Log("back button pressed.");
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


  public RuntimeData AsRuntimeData(){
    return new(){
      MasterVolume = _MasterSlider.value,
      SoundFXVolume = _SoundSlider.value,
      MusicVolume = _MusicSlider.value
    };
  }

  public void FromRuntimeData(RuntimeData data){
    if(data == null)
      return;

    _MasterSlider.value = data.MasterVolume;
    _SoundSlider.value = data.SoundFXVolume;
    _MusicSlider.value = data.MusicVolume;
  }


  public void OnUICancel(InputValue value){
    if(!_focus_context.InputAvailable(this))
      return;

    if(value.isPressed){
      _game_handler.CloseSettingsUI();
    }
  }
}