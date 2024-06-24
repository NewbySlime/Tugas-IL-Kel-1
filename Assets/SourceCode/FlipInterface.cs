using System;
using System.Collections.Generic;
using UnityEngine;


public class FlipInterface: MonoBehaviour{
  private delegate void _SetFlipDelegateHelper(object obj, bool flip);

  [Serializable]
  private class _ObjectMetadataSerializeable{
    public GameObject Object;

    public bool UseTransform;

    public bool UseRotation;
  }

  private class _ObjectMetadata{
    public class _SetterFunction{
      public object _obj;

      public _SetFlipDelegateHelper _SetFlipX;
      public _SetFlipDelegateHelper _SetFlipY;
    }

    public _ObjectMetadataSerializeable _metadata;

    public List<_SetterFunction> _setter_function_list = new List<_SetterFunction>();
  }

  [SerializeField]
  private List<_ObjectMetadataSerializeable> _AddedFlipObjectList;


  private bool _flipped_x = false;
  private bool _flipped_y = false;


  private Dictionary<GameObject, _ObjectMetadata> _initialized_metadata_map = new Dictionary<GameObject, _ObjectMetadata>();


  private void _initialize_metadata(_ObjectMetadataSerializeable _metadata){
    if(!_initialized_metadata_map.ContainsKey(_metadata.Object)){
      _initialized_metadata_map[_metadata.Object] = new _ObjectMetadata();
    }

    _ObjectMetadata _init_metadata = _initialized_metadata_map[_metadata.Object];
    _init_metadata._metadata = _metadata;
    _init_metadata._setter_function_list.Clear();

    {// cek SpriteRenderer
      SpriteRenderer _sr = _metadata.Object.GetComponent<SpriteRenderer>();
      if(_sr != null){
        _init_metadata._setter_function_list.Add(new _ObjectMetadata._SetterFunction{
          _obj = _sr,
          _SetFlipX = _set_flipx_sprite_renderer,
          _SetFlipY = _set_flipy_sprite_renderer
        });
      }
    }

    if(_metadata.Object != gameObject){// cek FlipInterface 
      FlipInterface _fi = _metadata.Object.GetComponent<FlipInterface>();
      if(_fi != null){
        _init_metadata._setter_function_list.Add(new _ObjectMetadata._SetterFunction{
          _obj = _fi,
          _SetFlipX = _set_flipx_flip_interface,
          _SetFlipY = _set_flipy_flip_interface
        });
      }
    }
  }


  public void Start(){
    _initialize_metadata(new _ObjectMetadataSerializeable{
      Object = gameObject,

      UseTransform = false,

      UseRotation = false
    });

    foreach(_ObjectMetadataSerializeable _metadata in _AddedFlipObjectList)
      _initialize_metadata(_metadata);
  }


  public void SetFlippedX(string val){
    DEBUGModeUtils.Log(string.Format("flipped {0}", val));
    SetFlippedX(val == "true");
  }

  public void SetFlippedY(string val){
    SetFlippedY(val == "true");
  }


  public void SetFlippedX(bool flipped){
    _flipped_x = flipped;
    DEBUGModeUtils.Log("flipped");

    foreach(_ObjectMetadata _metadata in _initialized_metadata_map.Values){
      if(_metadata._metadata.Object == null)
        continue;

      gameObject.SendMessage("FlipInterface_SetFlippedX", flipped, SendMessageOptions.DontRequireReceiver);
      foreach(var _setter_func in _metadata._setter_function_list){
        _setter_func._SetFlipX(_setter_func._obj, flipped);
      }


      if(_metadata._metadata.UseTransform){
        GameObject _target_obj = _metadata._metadata.Object;
        Vector2 _loc_pos = _target_obj.transform.localPosition;
        _loc_pos.x = Mathf.Abs(_loc_pos.x) * (flipped? -1: 1);

        _target_obj.transform.localPosition = _loc_pos;
      }

      
      Vector3 _current_angle = _metadata._metadata.Object.transform.eulerAngles;
      Vector2 _direction = MathExt.AngleToDirection(_current_angle.z);
      DEBUGModeUtils.Log(string.Format("first angle {0}, direction {1}", _current_angle.z, _direction));
      if(_metadata._metadata.UseRotation)
        _direction.x = Mathf.Abs(_direction.x) * (flipped? -1: 1);

      _current_angle.z = MathExt.DirectionToAngle(_direction);
      DEBUGModeUtils.Log(string.Format("angle {0}, direction {1}", _current_angle.z, _direction));
      _metadata._metadata.Object.transform.eulerAngles = _current_angle;
    }
  }

  public void SetFlippedY(bool flipped){
    _flipped_y = flipped;
    
    foreach(_ObjectMetadata _metadata in _initialized_metadata_map.Values){
      if(_metadata._metadata.Object == null)
        continue;

      gameObject.SendMessage("FlipInterface_SetFlippedY", flipped, SendMessageOptions.DontRequireReceiver);
      foreach(var _setter_func in _metadata._setter_function_list){
        _setter_func._SetFlipY(_setter_func._obj, flipped);
      }


      if(_metadata._metadata.UseTransform){
        GameObject _target_obj = _metadata._metadata.Object;
        Vector2 _loc_pos = _target_obj.transform.localPosition;
        _loc_pos.y = Mathf.Abs(_loc_pos.y) * (flipped? -1: 1);

        _target_obj.transform.localPosition = _loc_pos;
      }

      Vector2 _direction = MathExt.AngleToDirection(_metadata._metadata.Object.transform);
      if(_metadata._metadata.UseRotation)
        _direction.y = Mathf.Abs(_direction.y) * (flipped? -1: 1);

      Vector3 _current_angle = _metadata._metadata.Object.transform.eulerAngles;
      _current_angle.z = MathExt.DirectionToAngle(_direction) * Mathf.Rad2Deg;

      _metadata._metadata.Object.transform.eulerAngles = _current_angle;
    }
  }
  


  // MARK: SpriteRenderer setter
  private static void _set_flipx_sprite_renderer(object obj, bool flip){
    SpriteRenderer _sr = obj as SpriteRenderer;
    if(_sr == null){
      Debug.LogWarning("Cannot get SpriteRenderer.");
      return;
    }

    _sr.flipX = flip;
  }

  private static void _set_flipy_sprite_renderer(object obj, bool flip){
    SpriteRenderer _sr = obj as SpriteRenderer;
    if(_sr == null){
      Debug.LogWarning("Cannot get SpriteRenderer.");
      return;
    }

    _sr.flipY = flip;
  }


  // MARK: FlipInterface setter
  private static void _set_flipx_flip_interface(object obj, bool flip){
    FlipInterface _fi = obj as FlipInterface;
    if(_fi == null){
      Debug.LogWarning("Cannot get FlipInterface.");
      return;
    }

    _fi.SetFlippedX(flip);
  }

  private static void _set_flipy_flip_interface(object obj, bool flip){
    FlipInterface _fi = obj as FlipInterface;
    if(_fi == null){
      Debug.LogWarning("Cannot get FlipInterface.");
      return;
    }

    _fi.SetFlippedY(flip);
  }
}