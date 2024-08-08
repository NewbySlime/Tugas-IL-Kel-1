using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(RectTransform))]
/// <summary>
/// UI element for showing and giving "focus" effect/animation to fullbody image of a character that used in <see cref="DialogueCharacterUI"/>.
/// Image for the characters are fetched data from <see cref="CharacterDatabase"/>.
/// 
/// This class uses following component(s);
/// - <b>RectTransform</b> for base UI's needed components.
/// 
/// This class uses external component(s);
/// - <b>Image</b> UI for storing character's image.
/// 
/// This class uses following autoload(s);
/// - <see cref="CharacterDatabase"/> for getting character data about the bound character.
/// </summary>
public class CharacterFocusUI: MonoBehaviour{
  [SerializeField]
  private Color _UnfocusMultColor;
  [SerializeField]
  private float _Exaggeration = 50;

  [SerializeField]
  private float _FocusTime = 0.2f;

  [SerializeField]
  private Image _TextureUI;
  [SerializeField]
  private Material _TextureMaterial;


  private CharacterDatabase _character_database;

  private RectTransform _rt_transform;


  public void Start(){
    DEBUGModeUtils.Log("starting");
    _character_database = FindAnyObjectByType<CharacterDatabase>();
    if(_character_database == null){
      Debug.LogError("Cannot find database for Characters.");
      throw new MissingReferenceException();
    }

    _TextureUI.material = new Material(_TextureMaterial);

    _rt_transform = GetComponent<RectTransform>();
  }


  /// <summary>
  /// To bind the Character ID with this class where the would be used to gather data to show Character Fullbody image. 
  /// </summary>
  /// <param name="character_id">The target character's ID</param>
  /// <returns>If binding the ID successful</returns>
  public bool SetCharacter(string character_id){
    TypeDataStorage _data = _character_database.GetDataStorage(character_id);
    if(_data == null){
      Debug.LogWarning(string.Format("Cannot get Character with ID: '{0}'.", character_id));
      return false;
    }

    CharacterSpriteData.CharacterData _sprite_data = _data.GetData<CharacterSpriteData.CharacterData>();
    if(_sprite_data == null){
      Debug.LogWarning(string.Format("Character (ID: '{0}') does not have CharacterSpriteData.", character_id));
      return false;
    }

    // skip if no FullBody sprite
    if(_sprite_data.FullBody == null)
      return false;

    _TextureUI.sprite = _sprite_data.FullBody;
    //_TextureUI.material.SetTexture("_MainTex", _sprite_data.FullBody);
    return true;
  }

  /// <summary>
  /// To start "unfocus" effect.
  /// </summary>
  /// <returns>Coroutine helper object</returns>
  public IEnumerator UnfocusCharacter(){
    _TextureUI.material.SetColor("_ColorMultiply", _UnfocusMultColor);
    yield break;
  }

  /// <summary>
  /// To start "focus" effect.
  /// </summary>
  /// <returns>Coroutine helper object</returns>
  public IEnumerator FocusCharacter(){
    _TextureUI.material.SetColor("_ColorMultiply", Color.white);
    Vector3 _default_pos = _rt_transform.anchoredPosition;

    float _timer = _FocusTime;
    while(_timer > 0){
      yield return null;

      _timer -= Time.unscaledDeltaTime;
      
      float _val = Mathf.SmoothStep(0, 1, _timer/_FocusTime);
      _rt_transform.anchoredPosition = _default_pos + new Vector3(0, _val * _Exaggeration);
    }

    _rt_transform.anchoredPosition = _default_pos;
  }


  /// <summary>
  /// To force hide the UI element.
  /// </summary>
  public void HideCharacter(){
    _TextureUI.enabled = false;
  }

  /// <summary>
  /// To force show the UI element.
  /// </summary>
  public void ShowCharacter(){
    _TextureUI.enabled = true;
  }
}