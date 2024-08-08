using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


/// <summary>
/// Class for handling multiple progress bar with using one value.
/// Brief explanation: the progress uses non-normalized value, and the value is used for counting how many progress bar to use.
/// (ex. Expected value 8.00 which means 8 progress bar to be visualized).
/// 
/// This class uses external component(s);
/// - <b>GridLayoutGroup</b> for arranging each <see cref="ProgressionTexture"/> to its cells.
/// 
/// This class uses prefab(s);
/// - Prefab that has <see cref="ProgressTexture"/> as the "progression" effect for each bar.
/// </summary>
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


  /// <summary>
  /// Function to adding/removing progress bar based on the count.
  /// Uses <b>IEnumerator</b> due to the function need to wait until the created objects are initialized.
  /// </summary>
  /// <param name="count">How many progress bar</param>
  /// <returns>Coroutine helper object</returns>
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

  /// <summary>
  /// Function to set each progression bar with supplied texture.
  /// Uses <b>IEnumerator</b> due to progress bar have to be intialized.
  /// </summary>
  /// <param name="texture">The texture to be used</param>
  /// <returns>Coroutine helper object</returns>
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


  /// <summary>
  /// To set the progress value. NOTE: the value should be non-normalized and based on the bar count.
  /// To add/remove the bar count, see <see cref="SetProgressCount"/>. 
  /// </summary>
  /// <param name="progress">The progress value</param>
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

  
  /// <summary>
  /// To update or add/remove bar based on the new bar count.
  /// To set the progression value, see <see cref="SetProgressCount"/>.
  /// </summary>
  /// <param name="count">The bar count</param>
  public void SetProgressCount(int count){
    StartCoroutine(_set_progress_count(count));
  }

  /// <summary>
  /// To set the sprite for progression bar.
  /// </summary>
  /// <param name="texture">The texture to be used</param>
  public void SetProgressSprite(Texture texture){
    StartCoroutine(_set_progress_texture(texture));
  }

  /// <summary>
  /// To resize progression bars in this UI. NOTE: this function only resize the cells in <b>GridLayoutGroup</b>.
  /// </summary>
  /// <param name="size">The new size</param>
  public void SetSpriteSize(Vector2 size){
    _progress_grid.cellSize = size;
  }
}