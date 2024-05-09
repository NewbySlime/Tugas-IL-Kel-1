using UnityEngine;


public class MouseFollower: MonoBehaviour{
  [SerializeField]
  private Camera _BaseCamera;

  private void _update_position(){
    Vector3 _mouse_pos = Input.mousePosition;
    Vector3 _screen_size = new Vector3(Screen.width, Screen.height) / 2;
    
    _mouse_pos = ((_mouse_pos - _screen_size) / _screen_size.y) * _BaseCamera.orthographicSize;
    
    _mouse_pos.z -= _BaseCamera.transform.position.z;
    transform.position = _mouse_pos + _BaseCamera.transform.position;
  }


  public void Update(){
    _update_position();
  }

  public void FixedUpdate(){
    _update_position();
  }
}