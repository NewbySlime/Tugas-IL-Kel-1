using System.Collections;
using UnityEngine;

public static class UIUtility{
  public static IEnumerator SetHideUI(GameObject ui_obj, bool ui_hide, bool skip_animation = false){
    FadeUI _fadeui = ui_obj.GetComponent<FadeUI>();
    if(_fadeui != null){
      _fadeui.FadeToCover = !ui_hide;
    }

    SlideUI _slideui = ui_obj.GetComponent<SlideUI>();
    if(_slideui != null){
      _slideui.ShowAnimation = !ui_hide;
    }

    SetActiveUIOnTimeout _set_active_ui = ui_obj.GetComponent<SetActiveUIOnTimeout>();
    if(_set_active_ui != null){
      _set_active_ui.SetActiveTarget = !ui_hide;
    }

    TimingBaseUI.SkipAllTimer(ui_obj);
    yield return TimingBaseUI.StartAllTimer(ui_obj, skip_animation);
  }
}