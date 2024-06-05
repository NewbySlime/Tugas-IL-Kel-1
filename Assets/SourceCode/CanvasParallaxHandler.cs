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
    if(_canvas.worldCamera == null)
      return;

    Camera _camera = _canvas.worldCamera;
    foreach(ParallaxComponent _parallax in _ListParallaxes)
      _parallax.SetPosition(_camera.transform.position);
  }
}