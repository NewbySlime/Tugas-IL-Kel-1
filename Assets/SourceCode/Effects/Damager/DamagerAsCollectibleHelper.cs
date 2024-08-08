using UnityEngine;


[RequireComponent(typeof(DamagerComponent))]
/// <summary>
/// An extended <see cref="DamagerComponent"/> feature that makes the Damager object becomes an item once the speed is in certain threshold.
/// 
/// The threshold data contained in <see cref="DamagerComponent.DamagerContext"/>>.
/// 
/// This class uses Component(s) such as;
/// - <see cref="CollectibleComponent"/> to determine when the Item is being picked.
/// </summary>
public class DamagerAsCollectibleHelper: MonoBehaviour{
  [SerializeField]
  private CollectibleComponent _TargetCollectibleComponent;

  private DamagerComponent _damager;

  private Vector3 _last_position;
  private float _speed_threshold = -1;


  public void Start(){
    _damager = GetComponent<DamagerComponent>();

    if(_TargetCollectibleComponent == null){
      Debug.LogWarning("No Collectible component assigned.");
    }
    else{
      _TargetCollectibleComponent.SetEnableCollection(false);
    }

    _last_position = transform.position;
  }

  public void FixedUpdate(){
    if(_TargetCollectibleComponent != null){
      float _current_speed = (transform.position-_last_position).magnitude / Time.fixedDeltaTime;
      if(_current_speed <= _speed_threshold){
        if(!_TargetCollectibleComponent.GetEnableCollection())
          _TargetCollectibleComponent.SetEnableCollection(true);
      }
      else
        _TargetCollectibleComponent.SetEnableCollection(false);
    }

    _last_position = transform.position;
  }


  /// <summary>
  /// An interface function (Unity's Message) used for telling the object that the DamagerContet has changed.
  /// </summary>
  public void DamagerData_OnContextChanged(){
    DamagerComponent.DamagerContext _context = _damager.GetDamagerContext();
    if(_context.AsCollectible)
      _speed_threshold = _context.AllowCollectibleOnSpeedThreshold;
    else
      _speed_threshold = -1;
  }
}