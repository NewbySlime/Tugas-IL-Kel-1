using System;
using UnityEngine;


/// <summary>
/// Komponen untuk menyimpan data Texture pada Item.
/// Komponen yang diperlukan:
///   - ItemMetadata
/// </summary>
public class ItemTextureData: MonoBehaviour{
  [Serializable]
  public class ItemData{
    public Sprite SpriteTexture;
  }

  [SerializeField]
  private ItemData _ItemData;


  /// <summary>
  /// Fungsi PENTING untuk membancu proses inisialisasi ItemDatabase.
  /// </summary>
  /// <param name="item_data">Penyimpanan data berdasarkan tipe yang diberikan dari ItemDatabse</param>
  public void ItemDatabase_LoadData(TypeDataStorage item_data){
    item_data.AddData(_ItemData);
  }
}