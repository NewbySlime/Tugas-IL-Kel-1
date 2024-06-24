using System;
using UnityEngine;


public class CharacterSpriteData: MonoBehaviour{
  [Serializable]
  public class CharacterData{
    public Sprite FullBody;

    public RuntimeAnimatorController CharacterMovementAnimation;
  }

  [SerializeField]
  private CharacterData _ResourceData;


  public void CharacterDatabase_LoadData(TypeDataStorage data_storage){
    data_storage.AddData(_ResourceData);
  }
}