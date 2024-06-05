using UnityEngine;


public class NPCHandler: MonoBehaviour{
  [SerializeField]
  private Animator _CharacterAnimator;

  private CharacterDatabase _character_database;

  
  public void Start(){
    Debug.Log("Starting");
    _character_database = FindAnyObjectByType<CharacterDatabase>();
    if(_character_database == null){
      Debug.LogError("Cannot find database for Characters.");
      throw new MissingReferenceException();
    }
  }


  public bool SetCharacter(string character_id){
    Debug.Log("Setting character");
    TypeDataStorage _character_data = _character_database.GetDataStorage(character_id);
    if(_character_data == null){
      Debug.LogError(string.Format("Cannot get Character data for ID: '{0}'.", character_id));
      return false;
    }

    CharacterSpriteData.CharacterData _sprite_data = _character_data.GetData<CharacterSpriteData.CharacterData>();
    if(_sprite_data == null){
      Debug.LogWarning(string.Format("Character does not have Sprite data. (ID: '{0}')", character_id));
      return false;
    }

    _CharacterAnimator.runtimeAnimatorController = _sprite_data.CharacterMovementAnimation;
    return true;
  }
}