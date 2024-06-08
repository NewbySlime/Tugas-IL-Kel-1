using System;
using System.Collections.Generic;
using UnityEngine;


public class CharacterFriendlyObjectData: MonoBehaviour{
  [Serializable]
  public struct ContextData{
    public ObjectFriendlyHandler.FriendlyType Context;
    public GameObject CustomPrefab;
  }

  public class CharacterData{
    public Dictionary<ObjectFriendlyHandler.FriendlyType, ContextData> CustomMap;
  }


  [SerializeField]
  private List<ContextData> CustomList;

  
  public void CharacterDatabase_LoadData(TypeDataStorage data_storage){
    CharacterData _result = new(){
      CustomMap = new()
    };

    foreach(ContextData _data in CustomList)
      _result.CustomMap[_data.Context] = _data;

    data_storage.AddData(_result);
  }
}