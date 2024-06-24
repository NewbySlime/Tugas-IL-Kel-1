using Pathfinding;
using UnityEngine;


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


  public void OnEnable(){
    if(!ScanOnEnable)
      return;

    _trigger_scan();
  }

  public void OnDisable(){
    DEBUGModeUtils.Log("on disable");
    if(!ScanOnDisable)
      return;

    _trigger_scan();
  }
}