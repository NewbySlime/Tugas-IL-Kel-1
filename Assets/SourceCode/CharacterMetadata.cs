using System;
using UnityEngine;


/// <summary>
/// Base character data for <see cref="CharacterDatabase"/> that stores identifiers about the character. This data is important for creating a character.
/// </summary>
public class CharacterMetadata: MonoBehaviour{
  [Serializable]
  /// <summary>
  /// Identification data for <see cref="CharacterDatabase"/>.
  /// </summary>
  public class CharacterData{
    /// <summary>
    /// ID for this character.
    /// </summary>
    public string CharacterID;

    /// <summary>
    /// Name of the character.
    /// </summary>
    public string CharacterName;
  }

  [SerializeField]
  private CharacterData _Metadata;


  /// <summary>
  /// Interface class for catching message from <see cref="CharacterDatabase"/> and storing this class' data.
  /// </summary>
  /// <param name="data_storage">Data storage from the database</param>
  public void CharacterDatabase_LoadData(TypeDataStorage data_storage){
    data_storage.AddData(_Metadata);
  }


  /// <summary>
  /// Get ID for this character.
  /// </summary>
  /// <returns>The ID</returns>
  public string GetCharacterID(){
    return _Metadata.CharacterID;
  }

  /// <summary>
  /// Get the name of this character.
  /// </summary>
  /// <returns>The character's name</returns>
  public string GetCharacterName(){
    return _Metadata.CharacterName;
  }
}