using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  public class RegisterInputFocusSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "register_input_focus";

    public struct InputFocusData{
      public ObjectReference.ObjRefID RefID;
      public InputFocusContext.ContextEnum InputContext;
    }

    public struct SequenceData{
      public InputFocusData FocusData;
    }


    private InputFocusContext _input_handler;

    private SequenceData _seq_data;


    public void Start(){
      _input_handler = FindAnyObjectByType<InputFocusContext>();
      if(_input_handler == null){
        Debug.LogError("Cannot find InputFocusContext.");
        throw new MissingReferenceException();
      }
    }


    public void StartTriggerAsync(){
      GameObject _ref_obj = ObjectReference.GetReferenceObject(_seq_data.FocusData.RefID);
      if(_ref_obj == null){
        Debug.LogError(string.Format("Referenced Object is null. (RefID: {0})", _seq_data.FocusData.RefID));
        return;
      }

      Debug.Log(string.Format("registering input {0}", _ref_obj.name));

      _input_handler.RegisterInputObject(_ref_obj, _seq_data.FocusData.InputContext);
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


  [UnitTitle("Register Input Focus")]
  [UnitCategory("Sequence/Input")]
  public class RegisterInputFocusSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _input_focus_input;


    protected override void Definition(){
      base.Definition();

      _input_focus_input = ValueInput<RegisterInputFocusSequence.InputFocusData>("FocusContext");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = RegisterInputFocusSequence.SequenceID,
        SequenceData = new RegisterInputFocusSequence.SequenceData{
          FocusData = flow.GetValue<RegisterInputFocusSequence.InputFocusData>(_input_focus_input)
        }
      };
    }
  }
}