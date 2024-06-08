using System;
using System.Collections;
using System.Numerics;
using System.Reflection;
using JetBrains.Annotations;
using SequenceHelper;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;



/// <summary>
/// Komponen untuk mengontrol Objek Game berdasarkan input dari Player.
/// </summary>
[RequireComponent(typeof(MovementController))]
[RequireComponent(typeof(WeaponHandler))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(InventoryData))]
[RequireComponent(typeof(RecipeDiscoveryComponent))]
[RequireComponent(typeof(OnDeadGameOverTrigger))]
public class PlayerController: MonoBehaviour{
  private const string _PlayerRuntimeDataID = "player_data";

  public static ObjectReference.ObjRefID PlayerDefaultRefID = new(){
    ID = "player_object"
  };

  public static RegisterInputFocusSequence.InputFocusData PlayerInputContext = new(){
    RefID = PlayerDefaultRefID,
    InputContext = InputFocusContext.ContextEnum.Player
  };


  [Serializable]
  public class RuntimeData: PersistanceContext.IPersistance{
    public HealthComponent.RuntimeData PlayerHealth = new();
    
    public uint CurrentAmmoCount;
    public float CurrentAmmoRegenTime;

    public RecipeDiscoveryComponent.RuntimeData DiscoveredRecipe;


    public string GetDataID(){
      return "Player.RuntimeData";
    }

    
    public string GetData(){
      return ConvertExt.ToBase64String(JsonUtility.ToJson(this));
    }

    public void SetData(string data){
      JsonUtility.FromJsonOverwrite(ConvertExt.FromBase64String(data), this);
    }
  }


  [SerializeField]
  private WeaponHandler _WeaponHandler;

  [SerializeField]
  private string _WeaponItemID;
  [SerializeField]
  private float _WeaponRegenTime = 4f;
  [SerializeField]
  private uint _WeaponCount = 4;

  [SerializeField]
  private InteractionHandler _InteractionFront;

  /*
  [SerializeField]
  private InteractionHandler _InteractionDirect;
  */

  [SerializeField]
  private PickableObjectPickerHandler _PickerHandler;

  [SerializeField]
  private Animator _TargetAnimator;


  private AnimationTriggerFlagComponent _anim_trigger_flag;

  private GameHandler _game_handler;
  private PersistanceContext _persistance_handler;

  private GameUIHandler _ui_handler;

  private MovementController _movement_controller;
  private InputFocusContext _input_context;

  private MouseFollower _mouse_follower;

  private InventoryData _inv_data;
  private RecipeDiscoveryComponent _recipe_discovery;

  private HealthComponent _health_component;
  private OnDeadGameOverTrigger _dead_game_over_trigger;

  private MultipleProgressBar _weapon_counter_ui = null;

  private uint _current_weapon_count;
  private float _weapon_regen_timer = 0;

  private Coroutine _weapon_fire_coroutine = null;
  private bool _weapon_fire_trigger_flag = false;
  private bool _weapon_fire_finished_flag = false;


  public bool TriggerGameOverOnDead = true;
  public bool DisableMovementOnDead = true;

  public bool TriggerAvailable{private set; get;} = true;


  private void _update_ammo_counter_ui(){
    if(_weapon_counter_ui == null)
      return;

    float _progress = _current_weapon_count;
    if(_weapon_regen_timer > 0)
      _progress += 1-Mathf.Clamp(_weapon_regen_timer/_WeaponRegenTime, 0, float.PositiveInfinity);
    
    _weapon_counter_ui.SetProgress(_progress);
  }

  private void _on_anim_trigger(string trigger_name){
    Debug.Log(string.Format("weapon trigger anim trigger {0}", trigger_name));
    switch(trigger_name){
      case "weapon_trigger":{
        _weapon_fire_trigger_flag = true;
      }break;

      case "weapon_trigger_finished":{
        _weapon_fire_finished_flag = true;
      }break;
    }
  }


  private IEnumerator _trigger_weapon_fire(){
    if(_TargetAnimator != null){
      _movement_controller.FixedLookAt = true;
      _movement_controller.GlideOnGround = true;
      _movement_controller.SetEnableMovement(false);

      _weapon_fire_trigger_flag = false;
      _weapon_fire_finished_flag = false;

      _TargetAnimator.SetBool("is_attacking", true);
      while(!_weapon_fire_trigger_flag){
        UnityEngine.Vector2 _dir = (_mouse_follower.transform.position-transform.position).normalized;
        _movement_controller.LookAt(_dir);
        
        yield return null;
      }
    }

    _WeaponHandler.TriggerWeapon();
    _current_weapon_count--;

    if(_TargetAnimator != null){
      _TargetAnimator.SetBool("is_attacking", false);
      yield return new WaitUntil(() => _weapon_fire_finished_flag);

      _movement_controller.FixedLookAt = false;
      _movement_controller.GlideOnGround = false;
      _movement_controller.SetEnableMovement(true);
    }

    _weapon_fire_coroutine = null;
  }


  private void _scene_changed(string scene_id, GameHandler.GameContext context){
    Debug.Log("input added");
    ObjectReference.SetReferenceObject(PlayerDefaultRefID, gameObject);
    _input_context.RegisterInputObject(this, InputFocusContext.ContextEnum.Player);

    _WeaponHandler.SetWeaponItem(_WeaponItemID);

    PlayerHUDUI _player_ui = _ui_handler.GetPlayerHUDUI();
    HealthBarUI _health_bar_ui = _player_ui.GetHealthBar();
    _health_bar_ui.BindHealthComponent(_health_component);

    _weapon_counter_ui = _player_ui.GetAmmoCounter();
    _weapon_counter_ui.SetProgressCount((int)_WeaponCount);

    _ui_handler.GetRecipeBookUI().BindDiscoveryComponent(_recipe_discovery);

    _game_handler.LoadDataFromPersistanceEvent += _persistance_loading;

    _persistance_handler = _game_handler.PersistanceHandler;
    _persistance_handler.PersistanceSavingEvent += _persistance_saving;

    GameRuntimeData _runtime_data = FindAnyObjectByType<GameRuntimeData>();
    if(_runtime_data == null){
      Debug.LogWarning("No Runtime data storage, Player cannot retain data from last scene.");
    }
    else{
      // Inventory data
      InventoryData.RuntimeData _inv_rdata = _runtime_data.GetData<InventoryData.RuntimeData>(_PlayerRuntimeDataID);
      _inv_data.FromRuntimeData(_inv_rdata);

      // Recipe Discovery
      RecipeDiscoveryComponent.RuntimeData _recipe_rdata = _runtime_data.GetData<RecipeDiscoveryComponent.RuntimeData>(_PlayerRuntimeDataID);
      _recipe_discovery.FromRuntimeData(_recipe_rdata);

      // Self data
      RuntimeData _this_rdata = _runtime_data.GetData<RuntimeData>(_PlayerRuntimeDataID);
      FromRuntimeData(_this_rdata);
    }
  }

  private void _scene_removing(){
    Debug.Log("input removed");
    _input_context.RemoveInputObject(this, InputFocusContext.ContextEnum.Player);

    _game_handler.SceneChangedFinishedEvent -= _scene_changed;
    _game_handler.SceneRemovingEvent -= _scene_removing;

    _game_handler.LoadDataFromPersistanceEvent -= _persistance_loading;

    _persistance_handler.PersistanceSavingEvent -= _persistance_saving;

    _input_context.FocusContextRegisteredEvent -= _another_focus_context_registered;


    GameRuntimeData _runtime_data = FindAnyObjectByType<GameRuntimeData>();
    if(_runtime_data == null){
      Debug.LogWarning("No Runtime Data Storage, Player data would not be transferred to next scene.");
      return;
    }
    else{
      // Inventory data
      InventoryData.RuntimeData _rdata = _inv_data.AsRuntimeData();
      _runtime_data.SetData(_PlayerRuntimeDataID, _rdata);

      // Recipe Discovery data
      RecipeDiscoveryComponent.RuntimeData _recipe_rdata = _recipe_discovery.AsRuntimeData();
      _runtime_data.SetData(_PlayerRuntimeDataID, _recipe_rdata);

      // Self data
      _runtime_data.SetData(_PlayerRuntimeDataID, AsRuntimeData());
    }
  }


  private void _persistance_saving(PersistanceContext context){
    // Inventory data
    InventoryData.RuntimeData _rdata = _inv_data.AsRuntimeData();
    context.ParseData(_rdata);

    // Recipe Discovery data
    RecipeDiscoveryComponent.RuntimeData _recipe_rdata = _recipe_discovery.AsRuntimeData();
    context.ParseData(_recipe_rdata);

    // Self data
    context.ParseData(AsRuntimeData());
  }

  private void _persistance_loading(PersistanceContext context){
    // Inventory data
    InventoryData.RuntimeData _rdata = new InventoryData.RuntimeData();
    context.OverwriteData(_rdata);

    _inv_data.FromRuntimeData(_rdata);

    // Recipe Discovery data
    RecipeDiscoveryComponent.RuntimeData _recipe_rdata = new RecipeDiscoveryComponent.RuntimeData();
    context.OverwriteData(_recipe_rdata);

    _recipe_discovery.FromRuntimeData(_recipe_rdata);

    // Self data
    RuntimeData _this_rdata = new();
    context.OverwriteData(_this_rdata);
    FromRuntimeData(_this_rdata);
  }


  private void _player_on_death(){
    TriggerAvailable = false;

    if(DisableMovementOnDead)
      _movement_controller.SetEnableMovement(false);

    if(TriggerGameOverOnDead){
      _ui_handler.ResetMainUIMode();
      _input_context.RemoveInputObject(this, InputFocusContext.ContextEnum.Player);
    }
    else{
      _dead_game_over_trigger.TriggerEnable = false;
      _dead_game_over_trigger.CancelTrigger();
    }
  }

  private void _player_on_discovered_recipe(string recipe_item_id){
    _game_handler.TriggerRecipeAdded(recipe_item_id);
  }


  private void _reset_input(){
    _movement_controller.DoWalk(0);
  }

  private void _another_focus_context_registered(){
    if(_input_context.InputAvailable(this))
      return;

    _reset_input();
  }


  ~PlayerController(){
    _scene_removing();
  }


  public void Start(){
    TriggerAvailable = true;

    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find Game Handler.");
      throw new MissingReferenceException();
    }

    _game_handler.SceneChangedFinishedEvent += _scene_changed;
    _game_handler.SceneRemovingEvent += _scene_removing;


    if(_TargetAnimator != null){
      _anim_trigger_flag = GetComponent<AnimationTriggerFlagComponent>();
      if(_anim_trigger_flag == null){
        Debug.LogError("TargetAnimator exist, but cannot get AnimationTriggerFlagComponent.");
        throw new MissingReferenceException();
      }

      _anim_trigger_flag.AnimationTriggerEvent += _on_anim_trigger;
    }


    _health_component = GetComponent<HealthComponent>();
    _health_component.OnDeadEvent += _player_on_death;

    _dead_game_over_trigger = GetComponent<OnDeadGameOverTrigger>();

    _movement_controller = GetComponent<MovementController>();
    _input_context = FindAnyObjectByType<InputFocusContext>();
    if(_input_context == null){
      Debug.LogError("Cannot find InputFocusContext.");
      throw new MissingReferenceException();
    }

    _input_context.FocusContextRegisteredEvent += _another_focus_context_registered;

    _inv_data = GetComponent<InventoryData>();
    _recipe_discovery = GetComponent<RecipeDiscoveryComponent>();
    _recipe_discovery.OnRecipeDiscoveredEvent += _player_on_discovered_recipe;


    _mouse_follower = FindAnyObjectByType<MouseFollower>();
    if(_mouse_follower == null){
      Debug.LogWarning("Cannot get MouseObject.");
    }


    _ui_handler = FindAnyObjectByType<GameUIHandler>();
    if(_ui_handler == null){
      Debug.LogError("Cannot find GameUIHandler.");
      throw new MissingReferenceException();
    }

    if(_game_handler.SceneInitialized)
      _scene_changed(_game_handler.GetCurrentSceneID(), _game_handler.GetCurrentSceneContext());
  }


  public void Update(){
    _update_ammo_counter_ui();
  }

  public void FixedUpdate(){
    if(_mouse_follower != null){
      _WeaponHandler.LookAt(_mouse_follower.transform.position);
    }

    if(_current_weapon_count < _WeaponCount){
      if(_weapon_regen_timer < 0)
        _weapon_regen_timer = _WeaponRegenTime;

      _weapon_regen_timer -= Time.fixedDeltaTime;
      if(_weapon_regen_timer < 0)
        _current_weapon_count++;
    }
  }


  /// <summary>
  /// Input Handling ketika Player memberikan input untuk bergerak secara horizontal.
  /// </summary>
  /// <param name="value">Value yang diberikan Unity.</param>
  public void OnStrafe(InputValue value){
    if(!_input_context.InputAvailable(this))
      return;

    float _strafe_value = value.Get<float>();
    _movement_controller.DoWalk(_strafe_value);
  }

  /// <summary>
  /// Input Handling ketika Player memberikan input untuk melompat.
  /// </summary>
  /// <param name="value">Value yang diberikan Unity.</param>
  public void OnJump(InputValue value){
    if(!_input_context.InputAvailable(this))
      return;
      
    if(value.isPressed)
      _movement_controller.DoJump();
  }

  public void OnFire(InputValue value){
    if(!_input_context.InputAvailable(this))
      return;

    if(value.isPressed){
      if(false && _PickerHandler.GetHasObject()){
        _PickerHandler.ThrowObject((_mouse_follower.transform.position-transform.position).normalized);
        return;
      }

      if(_weapon_fire_coroutine == null && _current_weapon_count > 0 && _WeaponHandler.CanShoot()){
        StartCoroutine(_trigger_weapon_fire());
        return;
      }
    }
  }

  public void OnInteract(InputValue value){
    if(!_input_context.InputAvailable(this))
      return;

    if(value.isPressed){
      /*
      if(_InteractionDirect.TriggerInteraction())
        return;
      */
      
      if(_InteractionFront.TriggerInteraction())
        return;
    }
  }


  public RuntimeData AsRuntimeData(){
    RuntimeData _this_rdata = new(){
      PlayerHealth = _health_component.AsRuntimeData(),

      CurrentAmmoCount = _current_weapon_count,
      CurrentAmmoRegenTime = _weapon_regen_timer,

      DiscoveredRecipe = _recipe_discovery.AsRuntimeData()
    };

    return _this_rdata;
  }

  public void FromRuntimeData(RuntimeData data){
    if(data == null)
      return;

    _health_component.FromRuntimeData(data.PlayerHealth);
    
    _current_weapon_count = data.CurrentAmmoCount;
    _weapon_regen_timer = data.CurrentAmmoRegenTime;

    _recipe_discovery.FromRuntimeData(data.DiscoveredRecipe);
  }
}