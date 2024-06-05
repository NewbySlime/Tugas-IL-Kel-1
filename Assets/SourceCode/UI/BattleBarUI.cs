using System.Collections;
using UnityEngine;


public class BattleBarUI: MonoBehaviour{
  public enum LayoutPosition{
    Top,
    Bottom
  }

  private class _UpdateVisualInstance{
    public BaseProgressUI ProgressUI;

    public float smooth_speed_ref;

    public float value;
    public float target_value;
  }

  public class BarMetadata{
    public string BarName;
  }




  [SerializeField]
  private BaseProgressUI _ProgressTop;
  [SerializeField]
  private BaseProgressUI _ProgressBottom;

  [SerializeField]
  private float _SmoothTime;

  [SerializeField]
  private bool _UseProgressMax;


  private _UpdateVisualInstance _top_inst;
  private _UpdateVisualInstance _bottom_inst;


  private IEnumerator _update_bar(_UpdateVisualInstance target_obj){
    yield return null;
    yield return new WaitForEndOfFrame();

    while(true){
      yield return null;

      if(Mathf.Abs(target_obj.target_value-target_obj.value) < 0.01)
        continue;

      target_obj.value = Mathf.SmoothDamp(target_obj.value, target_obj.target_value, ref target_obj.smooth_speed_ref, _SmoothTime);
      target_obj.ProgressUI.SetProgress(target_obj.value);
    }
  }


  public void Start(){
    _top_inst = new(){
      ProgressUI = _ProgressTop
    };

    _bottom_inst = new(){
      ProgressUI = _ProgressBottom
    };
  }
  

  public void SetProgress(LayoutPosition position, float val, bool skip_animation = false){
    _UpdateVisualInstance _vis_instance = null;
    switch(position){
      case LayoutPosition.Top:{
        _vis_instance = _top_inst;
      }break;

      case LayoutPosition.Bottom:{
        _vis_instance = _bottom_inst;
      }break;
    }

    _vis_instance.target_value = val;
    if(skip_animation)
      _vis_instance.value = val;
  }

  
  public void SetBarData(LayoutPosition position, BarMetadata metadata){
    // TODO
  }


  public void OnEnable(){
    StartCoroutine(_update_bar(_top_inst));
    StartCoroutine(_update_bar(_bottom_inst));
  }
}