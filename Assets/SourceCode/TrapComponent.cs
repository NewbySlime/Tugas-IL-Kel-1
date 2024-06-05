using UnityEngine;


public class TrapComponent: MonoBehaviour{
  [SerializeField]
  private DamagerComponent _Damager;

  [SerializeField]
  private DamagerComponent.DamagerContext _DamagerContext;
  [SerializeField]
  private DamagerComponent.DamagerData _DamagerData;


  private GameHandler _game_handler;


  private void _game_scene_changed(string scene_id, GameHandler.GameContext context){
    _Damager.SetDamagerContext(_DamagerContext);
    _Damager.SetDamagerData(_DamagerData);
  }

  private void _game_scene_removed(){
    _game_handler.SceneChangedFinishedEvent -= _game_scene_changed;
    _game_handler.SceneRemovingEvent -= _game_scene_removed;
  }


  ~TrapComponent(){
    _game_scene_removed();
  }

  public void Start(){
    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingReferenceException();
    }
  
    _game_handler.SceneChangedFinishedEvent += _game_scene_changed;
    _game_handler.SceneRemovingEvent += _game_scene_removed;

    if(_game_handler.SceneInitialized)
      _game_scene_changed(_game_handler.GetCurrentSceneID(), _game_handler.GetCurrentSceneContext());
  }
}