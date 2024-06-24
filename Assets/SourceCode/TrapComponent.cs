using UnityEngine;


public class TrapComponent: MonoBehaviour{
  [SerializeField]
  private DamagerComponent _Damager;

  [SerializeField]
  private DamagerComponent.DamagerContext _DamagerContext;
  [SerializeField]
  private DamagerComponent.DamagerData _DamagerData;


  private GameHandler _game_handler;

  public bool IsInitialized{private set; get;} = false;


  private void _game_scene_changed(string scene_id, GameHandler.GameContext context){
    if(!gameObject.activeInHierarchy)
      return;
      
    // it destroys tilemap renderer???
    //_Damager.SetDamagerContext(_DamagerContext);
    _Damager.SetDamagerData(_DamagerData);
  }

  private void _game_scene_removed(){
    _game_handler.SceneChangedFinishedEvent -= _game_scene_changed;
    _game_handler.SceneRemovingEvent -= _game_scene_removed;
  }


  public void OnDestroy(){
    if(!IsInitialized)
      return;

    _game_scene_removed();
  }

  public void Start(){
    _Damager.AllowMultipleHitsSameObject = true;

    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingReferenceException();
    }
  
    _game_handler.SceneChangedFinishedEvent += _game_scene_changed;
    _game_handler.SceneRemovingEvent += _game_scene_removed;

    IsInitialized = true;
    if(_game_handler.SceneInitialized)
      _game_scene_changed(_game_handler.GetCurrentSceneID(), _game_handler.GetCurrentSceneContext());
  }
}