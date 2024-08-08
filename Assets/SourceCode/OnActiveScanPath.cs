using Pathfinding;
using UnityEngine;


/// <summary>
/// Trigger class for rescaning <b>Pathfinding.NavGraph</b>'s pathfinding data for each time this class get enabled or disabled. The target to trigger is based on the name of the pathfinding graph.
/// 
/// This class uses external component(s);
/// - Arongranberg's <b>NavGraph</b> Pathfinding object that is the target of the rescanning.
/// </summary>
public class OnActiveScanPath: MonoBehaviour{
  [SerializeField]
  private string GraphName;

  [SerializeField]
  private bool ScanOnEnable;
  [SerializeField]
  private bool ScanOnDisable;


  private void _trigger_scan(){
    NavGraph _nav = _get_nav(GraphName);
    if(_nav == null)
      return;

    AstarPath.active.Scan(_nav);
  }


  // get NavGraph based on the supplied name
  private NavGraph _get_nav(string name){
    NavGraph _result = null;
    for(int i = 0; i < AstarPath.active.data.graphs.Length; i++){
      NavGraph _current_graph = AstarPath.active.data.graphs[i];
      if(_current_graph.name == name){
        _result = _current_graph;
        break;
      }
    }

    return _result;
  }


  public void Start(){
    NavGraph _this_nav = _get_nav(GraphName);
    if(_this_nav == null){
      Debug.LogError(string.Format("NavGraph (name: {0}) cannot be found.", GraphName));
      return;
    }
  }


  /// <summary>
  /// Function to catch Unity's "Object Enabled" event.
  /// </summary>
  public void OnEnable(){
    if(!ScanOnEnable)
      return;

    _trigger_scan();
  }

  /// <summary>
  /// Function to catch Unity's "Object Disabled" event.
  /// </summary>
  public void OnDisable(){
    DEBUGModeUtils.Log("on disable");
    if(!ScanOnDisable)
      return;

    _trigger_scan();
  }
}