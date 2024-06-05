using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class CookerComponent: MonoBehaviour{
  public class ItemCookResult{
    public float FoodScore;
  }


  [SerializeField]
  private BaseProgressUI _CookingProgressUI;

  [SerializeField]
  private float _CookTimePerItem = 4;

  [SerializeField]
  private float _QTEAcceptBarSizeMax = 0.3f;
  [SerializeField]
  private float _QTEAcceptBarSizeMin = 0.15f;

  [SerializeField]
  private float _QTETimeMax = 6f;
  [SerializeField]
  private float _QTETimeMin = 4f;
  
  [SerializeField]
  private float _QTESpeedMax = 2f;
  [SerializeField]
  private float _QTESpeedMin = 0.4f;

  [SerializeField]
  private float _QTEMultMin = 0.8f;

  [SerializeField]
  private float _CookingMultMin = 0.2f;



  private ItemDatabase _item_database;
  private ItemRecipeDatabase _recipe_database;

  private GameUIHandler _ui_handler;

  private InputFocusContext _input_context;

  private bool _interacted_flag = false;


  private IEnumerator _cook_new_item(string item_id, ItemRecipeData.ItemData recipe_data){
    QuickTimeEventUI _qte_ui = _ui_handler.GetQTEUI();
    
    _ui_handler.SetMainHUDUIMode(GameUIHandler.MainHUDUIEnum.QTEUI, true);
    _input_context.RegisterInputObject(this, InputFocusContext.ContextEnum.Player);

    float _total_score = 0;
    foreach(string _ingredient_id in recipe_data.ItemList){
      TypeDataStorage _ingredient_data = _item_database.GetItemData(_ingredient_id);
      if(_ingredient_data == null)
        Debug.LogWarning(string.Format("Ingredient (ID: {0}) is not existed.", _ingredient_id));
      else{
        ItemTextureData.ItemData _tex_data = _ingredient_data.GetData<ItemTextureData.ItemData>();
        if(_tex_data == null)
          Debug.LogWarning(string.Format("Ingredient (ID: {0}) does not have Texture Data.", _ingredient_id));
        else{
          _qte_ui.SetEventSymbol(_tex_data.SpriteTexture);
        }
      }

      float _accept_size = UnityEngine.Random.Range(_QTEAcceptBarSizeMin, _QTEAcceptBarSizeMax);
      float _accept_high = _accept_size/2 + 0.5f;
      float _accept_low = _accept_size/2 - 0.5f;

      _qte_ui.SetAcceptBarMaxSize(_accept_high);
      _qte_ui.SetAcceptBarMinSize(_accept_low);

      float _base_qte_time = UnityEngine.Random.Range(_QTETimeMin, _QTETimeMax);
      float _qte_time = _base_qte_time;
      
      // left to right
      float _current_val = 1;
      _interacted_flag = false;
      while(true){
        float __time_val = _qte_time/_base_qte_time; 
        _qte_ui.SetAcceptBarColorLerp(__time_val);

        float _bar_val = Mathf.Abs(1-Mathf.Repeat(_current_val, 2));
        _qte_ui.SetQTETimingBar(_bar_val);
        
        yield return null;
        if(_qte_time > 0)
          _qte_time -= Time.deltaTime;
        else
          _qte_time = 0;

        float _current_speed = (_QTESpeedMax-_QTESpeedMin) * __time_val + _QTESpeedMin;
        _current_val += _current_speed * Time.deltaTime;

        if(_interacted_flag)
          break;
      }

      float _time_val = _qte_time/_base_qte_time;
      float _mult = (1-_QTEMultMin) * _time_val + _QTEMultMin;

      _total_score += recipe_data.FoodScore / recipe_data.ItemList.Count * _mult;
    }
    
    _ui_handler.SetMainHUDUIMode(GameUIHandler.MainHUDUIEnum.QTEUI, false);
    _input_context.RemoveInputObject(this, InputFocusContext.ContextEnum.Player);

    float _cooking_timer =_CookTimePerItem * recipe_data.ItemList.Count;
  }


  public void Start(){
    _item_database = FindAnyObjectByType<ItemDatabase>();
    if(_item_database == null){
      Debug.LogError("Cannot find database for Items.");
      throw new MissingReferenceException();
    }

    _recipe_database = FindAnyObjectByType<ItemRecipeDatabase>();
    if(_recipe_database == null){
      Debug.LogError("Cannot find database for Recipes.");
      throw new MissingReferenceException();
    }

    _ui_handler = FindAnyObjectByType<GameUIHandler>();
    if(_ui_handler == null){
      Debug.LogError("Cannot get GameUIHandler.");
      throw new MissingReferenceException();
    }

    _input_context = FindAnyObjectByType<InputFocusContext>();
    if(_input_context == null){
      Debug.LogError("Cannot get InputFocusContext.");
      throw new MissingReferenceException();
    }
  }


  public void InteractableInterface_Interact(){

  }


  public bool CookItem(string item_id){
    TypeDataStorage _item_data = _item_database.GetItemData(item_id);
    if(_item_data == null){
      Debug.LogWarning(string.Format("Cooking cancelled, item (ID: {0}) is not exist.", item_id));
      return false;
    }

    ItemRecipeData.ItemData _recipe_data = _item_data.GetData<ItemRecipeData.ItemData>();
    if(_recipe_data == null){
      Debug.LogWarning(string.Format("Cooking cancelled, item (ID: {0}) does not have ItemRecipeData.", item_id));
      return false;
    }

    StartCoroutine(_cook_new_item(item_id, _recipe_data));
    return true;
  }


  public void OnInteract(InputValue value){
    if(!_input_context.InputAvailable(this))
      return;

    if(value.isPressed)
      _interacted_flag = true;
  }
}