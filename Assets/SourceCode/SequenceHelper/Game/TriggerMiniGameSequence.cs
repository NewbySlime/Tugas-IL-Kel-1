using UnityEngine;
using Unity.VisualScripting;



namespace SequenceHelper{
  public class TriggerMiniGameSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "trigger_mini_game";

    public struct SequenceData{
      public ObjectReference.ObjRefID MiniGameObject;
    }


    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      GameObject _mini_game_obj = ObjectReference.GetReferenceObject(_seq_data.MiniGameObject);
      if(_mini_game_obj == null){
        Debug.LogError(string.Format("MiniGame Object is null. (RefID: {0})", _seq_data.MiniGameObject));
        return;
      }

      MiniGameHandler _mini_game = _mini_game_obj.GetComponent<MiniGameHandler>();
      if(_mini_game == null){
        Debug.LogError(string.Format("MiniGame Object does not have MiniGameHandler. (RefID: {0})", _seq_data.MiniGameObject));
        return;
      }

      _mini_game.TriggerGameStart();
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


  [UnitTitle("Trigger Mini Game")]
  [UnitCategory("Sequence/Game")]
  public class TriggerMiniGameSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _mini_game_input;


    protected override void Definition(){
      base.Definition();

      _mini_game_input = ValueInput<ObjectReference.ObjRefID>("MiniGameObjRef");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = TriggerMiniGameSequence.SequenceID,
        SequenceData = new TriggerMiniGameSequence.SequenceData{
          MiniGameObject = flow.GetValue<ObjectReference.ObjRefID>(_mini_game_input)
        }
      };
    }
  }
}