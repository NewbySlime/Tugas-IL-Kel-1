using System.Collections;
using UnityEngine;


[RequireComponent(typeof(PathFollower))]
/// <summary>
/// Base behaviour class for following any target objects. For further explanation, see <see cref="PathFollower"/>.
/// 
/// This behaviour uses following object(s);
/// - <see cref="PathFollower"/> component for pathfinding and AI movement.
/// 
/// This behaviour uses external object(s);
/// - Target <b>GameObject</b> to follow, or can be set manually by function.
/// </summary>
public class FollowerBehaviour: MonoBehaviour{
  [SerializeField]
  private Transform _TargetFollow;

  [SerializeField]
  private float _UpdateInterval = 0.5f;

  private PathFollower _follower;

  private Coroutine _follow_coroutine = null;

  /// <summary>
  /// Flag to check if the object is ready or not.
  /// </summary>
  /// <value></value>
  public bool IsInitialized{private set; get;} = false;

  
  // Continous follow, even if the path is invalid.
  private IEnumerator _follow_function(){
    while(_TargetFollow != null){
      yield return new WaitForSeconds(_UpdateInterval);

      _follower.CancelMoving();
      _follower.FollowPathAsync(_TargetFollow.position);
    }

    _follow_coroutine = null;
  }


  private IEnumerator _start_co_func(){
    yield return null;
    
    if(_TargetFollow != null)
      SetTargetFollow(_TargetFollow.gameObject);

    IsInitialized = true;
  }


  public void Start(){
    _follower = GetComponent<PathFollower>();
    StartCoroutine(_start_co_func());
  }


  /// <summary>
  /// Manually set the target object to follow. It will replace the previous follow object.
  /// </summary>
  /// <param name="obj">The object to follow</param>
  public void SetTargetFollow(GameObject obj){
    if(_follow_coroutine != null)
      StopCoroutine(_follow_coroutine);

    _follower.CancelMoving();
    _TargetFollow = obj == null? null: obj.transform;

    _follow_coroutine = StartCoroutine(_follow_function());
  }


  /// <summary>
  /// Function to catch Unity's "Object Enabled" event.
  /// </summary>
  public void OnEnable(){
    if(!IsInitialized)
      return;

    SetTargetFollow(_TargetFollow.gameObject);
  }
}