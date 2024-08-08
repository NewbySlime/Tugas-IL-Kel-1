using System.Collections;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Static class containing utility functions for handling UI.
/// </summary>
public static class UIUtility{
  /// <summary>
  /// Show/Hide a target UI object and handles its effect animations that inherits from <see cref="TimingBaseUI"/>.
  /// The list of available effect(s);
  /// - <see cref="FadeUI"/>
  /// - <see cref="SetActiveUIOnTimeout"/>
  /// - <see cref="ShrinkUI"/>
  /// - <see cref="SlideUI"/>
  /// </summary>
  /// <param name="ui_obj"></param>
  /// <param name="ui_hide"></param>
  /// <param name="skip_animation"></param>
  /// <returns></returns>
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

  
  /// <summary>
  /// Immediately refresh (update) the target UI object for UI layout. This will look for <b>LayoutGroup</b> objects and update their layouts.  
  ///  
  /// Problem/Bug: LayoutGroup does not refresh when reordering the child(s)
  /// Solution from: https://forum.unity.com/threads/layoutgroup-does-not-refresh-in-its-current-frame.458446/
  /// </summary>
  /// <param name="root">The target object to update</param>
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