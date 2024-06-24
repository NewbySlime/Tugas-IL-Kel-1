using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class MultipleProgressBar: MonoBehaviour{
  [SerializeField]
  private GameObject _ProgressBarPrefab;
  [SerializeField]
  private GameObject _ProgressTextureTargetParent;

  [SerializeField]
  private Vector2 _ProgressBarSize;

  private List<ProgressTexture> _progress_list = new();
  private Texture _progress_texture = null;

  private GridLayoutGroup _progress_grid;

  private float _progress_value;

  private bool _progress_bar_initialized = false;


  private IEnumerator _set_progress_count(int count){
    if(_ProgressBarPrefab == null || count == _progress_list.Count)
      yield break;

    _progress_bar_initialized = false;

    if(count > _progress_list.Count){
      int _start_idx = _progress_list.Count;
      for(int i = _start_idx; i < count; i++){
        GameObject _progress_obj = Instantiate(_ProgressBarPrefab);
        _progress_obj.transform.SetParent(_ProgressTextureTargetParent.transform);
        _progress_obj.transform.SetAsLastSibling();

        ProgressTexture _progress_tex = _progress_obj.GetComponent<ProgressTexture>();
        _progress_list.Add(_progress_tex);
      }

      yield return null;

      for(int i = _start_idx; i < count; i++){
        ProgressTexture _progress_tex = _progress_list[i];
        _progress_tex.SetTexture(_progress_texture);
      }
    }
    else{
      for(int i = _progress_list.Count-1; i >= count; i--){
        ProgressTexture _progress_tex = _progress_list[i];
        Destroy(_progress_tex.gameObject);
      }

      _progress_list.RemoveRange(count, _progress_list.Count-count);
    }
    
    _progress_bar_initialized = true;
    SetProgress(_progress_value);
  }

  private IEnumerator _set_progress_texture(Texture texture){
    _progress_texture = texture;
    
    yield return new WaitUntil(() => _progress_bar_initialized);
    foreach(ProgressTexture _progress in _progress_list)
      _progress.SetTexture(texture);
  }


  public void Start(){
    // tes _ProgressBarPrefab
    GameObject _prefab_test = Instantiate(_ProgressBarPrefab);
    if(!_prefab_test.GetComponent<ProgressTexture>()){
      Debug.LogWarning("ProgressBarPrefab does not have ProgressTexture.");
      _ProgressBarPrefab = null;
    }
    Destroy(_prefab_test);

    _progress_grid = _ProgressTextureTargetParent.GetComponent<GridLayoutGroup>();
    if(_progress_grid == null){
      Debug.LogError("Progress Parent Target does not have GridLayoutGroup.");
      throw new MissingComponentException();
    }

    SetSpriteSize(_ProgressBarSize);
  }


  public void SetProgress(float progress){
    _progress_value = progress;

    float _progress_left = progress;
    for(int i = 0; i < _progress_list.Count; i++){
      float _delta_progress = Mathf.Clamp(_progress_left, 0, 1);
      _progress_left -= _delta_progress;

      ProgressTexture _progress_tex = _progress_list[i];
      _progress_tex.SetProgress(_delta_progress);
    }
  }

  public void SetProgressCount(int count){
    StartCoroutine(_set_progress_count(count));
  }

  public void SetProgressSprite(Texture texture){
    StartCoroutine(_set_progress_texture(texture));
  }

  public void SetSpriteSize(Vector2 size){
    _progress_grid.cellSize = size;
  }
}