using UnityEngine;


/// <summary>
/// As the name suggest, this component will moves this object to world position of the mouse (pointer position). The component will update the position in rendering update and also physics update.
/// NOTE: since Unity gives its pointer position based on the screen space, this class converts it to world position based on the camera target reference.
/// </summary>
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