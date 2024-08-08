using System;
using UnityEngine;



[RequireComponent(typeof(ConsoleUIController))]
/// <summary>
/// OUTDATED
/// Extension component for <see cref="ConsoleUIController"/> as to handle some commands for player handling.
/// 
/// List of command(s);
/// - <b>[player.addItem [item_id] [item_count]]</b>
///   Command for adding items to player's inventory system.
///   Arguments:
///   - <b>[item_id]</b> What kind of item to add.
///   - <b>[item_count]</b> How many items to add.
/// 
/// This class uses following component(s);
/// - <see cref="ConsoleUIController"/> for handling command inputs from user.
/// </summary>
public class PlayerInventoryCommand: MonoBehaviour{
  /// <summary>
  /// Function to catch command message from <see cref="ConsoleUIController"/>.
  /// </summary>
  /// <param name="args">User's command input</param>
  public void ConsoleHandler_CommandAccept(string[] args){
    if(args.Length <= 0)
      return;

    switch(args[0]){
      // Add an item to player's inventory system.
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