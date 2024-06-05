using System;
using UnityEditor.Animations;
using UnityEngine;


public class CharacterSpriteData: MonoBehaviour{
  [Serializable]
  public class CharacterData{
    public Sprite FullBody;

    public AnimatorController CharacterMovementAnimation;
  }

  [SerializeField]
  private CharacterData _ResourceData;


  public void CharacterDatabase_LoadData(TypeDataStorage data_storage){
    data_storage.AddData(_ResourceData);
  }
}