using System.Collections;
using System.Collections.Specialized;
using System.Data.Common;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system to create a Character object based on the Character's ID.
  /// </summary>
  public class CreateCharacterSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "create_character_object";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// The Character ID to be used for instancing.
      /// </summary>
      public string CharacterID;

      /// <summary>
      /// Friendly context/state when the character is created.
      /// </summary>
      public ObjectFriendlyHandler.FriendlyType FriendlyContext;

      /// <summary>
      /// Reference to the object.
      /// The newly created object should use this as a reference as the reference is created by the node sequence.
      /// </summary>
      public ObjectReference.ObjRefID RefID;
    }


    private SequenceData _seq_data;

    private CharacterDatabase _character_database;


    public void Start(){
      _character_database = FindAnyObjectByType<CharacterDatabase>();
      if(_character_database == null){
        Debug.LogError("Cannot find database for Characters.");
        return;
      }
    }


    public void StartTriggerAsync(){
      GameObject _inst_obj = _character_database.CreateNewCharacter(_seq_data.CharacterID, _seq_data.FriendlyContext);
      ObjectReference.SetReferenceObject(_seq_data.RefID, _inst_obj);
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


  [UnitTitle("Create Character")]
  [UnitCategory("Sequence/Character")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="CreateCharacterSequence"/>.
  /// Also, the supplied Object Reference are created by this node object for use by another sequence node.
  /// </summary>
  public class CreateCharacterSequenceVS: AddSubSequence{
    [DoNotNormalize]
    private ValueInput _character_id_input;
    [DoNotSerialize]
    private ValueInput _friendly_context_input;

    [DoNotSerialize]
    private ValueOutput _object_output;

    private ObjectReference.ObjRefID _ref_id;


    protected override void Definition(){
      base.Definition();

      _character_id_input = ValueInput("CharacterID", "");
      _friendly_context_input = ValueInput("FriendlyContext", ObjectFriendlyHandler.FriendlyType.Neutral);

      _object_output = ValueOutput("ObjectRef", (flow) => _ref_id);
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      _ref_id = ObjectReference.CreateRandomReference();

      init_data = new(){
        SequenceID = CreateCharacterSequence.SequenceID,
        SequenceData = new CreateCharacterSequence.SequenceData{
          CharacterID = flow.GetValue<string>(_character_id_input),
          FriendlyContext = flow.GetValue<ObjectFriendlyHandler.FriendlyType>(_friendly_context_input),

          RefID = _ref_id
        }
      };
    }
  }
}