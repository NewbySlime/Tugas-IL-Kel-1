using UnityEngine;
using Unity.VisualScripting;



namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system to change the scene to main menu.
  /// </summary>
  public class ChangeToMainMenuSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "change_to_main_menu";


    private GameHandler _game_handler;

    
    public void Start(){
      _game_handler = FindAnyObjectByType<GameHandler>();
      if(_game_handler == null){
        Debug.LogError("Cannot find GameHandler.");
        throw new MissingReferenceException();
      }
    }


    public void StartTriggerAsync(){
      _game_handler.ChangeSceneToMainMenu();
    }

    public bool IsTriggering(){
      return false;
    }


    public string GetSequenceID(){
      return SequenceID;
    }

    public void SetSequenceData(object data){}
  }


  [UnitTitle("Change To Main Menu")]
  [UnitCategory("Sequence/Scene")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="ChangeToMainMenuSequence"/>.
  /// </summary>
  public class ChangeToMainMenuSequenceVS: AddSubSequence{
    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = ChangeToMainMenuSequence.SequenceID,
        SequenceData = null
      };
    }
  }
}