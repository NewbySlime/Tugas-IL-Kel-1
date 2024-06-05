using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  public class SetPlayerUIMode: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "set_player_ui_mode";

    public struct SequenceData{
      public GameUIHandler.PlayerUIMode UIMode;
    }

    private SequenceData _seq_data;

    private GameUIHandler _game_ui_handler;


    public void Start(){
      _game_ui_handler = FindAnyObjectByType<GameUIHandler>();
      if(_game_ui_handler == null){
        Debug.LogError("Cannot find GameUIHandler.");
        throw new MissingReferenceException();
      }
    }


    public void StartTriggerAsync(){
      _game_ui_handler.SetPlayerUIMode(_seq_data.UIMode);
    }

    public bool IsTriggering(){
      return false;
    }


    public string GetSequenceID(){
      return SequenceID;
    }

    public void SetSequenceData(object data){
      if(data is not SequenceData){
        Debug.LogError("Data is not SequenceData");
        return;
      }

      _seq_data = (SequenceData)data;
    }
  }


  [UnitTitle("Player UI Mode")]
  [UnitCategory("Sequence/UI")]
  public class SetPlayerUIModeVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _mode_input;


    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new SequenceHandlerVS.SequenceInitializeData.DataPart{
        SequenceID = SetPlayerUIMode.SequenceID,
        SequenceData = new SetPlayerUIMode.SequenceData{
          UIMode = flow.GetValue<GameUIHandler.PlayerUIMode>(_mode_input)
        }
      };
    }

    protected override void Definition(){
      base.Definition();

      _mode_input = ValueInput("Mode", GameUIHandler.PlayerUIMode.MainHUD);
    }
  }
}