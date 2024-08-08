using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(DamagerComponent))]
/// <summary>
/// An extended <see cref="DamagerComponent"/> feature that makes the Damager object exclude some determined layers in certain threshold (for instance, excluding Enemy layer when below threshold speed).
/// </summary>
public class DamagerLayerExcludingHelper: MonoBehaviour{
  [SerializeField]
  private Collider2D _TargetColliderManipulation;

  private DamagerComponent _damager;

  private Vector3 _last_position;
  private float _speed_threshold = -1;

  private LayerMask _default_excluding_layer;
  private LayerMask _excluding_layer;

  private bool _excluded_started = false;


  public void Start(){
    _damager = GetComponent<DamagerComponent>();

    if(_TargetColliderManipulation == null){
      Debug.LogWarning("No Collider component assigned.");
    }
    else{
      _TargetColliderManipulation.isTrigger = false;
      _default_excluding_layer = _TargetColliderManipulation.excludeLayers;
    }

    _last_position = transform.position;
  }

  public void FixedUpdate(){
    if(_TargetColliderManipulation != null && !_excluded_started){
      float _current_speed = (transform.position-_last_position).magnitude / Time.fixedDeltaTime;

      _excluded_started = _current_speed <= _speed_threshold;
      _TargetColliderManipulation.excludeLayers = _excluded_started? _excluding_layer: _default_excluding_layer;
    }

    _last_position = transform.position;
  }

  /// <summary>
  /// An interface function (Unity's Message) used for telling the object that the DamagerContet has been triggered.
  /// </summary>
  public void DamagerComponent_TriggerDamager(DamagerComponent.DamagerTriggerData data){
    _excluded_started = false;
  }

  /// <summary>
  /// An interface function (Unity's Message) used for telling the object that the DamagerContet has changed.
  /// </summary>
  public void DamagerComponent_OnContextChanged(){
    DEBUGModeUtils.Log("damager set context");
    DamagerComponent.DamagerContext _context = _damager.GetDamagerContext();
    _speed_threshold = _context.SetExcludeLayerOnSpeedThreshold;
    _excluding_layer = _context.ExcludeLayerHide;

    DEBUGModeUtils.Log(string.Format("damager speed threshold {0}", _speed_threshold));
  }
}