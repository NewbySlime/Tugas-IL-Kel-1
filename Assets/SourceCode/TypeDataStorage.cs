using System;
using System.Collections.Generic;
using Unity.VisualScripting;


/// <summary>
/// Kelas untuk menyimpan tipe-tipe yang berbeda tanpa harus memberi tahu tipenya secara eksplisit.
/// </summary>
public class TypeDataStorage{
  private Dictionary<Type, object> _data = new Dictionary<Type, object>();
  

  /// <summary>
  /// Untuk memeberikan data berdasarakan tipe "T".
  /// NOTE: setiap tipe hanya bisa satu data di kelas ini, jika lebih, akan di overwrite.
  /// </summary>
  /// <typeparam name="T">Tipe data yang mau dimasukkan</typeparam>
  /// <param name="data">Data yang mau dimasukkan</param>
  public void AddData<T>(T data){
    _data.Add(typeof(T), data);
  }

  /// <summary>
  /// Untuk menghapus data berdasarkan tipe "T".
  /// </summary>
  /// <typeparam name="T">Tipe data yang mau dihapuskan</typeparam>
  public void RemoveData<T>(){
    _data.Remove(typeof(T));
  }

  #nullable enable
  /// <summary>
  /// Untuk mengambil data berdasarkan tipe "T".
  /// </summary>
  /// <typeparam name="T">TIpe dat ayang mau diambil</typeparam>
  /// <returns>Data tertentu berdasarkan "T", namun akan mengeluarkan "default" atau null.</returns>
  public T? GetData<T>(){
    if(_data.ContainsKey(typeof(T)))
      return (T)_data[typeof(T)];
    else
      return default;
  }
}