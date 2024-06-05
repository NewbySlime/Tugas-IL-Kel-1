using UnityEngine;


public class PickableObjectPickerHandler: MonoBehaviour{
  [SerializeField]
  private Transform _PickableTargetPosition;

  [SerializeField]
  private GameObject _PickableContainer;

  [SerializeField]
  private float _ThrowForce;

  private bool _has_object = false;

  private PickableObject _current_object;
  private Rigidbody2D _current_rb;



  public void PickupObject(PickableObject pickable){
    if(_has_object)
      ThrowObject(Vector2.zero);

    _has_object = true;
    
    _current_object = pickable;
    _current_rb = pickable.gameObject.GetComponent<Rigidbody2D>();

    pickable.transform.SetParent(_PickableContainer.transform);
    pickable.transform.position = _PickableTargetPosition.position;

    pickable.AsStatic(true);
  }


  public void ThrowObject(Vector2 direction){
    if(!_has_object)
      return;

    _has_object = false;

    _current_object.AsStatic(false);
    _current_object.transform.SetParent(null);

    _current_rb.AddForce(direction * _ThrowForce);
    
    _current_object = null;
    _current_rb = null;
  }


  public bool GetHasObject(){
    return _has_object;
  }

  public PickableObject GetCurrentObject(){
    return _current_object;
  }
}