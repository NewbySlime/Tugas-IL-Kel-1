using System.Collections;
using UnityEngine;



/// <summary>
/// Class for modifying components related to Character object presentation (like Unity's <b>Animator</b> component) based on the ID of a target character.
/// 
/// This class uses external component(s);
/// Unity's <b>Animator</b> component to modify its animation data to fit the target character.
/// 
/// This class uses autoload(s);
/// - <see cref="CharacterDatabase"/> for getting target Character data.
/// </summary>
public class CharacterComponent: MonoBehaviour{
  [SerializeField]
  private Animator _TargetAnimator;

  [SerializeField]
  private string _CharacterID;
  public string CharacterID{get => _CharacterID;}
  
  private CharacterDatabase _character_database;
  
  /// <summary>
  /// Flag to tell if the object has been initialized or not.
  /// </summary>
  public bool IsInitialized{private set; get;} = false;


  /// <summary>
  /// Extended Start function for waiting until every object is initialized for use.
  /// </summary>
  /// <returns>Coroutine helper object</returns>
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


  /// <summary>
  /// Sets and modify the related component to this component to match the target character's object configuration.
  /// </summary>
  /// <param name="character_id">The target character's ID</param>
  public void SetCharacterID(string character_id){
    _CharacterID = character_id;
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