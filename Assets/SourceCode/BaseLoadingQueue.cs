using UnityEngine;


public class BaseLoadingQueue: ILoadingQueue{
  public bool LoadFlag = false;

  public bool IsLoaded(){
    return LoadFlag;
  }
}