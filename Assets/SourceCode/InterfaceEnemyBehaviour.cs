using UnityEngine;



public class InterfaceEnemyBehaviour: MonoBehaviour{
  protected GameObject _TargetEnemy = null;

  protected virtual void _on_target_enemy_changed(){}

  public void SetEnemy(GameObject target_object){
    _TargetEnemy = target_object;

    _on_target_enemy_changed();
  }
}