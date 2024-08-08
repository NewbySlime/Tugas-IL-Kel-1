using UnityEngine;
using Unity.VisualScripting;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;


[RequireComponent(typeof(CircleCollider2D))]
/// <summary>
/// Class for giving an object a functionality to get "alerted" when a valid object is entering its view.
/// An object can be valid when it is allowed to be collided or entered in this <b>Collider2D</b> area.
/// For further explanation, see the diagram contained in <b>Reference/Diagrams/AlertTrigger.drawio</b>
/// 
/// This class uses following component(s);
/// - <b>CircleCollider2D</b> trigger collider for watching objects entering or exiting.
/// </summary>
public class AlertTrigger: MonoBehaviour{
  /// <summary>
  /// Event for when a valid object entered its view.
  /// </summary>
  public event AlertObjectEnter AlertObjectEnterEvent;
  public delegate void AlertObjectEnter(GameObject gobject);
  
  /// <summary>
  /// Event for when a valid object exited its view.
  /// </summary>
  public event AlertObjectExited AlertObjectExitedEvent;
  public delegate void AlertObjectExited(GameObject gobject);


  [SerializeField]
  private float _ViewAngle;
  [SerializeField]
  private float _ViewDistance;

  [SerializeField]
  /// <summary>
  /// List of compatible components that can be manipulated to match the configuration of this component.
  /// Currently compatible component(s);
  /// - <b>URP's Light2D</b>
  /// </summary>
  private List<Component> _CompatibleComponents;

  [SerializeField]
  /// <summary>
  /// Layer(s) that can block its view.
  /// </summary>
  private LayerMask _ObstructionLayer;


  private CircleCollider2D _collider;

  private List<GameObject> _inside_circle = new List<GameObject>();
  private List<GameObject> _inside_focus = new List<GameObject>();


  // Function to update the compatible components to match this class' configuration.
  private void _update_angle_component(){
    Dictionary<Type, Action<Component>> _compatible_function = new Dictionary<Type, Action<Component>>{
      {typeof(Light2D), (Component _target_component) => {
        Light2D _light = (Light2D)_target_component;
        float _radius_ratio = _light.pointLightInnerRadius / _light.pointLightOuterRadius;
        _light.pointLightInnerRadius = _radius_ratio*_ViewDistance;
        _light.pointLightOuterRadius = _ViewDistance;

        float _outer_offset = _light.pointLightOuterAngle-_light.pointLightInnerAngle;
        _light.pointLightOuterAngle = _ViewAngle + _outer_offset;
        _light.pointLightInnerAngle = _ViewAngle;

        _light.transform.localEulerAngles = Vector3.zero;
      }}
    };

    foreach(Component _component in _CompatibleComponents){
      if(!_compatible_function.ContainsKey(_component.GetType())){
        Debug.LogWarning(string.Format("Assigned component is not compatible. (type: {0})", _component.GetType()));
        continue;
      }

      _compatible_function[_component.GetType()](_component);
    }
  }



  public void Start(){
    _collider = GetComponent<CircleCollider2D>();
    if(_collider == null){
      Debug.LogError("Cannot get Collider.");
      throw new MissingComponentException();
    }

    _collider.isTrigger = true;
    _collider.radius = _ViewDistance;

    _update_angle_component();
  }

  public void FixedUpdate(){
    // watch objects that entered or exited its view
    foreach(GameObject _object in _inside_circle){
      Vector2 _object_dir = _object.transform.position - transform.position;
      float _object_dist = _object_dir.magnitude; _object_dir.Normalize();

      float _object_angle = MathExt.DirectionToAngle(_object_dir);
      float _current_angle = MathExt.NormalizeAngle(transform.eulerAngles.z);

      RaycastHit2D _rayhit = Physics2D.Raycast(transform.position, _object_dir, _object_dist, _ObstructionLayer);

      float _delta_angle = Math.Abs(_object_angle-_current_angle);
      //DEBUGModeUtils.Log(string.Format("angle {0}, {1}", _object_angle, _current_angle));
      //DEBUGModeUtils.Log(string.Format("delta angle {0}", _delta_angle));
      bool _is_in_focus = _delta_angle <= (_ViewAngle/2) && _rayhit.collider == null;
      if(!_inside_focus.Contains(_object)){
        if(_is_in_focus){
          _inside_focus.Add(_object);
          AlertObjectEnterEvent?.Invoke(_object);
        }
      }
      else{
        if(!_is_in_focus){
          _inside_focus.Remove(_object);
          AlertObjectExitedEvent?.Invoke(_object);
        }
      }
    }
  }


  /// <summary>
  /// Function to catch <b>Collider2D</b> (as a trigger) event when an object is entered its area.
  /// </summary>
  /// <param name="collider">The object in question</param>
  public void OnTriggerEnter2D(Collider2D collider){
    _inside_circle.Add(collider.gameObject);
  }

  /// <summary>
  /// Function to catch <b>Collider2D</b> (as a trigger) event when an object is exiting its area.
  /// </summary>
  /// <param name="collider">The object in question</param>
  public void OnTriggerExit2D(Collider2D collider){
    if(!_inside_circle.Contains(collider.gameObject))
      return;

    _inside_circle.Remove(collider.gameObject);
    if(_inside_focus.Contains(collider.gameObject)){
      _inside_focus.Remove(collider.gameObject);
      AlertObjectExitedEvent?.Invoke(collider.gameObject);
    }
  }


#if UNITY_EDITOR
  private void OnDrawGizmos(){
    Gizmos.color = Color.green;
    Gizmos.DrawLine(transform.position, transform.position +
      (Vector3)MathExt.AngleToDirection(transform) * _ViewDistance
    );

    float _bottom_angle = transform.eulerAngles.z - _ViewAngle/2;
    Gizmos.color = Color.yellow;
    Gizmos.DrawLine(transform.position, transform.position + 
      (Vector3)MathExt.AngleToDirection(_bottom_angle) * _ViewDistance
    );

    float _top_angle = _bottom_angle + _ViewAngle;
    Gizmos.DrawLine(transform.position, transform.position + 
      (Vector3)MathExt.AngleToDirection(_top_angle) * _ViewDistance
    );

    foreach(GameObject _object in _inside_focus){
      Gizmos.color = Color.red;
      Gizmos.DrawLine(transform.position, _object.transform.position);
    }

    foreach(GameObject _object in _inside_circle){
      if(_inside_focus.Contains(_object))
        continue;

      Gizmos.color = Color.green;
      Gizmos.DrawLine(transform.position, _object.transform.position);
    }
  }
#endif
}