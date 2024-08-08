using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// TODO: recheck for next feature
/// 
/// UI Class for visualizing QTE system.
/// 
/// This class uses external component(s);
/// - <b>Slider</b> for the QTE response "bar".
/// - <b>Unity's MaskableGraphics</b> object for presenting the symbol used.
/// - <b>GameObject</b> that has <b>RectTransform</b> and <see cref="IMaterialReference"/> for "Accept" timing bar to resize and manipulate its color.
/// </summary>
public class QuickTimeEventUI: MonoBehaviour{
  [Serializable]
  // data for interpolating color based on the score.
  private struct _ColorLerpData{
    public float _Score;
    public Color _Color;
  }

  [SerializeField]
  private GameObject _AcceptBar;

  [SerializeField]
  private Slider _TimingBar;

  [SerializeField]
  private Image _EventSymbolImageUI;

  [SerializeField]
  /// <summary>
  /// Color interpolation data based on sum of the entire score for "Accept" timing bar.
  /// </summary>
  private List<_ColorLerpData> _ColorInterpolation;

  // the total of the sum of the entire score from _ColorInterpolation.
  private float _total_color_score = 0;

  private IMaterialReference _accept_bar_matref;
  private RectTransform _accept_bar_rect;

  private float _accept_bar_maxsize = 0;



  public void Start(){
    _accept_bar_matref = _AcceptBar.GetComponent<IMaterialReference>();
    if(_accept_bar_matref == null){
      Debug.LogError("Cannot get IMaterialReference from AcceptBar.");
      throw new MissingComponentException();
    }

    _accept_bar_rect = _AcceptBar.GetComponent<RectTransform>();
    if(_accept_bar_rect == null){
      Debug.LogError("Cannot get RectTransform from AcceptBar.");
      throw new MissingComponentException();
    }

    _accept_bar_rect.offsetMax = new(0, _accept_bar_rect.offsetMax.y);
    _accept_bar_rect.offsetMin = new(0, _accept_bar_rect.offsetMin.y);
    _accept_bar_rect.ForceUpdateRectTransforms();

    _accept_bar_maxsize = _accept_bar_rect.sizeDelta.x;

    _total_color_score = 0;
    foreach(_ColorLerpData _data in _ColorInterpolation)
      _total_color_score += _data._Score;

    _TimingBar.maxValue = 1;
    _TimingBar.minValue = 0;
  }


  /// <summary>
  /// To resize the right side of the "Accept" timing bar. The value is based on the maximum rect size, hence the use of normalized value.
  /// </summary>
  /// <param name="val">The normalized value</param>
  public void SetAcceptBarMaxSize(float val){
    val = 1-val;

    float _right_value = _accept_bar_maxsize * val;
    _accept_bar_rect.offsetMax = new Vector2(_right_value, _accept_bar_rect.offsetMax.y);
  }

  /// <summary>
  /// To resize the left side of the "Accept" timing bar. The value is based on the maximum rect size, hence the use of normalized value.
  /// </summary>
  /// <param name="val">The normalized value</param>
  public void SetAcceptBarMinSize(float val){
    float _left_value = _accept_bar_maxsize * val;
    _accept_bar_rect.offsetMin = new Vector2(_left_value, _accept_bar_rect.offsetMin.y);
  }


  /// <summary>
  /// Set the current time for the timing bar. NOTE: this uses normalized value of the timer.
  /// </summary>
  /// <param name="val">The normalized value</param>
  public void SetQTETimingBar(float val){
    _TimingBar.value = val;
  }

  /// <summary>
  /// Manipulate the "Accept" bar's color. NOTE: this uses normalized value of the maximum score.
  /// </summary>
  /// <param name="val">The normalized value</param>
  public void SetAcceptBarColorLerp(float val){
    float _score_dist = _total_color_score*val;

    float _current_score = 0;
    int i = 0;
    for(; i < (_ColorInterpolation.Count-1); i++){
      _ColorLerpData _data = _ColorInterpolation[i];

      float _new_score = _current_score + _data._Score;
      if(_new_score > _score_dist){
        _score_dist -= _current_score;
        break;
      }
      
      _current_score = _new_score;
    }

    Color _new_col;
    if(i >= (_ColorInterpolation.Count-1)){
      _ColorLerpData _data = _ColorInterpolation[_ColorInterpolation.Count-1];
      
      _new_col = _data._Color;
    }
    else{
      _ColorLerpData _before_data = _ColorInterpolation[i];
      _ColorLerpData _after_data = _ColorInterpolation[i+1];

      _new_col = Color.Lerp(_before_data._Color, _after_data._Color, _score_dist/_before_data._Score);
    }

    _accept_bar_matref.GetMaterial().SetColor("_ColorMultiply", _new_col);
  }


  /// <summary>
  /// To set the icon for the symbol used for the type of the event.
  /// </summary>
  /// <param name="symbol">The image to be used</param>
  public void SetEventSymbol(Sprite symbol){
    _EventSymbolImageUI.sprite = symbol;
  }
}