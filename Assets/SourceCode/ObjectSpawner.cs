using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Class for spawing any objects (stored as inactive object in certain container) whenever it got triggered.
/// The class also handles auto spawning with some conditions that can be modified with available variables.
/// 
/// This class uses external component(s);
/// - <b>GameObject</b>s as containers that holds objects used in this class.
/// - Target <b>GameObject</b>(s) for condition checking (minimal distance from the spawner for auto spawn to be triggered).
/// </summary>
public class ObjectSpawner: MonoBehaviour{
  [SerializeField]
  // The source of the spawn objects.
  private GameObject _CopyContainer = null;

  [SerializeField]
  // Container for spawned objects.
  private GameObject _SpawnedContainer;

  [SerializeField]
  // Container used for enabling objects when the spawner is spawning or despawning.
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


  /// <summary>
  /// Auto spawn, use condition of checking distance between spawner and target trigger objects.
  /// </summary>
  public bool TriggerByDistance = true;
  /// <summary>
  /// Auto spawn, use condition of waiting a cooldown from spawning.
  /// </summary>
  public bool TriggerByTimeout = true;
  /// <summary>
  /// Auto spawn, use condition of waiting until all spawned objects are destroyed.
  /// </summary>
  public bool TriggerWhenEmpty = true;

  /// <summary>
  /// Should the spawner use random count of how many objects can it spawn in one spawn trigger.
  /// </summary>
  public bool IgnoreRandomSpawnCount = false;


  // Helper function to spawn (copy) a target object.
  private void _spawn_object(GameObject target_copy){
    GameObject _copied_obj = Instantiate(target_copy);
    _copied_obj.transform.SetParent(_SpawnedContainer.transform);
    _copied_obj.SetActive(true);

    _copied_obj.transform.position = target_copy.transform.position;
    _copied_obj.transform.rotation = target_copy.transform.rotation;
  }


  // Spawn, allow reoccuring same objects to spawn.
  private void _spawn_same_random_objects(int count){
    int _random_count = count;
    while(_random_count > 0){
      int _choosen_idx = (int)Mathf.Round(MathExt.Range(0, _CopyContainer.transform.childCount-1, Random.value));
      _spawn_object(_CopyContainer.transform.GetChild(_choosen_idx).gameObject);

      _random_count--;
    }
  }

  // Spawn, prohibit spawning same objects that already been spawned.
  // When the spawn count is higher than the source spawn objects, it will be capped with the source count.
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
    // checking conditions for triggering auto spawn
    if(!TriggerByDistance && !TriggerByTimeout && !TriggerWhenEmpty)
      return;

    // spawn cooldown
    bool _trigger_spawn_by_timeout = !TriggerByTimeout;
    if(TriggerByTimeout){
      if(_trigger_timer > 0)
        _trigger_timer -= Time.fixedDeltaTime;

      _trigger_spawn_by_timeout = _trigger_timer <= 0;
    }

    // spawn when all target trigger objects are outside the minimal range
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

    // spawn when all spawned objects are destroyed
    bool _trigger_spawn_on_empty = !TriggerWhenEmpty;
    if(TriggerWhenEmpty)
      _trigger_spawn_on_empty = _SpawnedContainer.transform.childCount <= 0;

    if(_trigger_spawn_by_distance && _trigger_spawn_by_timeout && _trigger_spawn_on_empty)
      TriggerSpawn();
  }

  
  /// <summary>
  /// Immediately trigger spawning objects.
  /// Also enabling the EnableContainer.
  /// </summary>
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

  /// <summary>
  /// Function to despawn all currently spawned objects that is not destroyed yet.
  /// Also disabling the EnableContainer.
  /// </summary>
  public void DespawnAllSpawnedObjects(){
    if(_EnableContainer != null)
      _EnableContainer.SetActive(false);

    for(int i = 0; i < _SpawnedContainer.transform.childCount; i++){
      GameObject _child_obj = _SpawnedContainer.transform.GetChild(i).gameObject;
      Destroy(_child_obj);
    }
  }


  /// <summary>
  /// Add another target trigger object used for spawning by distance condition.
  /// </summary>
  /// <param name="obj">The trigger object</param>
  public void SetTriggerObjectByDistance(GameObject obj){
    _trigger_obj_by_dist[obj.GetInstanceID()] = obj;
  }

  /// <summary>
  /// Remove certain trigger object used for spawning by distance condition.
  /// </summary>
  /// <param name="obj">The trigger object</param>
  public void RemoveTriggerObjectByDistance(GameObject obj){
    if(!_trigger_obj_by_dist.ContainsKey(obj.GetInstanceID()))
      return;

    _trigger_obj_by_dist.Remove(obj.GetInstanceID());
  }


  /// <summary>
  /// Get a list of spawned object(s) that are still alive.
  /// </summary>
  /// <returns></returns>
  public List<GameObject> GetSpawnedObjectList(){
    List<GameObject> _result = new();
    for(int i = 0; i < _SpawnedContainer.transform.childCount; i++)
      _result.Add(_SpawnedContainer.transform.GetChild(i).gameObject);

    return _result;
  }


  /// <summary>
  /// Function to catch Unity's "Object Disabled" event.
  /// </summary>
  public void OnDisable(){
    DespawnAllSpawnedObjects();
  }
}