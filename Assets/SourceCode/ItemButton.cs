using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// OUTDATED This UI class handles button interaction with using certain item to bind and be associated with.
/// 
/// This class uses external component(s);
/// - Target <b>Unity's MaskableGraphics</b> for displaying the Image of the associated item.
/// - <b>Unity's TMP Text UI</b> for displaying item count for the associated item.
/// </summary>
public class ItemButton: MonoBehaviour, IPointerClickHandler{
  /// <summary>
  /// Event for when this button has been pressed. This event will tells the listener the item ID associated with the button.
  /// </summary>
  public event OnButtonPressed OnButtonPressedEvent;
  public delegate void OnButtonPressed(string item_id);

  [SerializeField]
  private UnityEngine.UI.Image _ItemSprite;

  [SerializeField]
  private TextMeshProUGUI _ItemCount;

  private string _item_id;
  private uint _item_count;

  
  #nullable enable
  /// <summary>
  /// Bind an item to this button by ID.
  /// </summary>
  /// <param name="item_id">The target item ID</param>
  public void SetItem(string item_id){
    ItemDatabase _item_database = FindAnyObjectByType<ItemDatabase>();
    TypeDataStorage? _item_data = _item_database.GetItemData(item_id);
    if(_item_data == null){
      Debug.LogError(string.Format("Item (ID: {0}) is invalid.", item_id));
      return;
    }

    _item_id = item_id;

    ItemTextureData.ItemData? _tex_data = _item_data.GetData<ItemTextureData.ItemData>();
    if(_tex_data == null){
      Debug.LogWarning(string.Format("Item (ID: {0}) doesn't have texture image.", item_id));
      return;
    }

    _ItemSprite.sprite = _tex_data.SpriteTexture;
  }
  #nullable disable


  /// <summary>
  /// Set the item count to display.
  /// </summary>
  /// <param name="count">The item count</param>
  public void SetItemCount(uint count){
    if(count == 1)
      _ItemCount.text = "";
    else
      _ItemCount.text = count.ToString();
  }


  /// <summary>
  /// Function to catch "clicked" event from <b>IPointerClickHandler</b>.
  /// </summary>
  /// <param name="_event_data">The event data</param>
  public void OnPointerClick(PointerEventData _event_data){
    OnButtonPressedEvent?.Invoke(_item_id);
  }
}