using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.Video;
using System.Collections;



namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system to start a video with view-blocking UI. 
  /// </summary>
  public class StartVideoUISequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "start_video_ui";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// Video source to be played.
      /// </summary>
      public VideoClip Video;
    }


    private GameUIHandler _ui_handler;
    private VideoPlayer _video_player;

    private SequenceData _seq_data;

    private bool _is_triggering = false;


    private IEnumerator _start_trigger(){
      _is_triggering = true;
      _video_player.clip = _seq_data.Video;
      _video_player.Play();

      yield return new WaitUntil(() => _video_player.frame > 0);
      yield return new WaitUntil(() => (ulong)_video_player.frame >= (_video_player.frameCount-1));

      _is_triggering = false;
    }


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

      StartCoroutine(_start_trigger());
    }

    public bool IsTriggering(){
      return _is_triggering;
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
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="StartVideoUISequence"/>.
  /// </summary>
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