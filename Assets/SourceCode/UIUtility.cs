using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class UIUtility{
  public static IEnumerator SetHideUI(GameObject ui_obj, bool ui_hide, bool skip_animation = false){
    DEBUGModeUtils.Log(string.Format("setting ui hide {0} {1}", ui_obj.name, ui_hide));

    FadeUI _fadeui = ui_obj.GetComponent<FadeUI>();
    if(_fadeui != null){
      _fadeui.FadeToCover = !ui_hide;
    }

    SlideUI _slideui = ui_obj.GetComponent<SlideUI>();
    if(_slideui != null){
      _slideui.ShowAnimation = !ui_hide;
    }

    ShrinkUI _shrinkui = ui_obj.GetComponent<ShrinkUI>();
    if(_shrinkui != null){
      _shrinkui.DoShrink = ui_hide;
    }

    SetActiveUIOnTimeout _set_active_ui = ui_obj.GetComponent<SetActiveUIOnTimeout>();
    if(_set_active_ui != null){
      _set_active_ui.SetActiveTarget = !ui_hide;
    }

    TimingBaseUI.SkipAllTimer(ui_obj);
    yield return TimingBaseUI.StartAllTimer(ui_obj, skip_animation);
  }

  
  // Problem/Bug: LayoutGroup does not refresh when reordering the child(s)
  // Solution from: https://forum.unity.com/threads/layoutgroup-does-not-refresh-in-its-current-frame.458446/ 
  public static void RefreshLayoutGroupsImmediateAndRecursive(GameObject root){
    var componentsInChildren = root.GetComponentsInChildren<LayoutGroup>(true);

    foreach (var layoutGroup in componentsInChildren){
      LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
    }

    var parent = root.GetComponent<LayoutGroup>();
    if(parent != null)
      LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());
  }
}