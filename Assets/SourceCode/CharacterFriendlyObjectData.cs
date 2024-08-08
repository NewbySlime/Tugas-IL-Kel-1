using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Custom character data for <see cref="CharacterDatabase"/> that stores custom prefab based on the <see cref="ObjectFriendlyHandler.FriendlyType"/> associated with it.
/// </summary>
public class CharacterFriendlyObjectData: MonoBehaviour{
  [Serializable]
  /// <summary>
  /// Data structure for storing friendly context for a character.
  /// </summary>
  public struct ContextData{
    /// <summary>
    /// The friendly type of this data.
    /// </summary>
    public ObjectFriendlyHandler.FriendlyType Context;

    /// <summary>
    /// The custom prefab for this data.
    /// </summary>
    public GameObject CustomPrefab;
  }

  /// <summary>
  /// Data for storing it to database.
  /// </summary>
  public class CharacterData{
    public Dictionary<ObjectFriendlyHandler.FriendlyType, ContextData> CustomMap;
  }


  [SerializeField]
  private List<ContextData> CustomList;

  
  /// <inheritdoc cref="CharacterMetadata.CharacterDatabase_LoadData(TypeDataStorage)"/>
  public void CharacterDatabase_LoadData(TypeDataStorage data_storage){
    CharacterData _result = new(){
      CustomMap = new()
    };

    foreach(ContextData _data in CustomList)
      _result.CustomMap[_data.Context] = _data;

    data_storage.AddData(_result);
  }
}