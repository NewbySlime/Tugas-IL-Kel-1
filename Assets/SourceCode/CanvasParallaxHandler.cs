using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Canvas))]
public class CanvasParallaxHandler: MonoBehaviour{
  [SerializeField]
  private List<ParallaxComponent> _ListParallaxes;

  private Canvas _canvas;


  public void Start(){
    _canvas = GetComponent<Canvas>();
  }

  public void Update(){
    DEBUGModeUtils.Log(string.Format("Camera {0}", _canvas.worldCamera == null));
    if(_canvas.worldCamera == null)
      return;

    Camera _camera = _canvas.worldCamera;
    DEBUGModeUtils.Log(string.Format("parallax pos {0}", _camera.transform.position));
    foreach(ParallaxComponent _parallax in _ListParallaxes)
      _parallax.SetPosition(_camera.transform.position);
  }
}