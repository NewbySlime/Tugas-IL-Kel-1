using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class PickableObject: MonoBehaviour{
  private Rigidbody2D _rigidbody;

  private LayerMask _default_exlayer;

  public TypeDataStorage TemporaryItemData{private set; get;} = new();


  public void Start(){
    _rigidbody = GetComponent<Rigidbody2D>();
    _default_exlayer = _rigidbody.excludeLayers;
  }


  public void AsStatic(bool flag){
    _rigidbody.constraints = flag? RigidbodyConstraints2D.FreezePosition: RigidbodyConstraints2D.None;
    _rigidbody.excludeLayers = flag? LayerMask.NameToLayer("Everything"): _default_exlayer;
  }
} 