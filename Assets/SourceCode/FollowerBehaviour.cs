using System.Collections;
using UnityEngine;


[RequireComponent(typeof(PathFollower))]
public class FollowerBehaviour: MonoBehaviour{
  [SerializeField]
  private Transform _TargetFollow;

  [SerializeField]
  private float _UpdateInterval = 0.5f;

  private PathFollower _follower;

  private Coroutine _follow_coroutine = null;

  
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
  }


  public void Start(){
    _follower = GetComponent<PathFollower>();
    StartCoroutine(_start_co_func());
  }


  public void SetTargetFollow(GameObject obj){
    if(_follow_coroutine != null)
      StopCoroutine(_follow_coroutine);

    _follower.CancelMoving();
    _TargetFollow = obj == null? null: obj.transform;

    _follow_coroutine = StartCoroutine(_follow_function());
  }
}