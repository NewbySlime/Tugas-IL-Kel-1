using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectSpawner: MonoBehaviour{
  [SerializeField]
  private GameObject _CopyContainer = null;

  [SerializeField]
  private GameObject _SpawnedContainer;

  [SerializeField]
  private GameObject _EnableContainer = null;

  [SerializeField]
  private int _RandomObjectSpawnedMax = 1;
  [SerializeField]
  private int _RandomObjectSpawnedMin = 1;

  [SerializeField]
  private float _RandomTimerSpawnMax = 5f;
  [SerializeField]
  private float _RandomTimerSpawnMin = 3f;

  [SerializeField]
  private List<GameObject> _ListTriggerObjectByDistance = new();

  [SerializeField]
  private float _MinDistanceToTrigger = 10f;

  [SerializeField]
  private bool _AllowSameObjectToSpawn = false;


  private Dictionary<int, GameObject> _trigger_obj_by_dist = new();

  private float _trigger_timer = -1;


  public bool TriggerByDistance = true;
  public bool TriggerByTimeout = true;
  public bool TriggerWhenEmpty = true;

  public bool IgnoreRandomSpawnCount = false;


  private void _spawn_object(GameObject target_copy){
    GameObject _copied_obj = Instantiate(target_copy);
    _copied_obj.transform.SetParent(_SpawnedContainer.transform);
    _copied_obj.SetActive(true);

    _copied_obj.transform.position = target_copy.transform.position;
    _copied_obj.transform.rotation = target_copy.transform.rotation;
  }


  private void _spawn_same_random_objects(int count){
    int _random_count = count;
    while(_random_count > 0){
      int _choosen_idx = (int)Mathf.Round(MathExt.Range(0, _CopyContainer.transform.childCount-1, Random.value));
      _spawn_object(_CopyContainer.transform.GetChild(_choosen_idx).gameObject);

      _random_count--;
    }
  }

  private void _spawn_different_random_objects(int count = int.MaxValue){
    List<GameObject> _spawn_list = new();
    for(int i = 0; i < _CopyContainer.transform.childCount; i++)
      _spawn_list.Add(_CopyContainer.transform.GetChild(i).gameObject);

    int _random_count = count;
    while(_spawn_list.Count > 0 && _random_count > 0){
      int _choosen_idx = (int)Mathf.Round(MathExt.Range(0, _spawn_list.Count-1, Random.value));;
      _spawn_object(_spawn_list[_choosen_idx]);
      
      _spawn_list.RemoveAt(_choosen_idx);
      _random_count--;
    }
  }

  private IEnumerator _start_co_func(){
    yield return null;
    yield return new WaitForEndOfFrame();

    for(int i = 0; i < _CopyContainer.transform.childCount; i++)
      _CopyContainer.transform.GetChild(i).gameObject.SetActive(false);
  }


  public void Start(){
    StartCoroutine(_start_co_func());

    foreach(GameObject _obj in _ListTriggerObjectByDistance)
      _trigger_obj_by_dist[_obj.GetInstanceID()] = _obj;
  }

  public void FixedUpdate(){
    if(!TriggerByDistance && !TriggerByTimeout && !TriggerWhenEmpty)
      return;

    bool _trigger_spawn_by_timeout = !TriggerByTimeout;
    if(TriggerByTimeout){
      if(_trigger_timer > 0)
        _trigger_timer -= Time.fixedDeltaTime;

      _trigger_spawn_by_timeout = _trigger_timer <= 0;
    }

    bool _trigger_spawn_by_distance = !TriggerByDistance;
    if(TriggerByDistance){
      _trigger_spawn_by_distance = true;
      foreach(GameObject _trigger_obj in _trigger_obj_by_dist.Values){
        float _dist = (_trigger_obj.transform.position-transform.position).magnitude;
        if(_dist < _MinDistanceToTrigger){
          _trigger_spawn_by_distance = false;
          break;
        }
      }
    }

    bool _trigger_spawn_on_empty = !TriggerWhenEmpty;
    if(TriggerWhenEmpty)
      _trigger_spawn_on_empty = _SpawnedContainer.transform.childCount <= 0;

    if(_trigger_spawn_by_distance && _trigger_spawn_by_timeout && _trigger_spawn_on_empty)
      TriggerSpawn();
  }

  
  public void TriggerSpawn(){
    if(_EnableContainer != null)
      _EnableContainer.SetActive(true);

    if(IgnoreRandomSpawnCount)
      _spawn_different_random_objects();
    else{
      int _random_spawn_count = (int)Mathf.Round(MathExt.Range(_RandomObjectSpawnedMin, _RandomObjectSpawnedMax, Random.value));

      if(_AllowSameObjectToSpawn)
        _spawn_same_random_objects(_random_spawn_count);
      else
        _spawn_different_random_objects(_random_spawn_count);
    }

    _trigger_timer = (int)Mathf.Round(MathExt.Range(_RandomTimerSpawnMin, _RandomTimerSpawnMax, Random.value));
  }

  public void DespawnAllSpawnedObjects(){
    if(_EnableContainer != null)
      _EnableContainer.SetActive(false);

    for(int i = 0; i < _SpawnedContainer.transform.childCount; i++){
      GameObject _child_obj = _SpawnedContainer.transform.GetChild(i).gameObject;
      Destroy(_child_obj);
    }
  }


  public void SetTriggerObjectByDistance(GameObject obj){
    _trigger_obj_by_dist[obj.GetInstanceID()] = obj;
  }

  public void RemoveTriggerObjectByDistance(GameObject obj){
    if(!_trigger_obj_by_dist.ContainsKey(obj.GetInstanceID()))
      return;

    _trigger_obj_by_dist.Remove(obj.GetInstanceID());
  }


  public List<GameObject> GetSpawnedObjectList(){
    List<GameObject> _result = new();
    for(int i = 0; i < _SpawnedContainer.transform.childCount; i++)
      _result.Add(_SpawnedContainer.transform.GetChild(i).gameObject);

    return _result;
  }


  public void OnDisable(){
    DespawnAllSpawnedObjects();
  }
}