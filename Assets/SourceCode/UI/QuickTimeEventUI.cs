using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;



public class QuickTimeEventUI: MonoBehaviour{
  [Serializable]
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
  private List<_ColorLerpData> _ColorInterpolation;

  private float _total_color_score = 0;

  private IMaterialReference _accept_bar_matref;
  private RectTransform _accept_bar_rect;



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

    _total_color_score = 0;
    foreach(_ColorLerpData _data in _ColorInterpolation)
      _total_color_score += _data._Score;

    _TimingBar.maxValue = 1;
    _TimingBar.minValue = 0;
  }


  public void SetAcceptBarMaxSize(float val){
    val = 1-val;

    float _right_value = _accept_bar_rect.sizeDelta.x * val;
    _accept_bar_rect.offsetMax = new Vector2(_right_value, _accept_bar_rect.offsetMax.y);
  }

  public void SetAcceptBarMinSize(float val){
    float _left_value = _accept_bar_rect.sizeDelta.x * val;
    _accept_bar_rect.offsetMin = new Vector2(_left_value, _accept_bar_rect.offsetMin.y);
  }


  public void SetQTETimingBar(float val){
    _TimingBar.value = val;
  }

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


  public void SetEventSymbol(Sprite symbol){
    _EventSymbolImageUI.sprite = symbol;
  }
}