using UnityEngine;


/// <summary>
/// Trigger class for automatically registering input focus to autoload <see cref="InputFocusContext"/> whenever this class gets enabled or disabled.
/// </summary>
public class OnEnabledRegisterInputFocus: MonoBehaviour{
  [SerializeField]
  private InputFocusContext.ContextEnum _TargetContext;

  [SerializeField]
  private GameObject _TargetObject;


  private InputFocusContext _focus_context;

  
  /// <summary>
  /// Flag for checking if this object is ready or not.
  /// </summary>
  public bool IsInitialized{private set; get;} = false;


  public void Start(){
    _focus_context = FindAnyObjectByType<InputFocusContext>();
    if(_focus_context == null){
      Debug.LogError("Cannot find InputFocusContext.");
      throw new MissingReferenceException();
    }

    IsInitialized = true;
  }


  /// <summary>
  /// Function to catch Unity's "Object Enabled" event.
  /// </summary>
  public void OnEnable(){
    if(!IsInitialized)
      return;

    _focus_context.RegisterInputObject(_TargetObject, _TargetContext);
  }

  /// <summary>
  /// Function to catch Unity's "Object Disabled" event.
  /// </summary>
  public void OnDisable(){
    if(!IsInitialized)
      return;

    _focus_context.RemoveInputObject(_TargetObject, _TargetContext);
  }
}