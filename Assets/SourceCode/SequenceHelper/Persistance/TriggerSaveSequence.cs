using UnityEngine;
using Unity.VisualScripting;
using System.Runtime.InteropServices.WindowsRuntime;


namespace SequenceHelper{
  public class TriggerSaveSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "trigger_save";

    public struct SequenceData{}
    
    private SequenceData _seq_data;

    private GameHandler _game_handler;


    public void Start(){
      _game_handler = FindAnyObjectByType<GameHandler>();
      if(_game_handler == null){
        Debug.LogError("Cannot find GameHandler.");
        throw new MissingReferenceException();
      }
    }


    public void StartTriggerAsync(){
      _game_handler.SaveGame();
    }

    public bool IsTriggering(){
      return false;
    }


    public string GetSequenceID(){
      return SequenceID;
    }

    public void SetSequenceData(object data){
      if(data is not SequenceData){
        Debug.LogError("Data is not SequenceData.");
        return;
      }

      _seq_data = (SequenceData)data;
    }
  }


  [UnitTitle("Trigger Save")]
  [UnitCategory("Sequence/PersistanceHandling")]
  public class TriggerSaveSequenceVS: AddSubSequence{
    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = TriggerSaveSequence.SequenceID,
        SequenceData = new TriggerSaveSequence.SequenceData{}
      };
    }
  }
}