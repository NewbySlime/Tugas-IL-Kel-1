using JetBrains.Annotations;
using UnityEngine;



/// <summary>
/// Class to handle Minigame system for when Boss Battles in the Game. Win/Lose Condition almost the same as the EnemyFightMG.
/// 
/// NOTE: bound objects should have <see cref="HealthComponent"/> and <see cref="ObjectFriendlyHandler"/> for the Minigame to work properly.
/// 
/// <seealso cref="EnemyFightMG"/>
/// <seealso cref="MiniGameHandler"/>
/// </summary>
public class BossFightMG: EnemyFightMG{
  private GameUIHandler _ui_handler;

  private GameObject _boss_object = null;
  private HealthComponent _boss_health = null;
  private ObjectFriendlyHandler _boss_friendly_context = null;

  private void _unbind_boss_object(){
    if(_boss_object == null)
      return;

    _boss_health.OnDeadEvent -= _on_boss_dead;

    _boss_object = null;
    _boss_health = null;
    _boss_friendly_context = null;
  }

  private void _bind_boss_object(GameObject boss_object){
    _boss_friendly_context = boss_object.GetComponent<ObjectFriendlyHandler>();
    if(_boss_friendly_context == null){
      Debug.LogError(string.Format("Boss Object ({0}) does not have ObjectFriendlyHandler.", boss_object.name));
      return;
    }

    _boss_health = boss_object.GetComponent<HealthComponent>();
    if(_boss_health == null){
      Debug.LogError(string.Format("Tried to add Object ({0}), but it doesn't have HealthComponent.", boss_object.name));
      return;
    }

    _boss_health.OnDeadEvent += _on_boss_dead;

    _boss_friendly_context.SetFriendlyType(ObjectFriendlyHandler.FriendlyType.Enemy);
    _ui_handler.GetBossHealthBarUI().BindBossObject(boss_object);

    _boss_object = boss_object;
  }


  private void _show_boss_health_bar_ui(){
    if(_boss_object == null || !IsMiniGameRunning)
      return;

    _ui_handler.SetMainHUDUIMode(GameUIHandler.MainHUDUIEnum.BossHealthBarUI, true);
  }


  private void _on_boss_dead(){
    _GameFinished(ResultCase.Win);
  }


  protected override void _OnGameFinished(ResultCase result){
    base._OnGameFinished(result);

    DEBUGModeUtils.Log("mini game hide");
    _ui_handler.SetMainHUDUIMode(GameUIHandler.MainHUDUIEnum.BossHealthBarUI, false);
  }


  public new void Start(){
    base.Start();

    _ui_handler = FindAnyObjectByType<GameUIHandler>();
    if(_ui_handler == null){
      Debug.LogError("Cannot get GameUIHandler.");
      throw new MissingReferenceException();
    }
  }


  /// <summary>
  /// Bound enemy (in this case Boss) object for the win condition.
  /// The target object must have <see cref="HealthComponent"/> and <see cref="ObjectFriendlyHandler"/>.
  /// </summary>
  /// <param name="boss_object">The target Boss object</param>
  public void SetWatchBoss(GameObject boss_object){
    _unbind_boss_object();
    _bind_boss_object(boss_object);

    AddWatchObject(boss_object);
    _show_boss_health_bar_ui();
  }

  public override void TriggerGameStart(){
    base.TriggerGameStart();
    _show_boss_health_bar_ui();
  }
}