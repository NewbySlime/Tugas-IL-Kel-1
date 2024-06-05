using UnityEngine;


public class SpriteMaterialReference: MonoBehaviour, IMaterialReference{
  [SerializeField]
  private SpriteRenderer _Sprite;


  public Material GetMaterial(){
    return _Sprite.material;
  }
}