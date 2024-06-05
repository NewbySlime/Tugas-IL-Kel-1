using System.Collections;
using System.Collections.Specialized;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  public class CreateCharacterSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "create_character_object";

    public struct SequenceData{
      public string CharacterID;
      public ObjectReference.ObjRefID RefID;
    }


    [SerializeField]
    private GameObject _CharacterPrefab;


    private SequenceData _seq_data;

    private bool _seq_triggering = false;


    private IEnumerator _start_trigger(){
      _seq_triggering = true;
      GameObject _inst_obj = Instantiate(_CharacterPrefab);
      ObjectReference.SetReferenceObject(_seq_data.RefID, _inst_obj);

      yield return new WaitForNextFrameUnit();

      NPCHandler _handler = _inst_obj.GetComponent<NPCHandler>();
      _handler.SetCharacter(_seq_data.CharacterID);

      _seq_triggering = false;
    }


    public void Start(){
      // test _CharacterPrefab
      GameObject _test_obj = Instantiate(_CharacterPrefab);
      if(_test_obj.GetComponent<NPCHandler>() == null){
        Debug.LogError("CharacterPrefab does not have NPCHandler.");
        throw new MissingComponentException();
      }

      Destroy(_test_obj);
    }


    public void StartTriggerAsync(){
      StartCoroutine(_start_trigger());
    }

    public bool IsTriggering(){
      return _seq_triggering;
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


  [UnitTitle("Create Character")]
  [UnitCategory("Sequence/Character")]
  public class CreateCharacterSequenceVS: AddSubSequence{
    [DoNotNormalize]
    private ValueInput _character_id_input;

    [DoNotSerialize]
    private ValueOutput _object_output;

    private ObjectReference.ObjRefID _ref_id;


    protected override void Definition(){
      base.Definition();

      _character_id_input = ValueInput("CharacterID", "");
      _object_output = ValueOutput("ObjectRef", (flow) => _ref_id);
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      _ref_id = ObjectReference.CreateRandomReference();

      init_data = new(){
        SequenceID = CreateCharacterSequence.SequenceID,
        SequenceData = new CreateCharacterSequence.SequenceData{
          CharacterID = flow.GetValue<string>(_character_id_input),
          RefID = _ref_id
        }
      };
    }
  }
}