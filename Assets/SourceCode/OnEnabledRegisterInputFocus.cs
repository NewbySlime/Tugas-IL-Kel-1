using UnityEngine;


public class OnEnabledRegisterInputFocus: MonoBehaviour{
  [SerializeField]
  private InputFocusContext.ContextEnum _TargetContext;

  [SerializeField]
  private GameObject _TargetObject;


  private InputFocusContext _focus_context;

  
  public bool IsInitialized{private set; get;} = false;


  public void Start(){
    _focus_context = FindAnyObjectByType<InputFocusContext>();
    if(_focus_context == null){
      Debug.LogError("Cannot find InputFocusContext.");
      throw new MissingReferenceException();
    }

    IsInitialized = true;
  }


  public void OnEnable(){
    if(!IsInitialized)
      return;

    _focus_context.RegisterInputObject(_TargetObject, _TargetContext);
  }

  public void OnDisable(){
    if(!IsInitialized)
      return;

    _focus_context.RemoveInputObject(_TargetObject, _TargetContext);
  }
}