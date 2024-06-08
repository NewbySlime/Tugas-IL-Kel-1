using System.Collections.Generic;
using UnityEngine;


public class EnemyFightMG: MiniGameHandler{
  private struct _object_metadata{
    public GameObject obj;
    public HealthComponent health_component; 
  }


  [SerializeField]
  private List<ObjectSpawner> _ObjectSpawners;

  private Dictionary<int, _object_metadata> _list_watched_enemies = new();

  private Dictionary<int, _object_metadata> _list_watched_allies = new();


  private void _on_enemy_died(){
    List<int> _removed_list = new();
    foreach(int id in _list_watched_enemies.Keys){
      _object_metadata _metadata = _list_watched_enemies[id];
      if(!_metadata.health_component.IsDead())
        continue;

      _metadata.health_component.OnDeadEvent -= _on_enemy_died;

      _OnEnemyDied(_metadata.obj);
      _removed_list.Add(id);
    }

    foreach(int _remove_id in _removed_list)
      _list_watched_enemies.Remove(_remove_id);

    if(_list_watched_enemies.Count <= 0)
      _GameFinished(ResultCase.Win);
  }

  private void _on_ally_died(){
    List<int> _removed_list = new();
    foreach(int id in _list_watched_allies.Keys){
      _object_metadata _metadata = _list_watched_allies[id];
      if(!_metadata.health_component.IsDead())
        continue;

      _metadata.health_component.OnDeadEvent -= _on_enemy_died;

      _OnAllyDied(_metadata.obj);
      _removed_list.Add(id);
    }

    foreach(int _remove_id in _removed_list)
      _list_watched_allies.Remove(_remove_id);

    if(_list_watched_allies.Count <= 0)
      _GameFinished(ResultCase.Lose);
  }


  protected override void _OnGameFinished(ResultCase result){
    base._OnGameFinished(result);

    switch(result){
      case ResultCase.Win:{
        // remove all enemy object
      }break;
    }
  }


  protected virtual void _OnEnemyDied(GameObject obj){}
  protected virtual void _OnEnemyAddedToWatchList(GameObject obj){}

  protected virtual void _OnAllyDied(GameObject obj){}
  protected virtual void _OnAllyAddedToWatchList(GameObject obj){}


  public void AddWatchObject(GameObject obj){
    ObjectFriendlyHandler _friendly_handler = obj.GetComponent<ObjectFriendlyHandler>();
    if(_friendly_handler == null){
      Debug.LogError(string.Format("Object ({0}) does not have ObjectFriendlyHandler.", obj.name));
      return;
    }

    HealthComponent _health = obj.GetComponent<HealthComponent>();
    if(_health == null){
      Debug.LogError(string.Format("Tried to add Object ({0}, FriendlyContext: {1}), but it doesn't have HealthComponent.", obj.name, _friendly_handler.FriendlyContext));
      return;
    }

    _object_metadata _metadata = new(){
      obj = obj,
      health_component = _health
    };

    switch(_friendly_handler.FriendlyContext){
      case ObjectFriendlyHandler.FriendlyType.PlayerFriend:{
        _health.OnDeadEvent += _on_ally_died;
        _list_watched_allies[obj.GetInstanceID()] = _metadata;

        _OnAllyAddedToWatchList(obj);
      }break;

      case ObjectFriendlyHandler.FriendlyType.Enemy:{
        _health.OnDeadEvent += _on_enemy_died;
        _list_watched_enemies[obj.GetInstanceID()] = _metadata;

        _OnEnemyAddedToWatchList(obj);
      }break;


      case ObjectFriendlyHandler.FriendlyType.Neutral:{
        Debug.LogError(string.Format("Adding Object ({0}) with FriendlyContext as Neutral is not supported."));
      }break;
    }
  }

  
  public void AddWatchObjectFromSpawner(ObjectSpawner spawner){
    List<GameObject> _list_spawned = spawner.GetSpawnedObjectList();
    foreach(GameObject _spawned_obj in _list_spawned){
      if(_spawned_obj.GetComponent<ObjectFriendlyHandler>() == null || _spawned_obj.GetComponent<HealthComponent>() == null)
        continue;

      AddWatchObject(_spawned_obj);
    }
  }


  public override void TriggerGameStart(){
    base.TriggerGameStart();

    foreach(ObjectSpawner _spawner in _ObjectSpawners){
      _spawner.TriggerSpawn();

      AddWatchObjectFromSpawner(_spawner);
    }
  }
}