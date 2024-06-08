using System.Collections;
using UnityEngine;



public class CharacterComponent: MonoBehaviour{
  [SerializeField]
  private Animator _TargetAnimator;

  [SerializeField]
  private string _CharacterID;
  public string CharacterID{get => _CharacterID;}
  
  private CharacterDatabase _character_database;
  
  public bool IsInitialized{private set; get;} = false;


  private IEnumerator _start_co_func(){
    yield return null;
    yield return new WaitForEndOfFrame();
    IsInitialized = true;

    SetCharacterID(CharacterID);
  }


  public void Start(){
    _character_database = FindAnyObjectByType<CharacterDatabase>();
    if(_character_database == null){
      Debug.LogError("Cannot find database for Characters.");
      throw new MissingReferenceException();
    }

    StartCoroutine(_start_co_func());
  }


  public void SetCharacterID(string character_id){
    if(!IsInitialized)
      return;

    TypeDataStorage _character_data = _character_database.GetDataStorage(character_id);
    if(_character_data == null){
      Debug.LogError(string.Format("Cannot get Character data for ID: '{0}'.", character_id));
      return;
    }

    CharacterSpriteData.CharacterData _sprite_data = _character_data.GetData<CharacterSpriteData.CharacterData>();
    if(_sprite_data == null){
      Debug.LogError(string.Format("Character (ID: {0}) does not have CharacterSpriteData.", character_id));
      return;
    }

    _TargetAnimator.runtimeAnimatorController = _sprite_data.CharacterMovementAnimation;
  }
}