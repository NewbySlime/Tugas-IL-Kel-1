using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  public class SetMainUIMode: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "set_main_ui_mode";

    public struct SequenceData{
      public GameUIHandler.MainHUDUIEnum UIMode;
      public bool ShowFlag;
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
      _game_ui_handler.SetMainHUDUIMode(_seq_data.UIMode, _seq_data.ShowFlag);
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


  [UnitTitle("Main UI Mode")]
  [UnitCategory("Sequence/UI")]
  public class SetMainUIModeVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _mode_input;
    [DoNotSerialize]
    private ValueInput _show_input;


    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new SequenceHandlerVS.SequenceInitializeData.DataPart{
        SequenceID = SetMainUIMode.SequenceID,
        SequenceData = new SetMainUIMode.SequenceData{
          UIMode = flow.GetValue<GameUIHandler.MainHUDUIEnum>(_mode_input),
          ShowFlag = flow.GetValue<bool>(_show_input)
        }
      };
    }

    protected override void Definition(){
      base.Definition();

      _mode_input = ValueInput("Mode", GameUIHandler.MainHUDUIEnum.PlayerHUD);
      _show_input = ValueInput("Show", true);
    }
  }
}