using UnityEngine;
using Unity.VisualScripting;



namespace SequenceHelper{
  public class ChangeToMainMenuSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
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

    public void SetSequenceData(object data){

    }
  }


  [UnitTitle("Change To Main Menu")]
  [UnitCategory("Sequence/Scene")]
  public class ChangeToMainMenuSequenceVS: AddSubSequence{
    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = ChangeToMainMenuSequence.SequenceID,
        SequenceData = null
      };
    }
  }
}