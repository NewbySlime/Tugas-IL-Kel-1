using System;
using UnityEngine;


public class CharacterMetadata: MonoBehaviour{
  [Serializable]
  public struct CharacterData{
    public string CharacterID;
    public string CharacterName;
  }

  [SerializeField]
  private CharacterData _Metadata;


  public void CharacterDatabase_LoadData(TypeDataStorage data_storage){
    data_storage.AddData(_Metadata);
  }


  public string GetCharacterID(){
    return _Metadata.CharacterID;
  }

  public string GetCharacterName(){
    return _Metadata.CharacterName;
  }
}