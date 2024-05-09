using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Komponen untuk mengecek apakah Input "lagi dipakai Object lain?" sehingga Object lain bisa ngecek dan menonaktifkan/tidak melakukan pemrosesan Input dari Player.
/// </summary>
public class InputContext: MonoBehaviour{
  public delegate void OnVariableChanged();
  public event OnVariableChanged OnUIInputContext;


  // variable untuk menentukan apakah input sekarang dipakai UI atau tidak
  private HashSet<GameObject> _ui_input_context_key_list = new HashSet<GameObject>();


  /// <summary>
  /// Fungsi bind agar Input "diklaim".
  /// </summary>
  /// <param name="bound">Object yang mau di-"bind"</param>
  public void BindUIInputContext(GameObject bound){
    _ui_input_context_key_list.Add(bound);
    
    if(_ui_input_context_key_list.Count == 1)
      OnUIInputContext?.Invoke();
  }

  /// <summary>
  /// Fungsi menghilangkan bind agar Input bisa dipakai lagi oleh yang lainnya.
  /// Object yang diberikan harus sesuai dengan yang di-"bind".
  /// </summary>
  /// <param name="bound">Object yang mau dihapus state bound nya</param>
  public void RemoveUIInputContext(GameObject bound){
    _ui_input_context_key_list.Remove(bound);

    if(_ui_input_context_key_list.Count == 0)
      OnUIInputContext?.Invoke();
  }

  /// <summary>
  /// Fungsi untuk mengecek apakah GameObject bisa menggunakan input tersebut.
  /// </summary>
  /// <param name="obj">GameObject yang mau dites</param>
  /// <returns>Apakah Input bisa dipakai atau tidak</returns>
  public bool GetUIInputContext(GameObject obj){
    return _ui_input_context_key_list.Contains(obj);
  }
}