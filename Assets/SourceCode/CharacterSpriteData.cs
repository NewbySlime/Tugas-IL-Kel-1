using System;
using UnityEngine;


/// <summary>
/// Custom character data for <see cref="CharacterDatabase"/> that stores sprites and images for the character.
/// </summary>
public class CharacterSpriteData: MonoBehaviour{
  [Serializable]
  /// <summary>
  /// Data structure for storing the sprites needed for a character.
  /// </summary>
  public class CharacterData{
    /// <summary>
    /// Full body image for this Character.
    /// </summary>
    public Sprite FullBody;

    /// <summary>
    /// Animation data for this character.
    /// </summary>
    public RuntimeAnimatorController CharacterMovementAnimation;
  }

  [SerializeField]
  private CharacterData _ResourceData;


  /// <inheritdoc cref="CharacterMetadata.CharacterDatabase_LoadData(TypeDataStorage)"/>
  public void CharacterDatabase_LoadData(TypeDataStorage data_storage){
    data_storage.AddData(_ResourceData);
  }
}