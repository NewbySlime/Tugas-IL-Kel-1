using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// Komponen untuk mengontrol tombol dan memproses jika komponen ini di "bind" dengan Item pada Inventory.
/// </summary>
public class ItemButton: MonoBehaviour, IPointerClickHandler{
  /// <summary>
  /// Event jika tombol pada GameObject ini dipencet.
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
  /// Fungsi untuk menyiapkan Item ID pada Komponen ini. 
  /// </summary>
  /// <param name="item_id">ID Item yang mau di-set</param>
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
    }
    else{
      _ItemSprite.sprite = _tex_data.SpriteTexture;
    }
  }
  #nullable disable


  /// <summary>
  /// Fungsi untuk memberikan info tentang berapa banyak Item yang terkait di komponen ini.
  /// </summary>
  /// <param name="count">Berapa banyak Item yang ada</param>
  public void SetItemCount(uint count){
    if(count == 1)
      _ItemCount.text = "";
    else
      _ItemCount.text = count.ToString();
  }



  /// <summary>
  /// Ke-trigger jika GameObject ini diklik
  /// </summary>
  /// <param name="_event_data">Event data dari Unity</param>
  public void OnPointerClick(PointerEventData _event_data){
    OnButtonPressedEvent?.Invoke(_item_id);
  }
}