using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(RectTransform))]
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

  public IEnumerator UnfocusCharacter(){
    _TextureUI.material.SetColor("_ColorMultiply", _UnfocusMultColor);
    yield break;
  }

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


  public void HideCharacter(){
    _TextureUI.enabled = false;
  }

  public void ShowCharacter(){
    _TextureUI.enabled = true;
  }
}