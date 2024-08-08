using UnityEngine;


/// <summary>
/// Class to recursively check and rescale itself and its children to the normally used scale for UI.
/// NOTE: since this class will rescale itself and its children to "one" it is preferably to modify the <b>RectTransform</b>'s data instead of scaling it.
/// </summary>
public class CanvasRecursiveScaleCalibrator: MonoBehaviour{
  private void _trigger_calibrate(GameObject target){
    target.transform.localScale = Vector3.one;
    for(int i = 0; i < target.transform.childCount; i++)
      _trigger_calibrate(target.transform.GetChild(i).gameObject);
  }


  /// <summary>
  /// Function to trigger checking and rescaling to itself and its children.
  /// </summary>
  public void TriggerCalibrate(){
    _trigger_calibrate(gameObject);
  }
}