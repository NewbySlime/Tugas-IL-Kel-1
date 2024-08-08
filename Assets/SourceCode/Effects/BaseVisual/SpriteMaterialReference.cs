using UnityEngine;


/// <summary>
/// A class that will hold the Material of the <b>SpriteRenderer</b> and used as a reference that can be used later on. See also <see cref="IMaterialReference"/>.
/// </summary>
public class SpriteMaterialReference: MonoBehaviour, IMaterialReference{
  [SerializeField]
  private SpriteRenderer _Sprite;


  public Material GetMaterial(){
    return _Sprite.material;
  }
}