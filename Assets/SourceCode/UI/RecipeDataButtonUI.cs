using System.Collections;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class RecipeDataButtonUI: ButtonBaseUI, IObjectInitialized{
  [SerializeField]
  private Image _ItemImageUI;

  [SerializeField]
  private TMP_Text _ItemTitle;
  [SerializeField]
  private TMP_Text _ItemDescription;
  [SerializeField]
  private TMP_Text _RecipeDescription;


  private ItemDatabase _item_database;

  private string _current_item_id = "";


  public bool IsInitialized{private get; set;} = false;


  private void _set_item_id(string item_id){
    _current_item_id = item_id;

    _ItemImageUI.sprite = null;
    _ItemTitle.text = "???";
    _ItemDescription.text = "";
    _RecipeDescription.text = "???";

    Debug.Log(string.Format("set item {0} {1}", IsInitialized, _current_item_id));

    if(!IsInitialized || _current_item_id.Length <= 0)
      return;

    TypeDataStorage _recipe_item_data = _item_database.GetItemData(_current_item_id);
    if(_recipe_item_data == null){
      Debug.LogError(string.Format("Item (ID: {0}) does not exist.", _current_item_id));
      return;
    }

    ItemRecipeDiscoveryData.ItemData _recipe_discovery_data = _recipe_item_data.GetData<ItemRecipeDiscoveryData.ItemData>();
    if(_recipe_discovery_data == null){
      Debug.LogError(string.Format("Item (ID: {0}) does not have ItemRecipeDiscoveryData.", _current_item_id));
      return;
    }


    TypeDataStorage _item_data = _item_database.GetItemData(_recipe_discovery_data.RecipeForItemID);
    if(_item_data == null){
      Debug.LogError(string.Format("Item (RecipeDiscoveryID: {0}, ID: {1}) does not exist.", _current_item_id, _recipe_discovery_data.RecipeForItemID));
      return;
    }

    // guaranteed exist
    ItemMetadata.ItemData _item_metadata = _item_data.GetData<ItemMetadata.ItemData>();
    ItemRecipeData.ItemData _item_recipe_data = _item_data.GetData<ItemRecipeData.ItemData>();
    if(_item_recipe_data == null){
      Debug.LogError(string.Format("Item (RecipeDiscoveryID: {0}, ID: {1}) does not have ItemRecipeData.", _current_item_id, _recipe_discovery_data.RecipeForItemID));
      return;
    }

    ItemTextureData.ItemData _item_texture_data = _item_data.GetData<ItemTextureData.ItemData>();
    if(_item_texture_data == null){
      Debug.LogError(string.Format("Item (RecipeDiscoveryID: {0}, ID: {1}) does not have ItemTextureData.", _current_item_id, _recipe_discovery_data.RecipeForItemID));
      return;
    }


    _ItemImageUI.sprite = _item_texture_data.SpriteTexture;
    _ItemTitle.text = _item_metadata.Name;
    _ItemDescription.text = _item_metadata.Description;
  }


  private IEnumerator _start_co_func(){
    yield return null;
    yield return new WaitForEndOfFrame();

    IsInitialized = true;
    _set_item_id(_current_item_id);
  }

  public void Start(){
    _item_database = FindAnyObjectByType<ItemDatabase>();
    if(_item_database == null){
      Debug.LogError("Cannot find database for Items.");
      throw new MissingReferenceException();
    }

    StartCoroutine(_start_co_func());
  }

  public void SetRecipeID(string recipe_item_id){
    _set_item_id(recipe_item_id);
  }


  public bool GetIsInitialized(){
    return IsInitialized;
  }
}