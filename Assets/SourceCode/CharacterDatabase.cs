using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Database Class for loading and storing data for Characters.
/// The database will find any prefab (data) in a determined folder with the prefab has <see cref="CharacterMetadata"/> data component for early identification.
/// NOTE: for creating custom data component for a character, the database will use interface function <b>CharacterDatabase_LoadData(TypeDataStorage)</b>.
/// 
/// This class uses prefab(s);
/// - Prefab for character object creation. It contains any components that can be used for handling a character object (AI Behaviours, Animation Handler, and such).
/// </summary>
public class CharacterDatabase: MonoBehaviour{
  /// <summary>
  /// Event for when this class has been initialized.
  /// </summary>
  public event OnInitialized OnInitializedEvent;
  public delegate void OnInitialized();

  /// <summary>
  /// Path to a folder containing prefabs of character data in Resources folder.
  /// </summary>
  private static string character_data_folder = "Characters";


  private struct _character_metadata{
    public string character_id;
    public string character_name;

    // custom data about the character
    public TypeDataStorage data_storage;

    // object prefab
    public GameObject _this;
  }


  [SerializeField]
  private GameObject _CharacterBase;

  private Dictionary<string, _character_metadata> _character_map = new();


  /// <summary>
  /// Flag if this class is ready or not yet.
  /// </summary>
  public bool IsInitialized{get; private set;} = false;


  public void Start(){
    // load all prefabs and store the data
    GameObject[] _prefab_list = Resources.LoadAll<GameObject>(character_data_folder);
    foreach(GameObject _prefab_obj in _prefab_list){
      GameObject _tmp_gameobj = Instantiate(_prefab_obj);
      CharacterMetadata _metadata = _tmp_gameobj.GetComponent<CharacterMetadata>();
      if(_metadata == null){
        Debug.LogError(string.Format("Prefab ({0}) is not an Character.", _prefab_obj.name));
        continue;
      }

      TypeDataStorage _character_data = new();
      _tmp_gameobj.SendMessage("CharacterDatabase_LoadData", _character_data, SendMessageOptions.DontRequireReceiver);

      Destroy(_tmp_gameobj);

      _character_map.Add(_metadata.GetCharacterID(), new _character_metadata{
        character_id = _metadata.GetCharacterID(),
        character_name = _metadata.GetCharacterName(),

        data_storage = _character_data,

        _this = _prefab_obj,
      });
    }

    GameObject _tmp_obj = Instantiate(_CharacterBase);
    if(_tmp_obj.GetComponent<CharacterComponent>() == null)
      Debug.LogWarning("CharacterBase Prefab does not have CharacterComponent.");

    Destroy(_tmp_obj);

    IsInitialized = true;
    OnInitializedEvent?.Invoke();
  }


  #nullable enable
  /// <summary>
  /// Get custom data about the character.
  /// </summary>
  /// <param name="character_id">Target character ID</param>
  /// <returns>Character's custom data</returns>
  public TypeDataStorage? GetDataStorage(string character_id){
    if(!_character_map.ContainsKey(character_id))
      return null;

    return _character_map[character_id].data_storage;
  }
  #nullable disable

  /// <summary>
  /// Get base prefab for creating character object.
  /// </summary>
  /// <returns>the prefab</returns>
  private GameObject GetCharacterBasePrefab(){
    return _CharacterBase;
  }

  /// <summary>
  /// Create a character object based on the ID and the friendly context.
  /// A character can use a custom object based on its friendly context contained in <see cref="CharacterFriendlyObjectData"/>.
  /// </summary>
  /// <param name="character_id">Target character ID</param>
  /// <param name="friendly_context">Friendly context of the created character</param>
  /// <returns></returns>
  public GameObject CreateNewCharacter(string character_id, ObjectFriendlyHandler.FriendlyType friendly_context = ObjectFriendlyHandler.FriendlyType.Neutral){
    TypeDataStorage _character_data = GetDataStorage(character_id);
    if(_character_data == null){
      Debug.LogError(string.Format("Cannot get Character data for ID: '{0}'.", character_id));
      return null;
    }

    bool _use_default = true;
    GameObject _inst_obj = null;

    CharacterFriendlyObjectData.CharacterData _friendly_data = _character_data.GetData<CharacterFriendlyObjectData.CharacterData>();
    if(_friendly_data != null && _friendly_data.CustomMap.ContainsKey(friendly_context)){
      _use_default = false;
      _inst_obj = Instantiate(_friendly_data.CustomMap[friendly_context].CustomPrefab);
    }

    if(_use_default)
      _inst_obj = Instantiate(_CharacterBase);

    
    // Check for CharacterComponent
    CharacterComponent _character_component = _inst_obj.GetComponent<CharacterComponent>();
    if(_character_component == null)
      Debug.LogWarning(string.Format("Instantiated Character Object (ID: {0}, FriendlyContext: {1}) does not have CharacterComponent.", character_id, friendly_context));
    else
      _character_component.SetCharacterID(character_id);


    // Check for ObjectFriendlyHandler
    ObjectFriendlyHandler _friendly_handler = _inst_obj.GetComponent<ObjectFriendlyHandler>();
    if(_friendly_handler == null)
      Debug.LogWarning(string.Format("Instantiated Character Object (ID: {0}, FriendlyContext: {1}) does not have ObjectFriendlyHandler.", character_id, friendly_context));
    else
      _friendly_handler.FriendlyContext = friendly_context;

    return _inst_obj;
  }
}