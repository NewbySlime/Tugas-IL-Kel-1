using System;
using UnityEngine;



/// <summary>
/// Komponen untuk menambah fungsi Console. Fungsi yang diberikan adalah memodifikasi Inventory Player.
/// Komponen yang diperlukan:
///   - ConsoleUIController
/// </summary>
public class PlayerInventoryCommand: MonoBehaviour{
  
  /// <summary>
  /// Fungsi untuk menerima "message" dari ConsoleHandler (ConsoleUIController).
  /// </summary>
  /// <param name="args"></param>
  public void ConsoleHandler_CommandAccept(string[] args){
    if(args.Length <= 0)
      return;

    switch(args[0]){
      // Command untuk menambahkan Item ke Inventory Player
      case "player.addItem":{
        if(args.Length <= 1){

          return;
        }

        int _item_count = 1;
        if(args.Length >= 3){
          if(!Int32.TryParse(args[2], out _item_count))
            _item_count = 1;
        }

        PlayerController _player = FindAnyObjectByType<PlayerController>();
        if(_player == null){
          
          return;
        }

        InventoryData _player_inv = _player.gameObject.GetComponent<InventoryData>();
        if(_player_inv == null){

          return;
        }

        _player_inv.AddItem(args[1], (uint)_item_count);
      }break;
    }
  }
}