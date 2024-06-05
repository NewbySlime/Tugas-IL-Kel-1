using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.Video;



namespace SequenceHelper{
  public class StartVideoUISequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "start_video_ui";

    public struct SequenceData{
      public VideoClip Video;
    }


    private GameUIHandler _ui_handler;
    private VideoPlayer _video_player;

    private SequenceData _seq_data;


    public void Start(){
      _ui_handler = FindAnyObjectByType<GameUIHandler>();
      if(_ui_handler == null){
        Debug.LogError("Cannot find GameUIHandler.");
        throw new MissingReferenceException();
      }

      _video_player = _ui_handler.GetVideoPlayerUI();
    }


    public void StartTriggerAsync(){
      if(IsTriggering())
        return;

      _video_player.clip = _seq_data.Video;
      _video_player.Play();
    }

    public bool IsTriggering(){
      return _video_player.isPlaying || _video_player.isPaused;
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


  [UnitTitle("Start Video")]
  [UnitCategory("Sequence/UI")]
  public class StartVideoUISequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _video_clip_input;


    protected override void Definition(){
      base.Definition();

      _video_clip_input = ValueInput<VideoClip>("Video Clip");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = StartVideoUISequence.SequenceID,
        SequenceData = new StartVideoUISequence.SequenceData(){
          Video = flow.GetValue<VideoClip>(_video_clip_input)
        }
      };
    }
  }
}