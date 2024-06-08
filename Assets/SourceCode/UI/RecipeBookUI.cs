using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SequenceHelper;
using Unity.VisualScripting;
using UnityEngine;


public class RecipeBookUI: MonoBehaviour{
  [Serializable]
  private struct _recipe_discovery_effect_data{
    public float DelayShow;
    public float FinishDelay;
  }

  private struct _recipe_metadata{
    public RecipeDataButtonUI _recipe_ui;
  }

  private class _trigger_recipe_discovery_queue_data{
    public bool AllowTrigger = false;
  }


  [SerializeField]
  private GameObject _RecipeDataButton;

  [SerializeField]
  private GameObject _RecipeDataContainer;

  [SerializeField]
  private _recipe_discovery_effect_data _NewRecipeEffectData;


  private RecipeDiscoveryComponent _discovery_component = null;
  private ItemDatabase _item_database;

  private Queue<_trigger_recipe_discovery_queue_data> _recipe_discovery_queue = new();
  private Dictionary<string, _recipe_metadata> _recipe_metadata_map = new();

  public bool IsEffectTriggering{private set; get;} = false;


  private void _sort_recipe_ui(){
    // data from dictionary should have sorted
    foreach(string _recipe_id in _recipe_metadata_map.Keys){
      _recipe_metadata _metadata = _recipe_metadata_map[_recipe_id];
      _metadata._recipe_ui.transform.SetAsLastSibling();
    }
  }

  private IEnumerator _add_recipe_metadata(string recipe_item_id){
    if(_recipe_metadata_map.ContainsKey(recipe_item_id)){
      Debug.LogWarning(string.Format("RecipeUI for Item (ID: {0}) already exists.", recipe_item_id));
      yield break;
    }

    TypeDataStorage _item_data = _item_database.GetItemData(recipe_item_id);
    if(_item_data == null){
      Debug.LogError(string.Format("Item (ID: {0}) does not exist.", recipe_item_id));
      yield break;
    }

    ItemRecipeDiscoveryData.ItemData _discovery_data = _item_data.GetData<ItemRecipeDiscoveryData.ItemData>();
    if(_discovery_data == null){
      Debug.LogError(string.Format("Item (ID: {0}) does not have ItemRecipeDiscoveryData.", recipe_item_id));
      yield break;
    } 

    GameObject _recipe_obj = Instantiate(_RecipeDataButton);
    _recipe_obj.SendMessage("TriggerSetOnEnable", false, SendMessageOptions.DontRequireReceiver);

    RecipeDataButtonUI _recipe_ui = _recipe_obj.GetComponent<RecipeDataButtonUI>();
    _recipe_ui.SetRecipeID(recipe_item_id);

    _recipe_metadata_map[recipe_item_id] = new(){
      _recipe_ui = _recipe_ui
    };

    yield return new WaitUntil(() => ObjectUtility.IsObjectInitialized(_recipe_obj));

    _recipe_obj.transform.SetParent(_RecipeDataContainer.transform);
    _recipe_obj.transform.SetAsLastSibling();

    _recipe_obj.transform.localScale = Vector3.one;
  }

  private void _remove_recipe_metadata(string recipe_item_id){
    if(!_recipe_metadata_map.ContainsKey(recipe_item_id))
      return;

    _recipe_metadata _metadata = _recipe_metadata_map[recipe_item_id];
    Destroy(_metadata._recipe_ui.gameObject);

    _recipe_metadata_map.Remove(recipe_item_id);
  }


  private IEnumerator _trigger_recipe_discovery_effect(string recipe_item_id){
    if(!IsEffectTriggering){
      IsEffectTriggering = true;
    }
    else{
      _trigger_recipe_discovery_queue_data _queue_data = new();
      _recipe_discovery_queue.Enqueue(_queue_data);

      yield return new WaitUntil(() => _queue_data.AllowTrigger);

      _recipe_discovery_queue.Dequeue();
    }

    // wait until this object received event from discovery component
    yield return new WaitForEndOfFrame();

    if(!_recipe_metadata_map.ContainsKey(recipe_item_id))
      yield return _add_recipe_metadata(recipe_item_id);

    if(!_recipe_metadata_map.ContainsKey(recipe_item_id))
      yield break;
      
    _recipe_metadata _metadata = _recipe_metadata_map[recipe_item_id];
    RecipeDataButtonUI _recipe_ui = _metadata._recipe_ui;

    yield return new WaitUntil(() => ObjectUtility.IsObjectInitialized(_recipe_ui));

    _recipe_ui.gameObject.SendMessage("TriggerSetOnEnable", true, SendMessageOptions.DontRequireReceiver);
    _recipe_ui.gameObject.SetActive(false);

    yield return new WaitForSeconds(_NewRecipeEffectData.DelayShow);
    _recipe_ui.gameObject.SetActive(true);

    if(_recipe_discovery_queue.Count <= 0){
      yield return new WaitForSeconds(_NewRecipeEffectData.FinishDelay);
      IsEffectTriggering = false;
    }
    else{
      _trigger_recipe_discovery_queue_data _next_queue_data = _recipe_discovery_queue.Peek();
      _next_queue_data.AllowTrigger = true;
    }
  }


  private void _on_discovery_added(string recipe_item_id){
    StartCoroutine(_add_recipe_metadata(recipe_item_id));
  }


  public void Start(){
    GameObject _test_obj = Instantiate(_RecipeDataButton);
    if(_test_obj.GetComponent<RecipeDataButtonUI>() == null){
      Debug.LogError("RecipeDataButton Prefab does not have RecipeDataButtonUI.");
      throw new MissingReferenceException();
    }

    Destroy(_test_obj);

    _item_database = FindAnyObjectByType<ItemDatabase>();
    if(_item_database == null){
      Debug.LogError("Cannot find database for Items.");
      throw new MissingReferenceException();
    }
  }


  public void BindDiscoveryComponent(RecipeDiscoveryComponent discovery){
    UnbindDiscoveryComponent();
    if(discovery == null)
      return;

    discovery.OnRecipeDiscoveredEvent += _on_discovery_added;
    _discovery_component = discovery;

    List<string> _list_known = _discovery_component.GetListKnownRecipe();
    foreach(string _recipe_id in _list_known)
      StartCoroutine(_add_recipe_metadata(_recipe_id));
  }

  public void UnbindDiscoveryComponent(){
    if(_discovery_component == null)
      return;

    _discovery_component.OnRecipeDiscoveredEvent -= _on_discovery_added;
    _discovery_component = null;

    foreach(string _recipe_id in _recipe_metadata_map.Keys)
      _remove_recipe_metadata(_recipe_id);
  }


  public void TriggerRecipeDiscoveryEffect(string recipe_item_id){
    StartCoroutine(_trigger_recipe_discovery_effect(recipe_item_id));
  }


  public void OnEnable(){
    _sort_recipe_ui();
  }
}