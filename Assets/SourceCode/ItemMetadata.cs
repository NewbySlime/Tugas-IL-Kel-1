using System;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Metadata PENTING untuk memberi tahu bahwa Objek tersebut adalah sebuat Item pada Game. 
/// </summary>
public class ItemMetadata: MonoBehaviour{
  /// <summary>
  /// Data yang digunakan untuk merepresentasikan kelas ItemMetadata.
  /// </summary>
  [Serializable]
  public struct ItemData{
    public string ItemID;

    public string Name;
    public string Description;
  }

  [SerializeField]
  private ItemData _item_metadata;


  /// <summary>
  /// Fungsi PENTING untuk membantu proses inisialisasi ItemDatabase.
  /// </summary>
  /// <param name="data">Penyimpanan data berdasarkan tipe yang diberikan dari ItemDatabase</param>
  public void ItemDatabase_LoadData(TypeDataStorage data){
    data.AddData(_item_metadata);
  }
  

  /// <summary>
  /// ItemID pada item.
  /// </summary>
  /// <returns>ID item</returns>
  public string GetItemID(){
    return _item_metadata.ItemID;
  }
}