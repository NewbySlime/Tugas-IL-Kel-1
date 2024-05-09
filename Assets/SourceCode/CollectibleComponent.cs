using UnityEngine;


public class CollectibleComponent: MonoBehaviour{
  [SerializeField]
  private RigidbodyMessageRelay _TargetRigidbody;

  [SerializeField]
  private SpriteRenderer _ItemSpriteRenderer;

  [SerializeField]
  private string _ItemID;

  [SerializeField]
  private uint _ItemCount = 1;


  private ItemDatabase _item_database;

  private string _item_id;
  private uint _item_count = 1;


  private void _collider_entered(Collider2D collider){
    Debug.Log(string.Format("collider name {0}", collider.gameObject.name));
    InventoryData _inv = collider.gameObject.GetComponent<InventoryData>();
    if(_inv == null)
      return;

    _inv.AddItem(_item_id, _item_count);
    Destroy(gameObject);
  }

  private void _item_database_on_initialized(){
    SetItemID(_ItemID);
    SetItemCount(_ItemCount);
  }


  public void Start(){
    _TargetRigidbody.OnTriggerEntered2DEvent += _collider_entered;

    _item_database = FindAnyObjectByType<ItemDatabase>();
    if(_item_database == null){
      Debug.LogWarning("ItemDatabase is not exist.");
      return;
    }
    else{
      _item_database.OnInitializedEvent += _item_database_on_initialized;
      if(_item_database.IsInitialized)
        _item_database_on_initialized();
    }
  }


  #nullable enable
  public void SetItemID(string item_id){
    if(_item_database == null)
      return;

    TypeDataStorage? _item_data = _item_database.GetItemData(item_id);
    if(_item_data == null){
      Debug.LogError(string.Format("Item ID '{0}' is not exist.", item_id));
      return;
    }

    ItemTextureData.ItemData? _item_texture = _item_data.GetData<ItemTextureData.ItemData>();
    if(_item_texture == null){
      Debug.LogError(string.Format("No texture for item ID: '{0}'", item_id));
      return;
    }

    _ItemSpriteRenderer.sprite = _item_texture.SpriteTexture;
    _item_id = item_id;
  }
  #nullable disable

  public void SetItemCount(uint item_count){
    if(_item_count <= 0){
      Debug.LogWarning("Cannot set item count to 0.");
      return;
    }

    _item_count = item_count;
  }
}