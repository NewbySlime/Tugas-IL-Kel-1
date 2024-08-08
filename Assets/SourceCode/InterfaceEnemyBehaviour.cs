using UnityEngine;



/// <summary>
/// Interface class for enemy behaviour, mostly this class contains interface functions needed by enemy's functional stuff.
/// 
/// This class uses external component(s);
/// - Target <b>GameObject</b> as its enemy. The target can be set to null, but this will set to default behaviour (for "no enemy" state).
/// </summary>
public class InterfaceEnemyBehaviour: MonoBehaviour{
  /// <summary>
  /// The target enemy for this behaviour.
  /// </summary>
  protected GameObject _TargetEnemy = null;

  /// <summary>
  /// Virtual object that the inheriting class can use whenever a target enemy object has been changed.
  /// </summary>
  protected virtual void _on_target_enemy_changed(){}


  /// <summary>
  /// Set the target enemy for this object.
  /// </summary>
  /// <param name="target_object">The target enemy</param>
  public void SetEnemy(GameObject target_object){
    _TargetEnemy = target_object;

    _on_target_enemy_changed();
  }
}