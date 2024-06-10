using UnityEngine;


public class CanvasRecursiveScaleCalibrator: MonoBehaviour{
  private void _trigger_calibrate(GameObject target){
    target.transform.localScale = Vector3.one;
    for(int i = 0; i < target.transform.childCount; i++)
      _trigger_calibrate(target.transform.GetChild(i).gameObject);
  }


  public void TriggerCalibrate(){
    _trigger_calibrate(gameObject);
  }
}