using UnityEngine;


public class TestObject: MonoBehaviour{
  [SerializeField]
  private Transform _TestingObject;
  
  [SerializeField]
  private float _AngleSpeed;


  public void FixedUpdate(){
    Vector2 _direction = MathExt.AngleToDirection(_TestingObject);
    Debug.Log(string.Format("first angle {0}, direction {1}", _TestingObject.eulerAngles.z, _direction));

    float _angle = MathExt.DirectionToAngle(_direction);
    Debug.Log(string.Format("direction {0}, last angle {1}", _direction, _angle));
    _angle += _AngleSpeed * Time.fixedDeltaTime;

    Vector3 _angle_v = _TestingObject.eulerAngles;
    _angle_v.z = _angle;
    _TestingObject.eulerAngles = _angle_v;
  }
}