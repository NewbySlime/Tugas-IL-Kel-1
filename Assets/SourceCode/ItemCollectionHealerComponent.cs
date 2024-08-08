using UnityEngine;


[RequireComponent(typeof(InventoryData))]
[RequireComponent(typeof(HealthComponent))]
/// <summary>
/// Component for automatically heal an object with <see cref="HealthComponent"/> whenever an item entered has a data about <see cref="ItemHealData"/>.
/// 
/// This class uses following component(s);
/// - <see cref="InventoryData"/> for handling object's inventory.
/// - <see cref="HealthComponent"/> the health system.
/// 
/// This class uses autoload(s);
/// - <see cref="ItemDatabase"/> for getting data about an item.
/// </summary>
public class ItemCollectionHealerComponent: MonoBehaviour{
  [SerializeField]
  private bool _RemoveItemOnGet = true;

  private ItemDatabase _item_database;
  
  private InventoryData _inv_data;
  private HealthComponent _health_component;


  private void _inventory_on_added(string item_id, uint count){
    TypeDataStorage _item_data = _item_database.GetItemData(item_id);
    if(_item_data == null){
      Debug.LogWarning(string.Format("Adding a non-existed Item (ID: {0}) to Inventory?", item_id));
      return; 
    }

    ItemHealData.ItemData _heal_data = _item_data.GetData<ItemHealData.ItemData>();
    if(_heal_data == null)
      return;

    _health_component.DoHeal(_heal_data.HealPoint * count);
    if(_RemoveItemOnGet)
      _inv_data.RemoveItem(item_id, uint.MaxValue);
  }


  public void Start(){
    _item_database = FindAnyObjectByType<ItemDatabase>();
    if(_item_database == null){
      Debug.LogError("Cannot find database for Items.");
      throw new MissingReferenceException();
    }

    _inv_data = GetComponent<InventoryData>();
    _inv_data.OnItemAddedEvent += _inventory_on_added;

    _health_component = GetComponent<HealthComponent>();
  }
}