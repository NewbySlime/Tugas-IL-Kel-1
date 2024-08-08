using UnityEngine;


/// <summary>
/// Base class for storing flag that can be used by <see cref="ILoadingQueue"/>. This class can be used as a dummy loading queue for a class that does not have <see cref="ILoadingQueue"/> component.
/// </summary>
public class BaseLoadingQueue: ILoadingQueue{
  /// <summary>
  /// Flag to tell if the related class has been loaded.
  /// </summary>
  public bool LoadFlag = false;

  public bool IsLoaded(){
    return LoadFlag;
  }
}