using UnityEngine;


public class PlayerHUDUI: MonoBehaviour{
  [SerializeField]
  private HealthBarUI _HealthBar;

  [SerializeField]
  private MultipleProgressBar _AmmoCounter;

  [SerializeField]
  private InventoryHUDUI _InvHUD;

  [SerializeField]
  private GameObject _VisualContainer;

  
  public HealthBarUI GetHealthBar(){
    return _HealthBar;
  }

  public MultipleProgressBar GetAmmoCounter(){
    return _AmmoCounter;
  }

  public InventoryHUDUI GetInvHUD(){
    return _InvHUD;
  }


  public GameObject GetVisualContainer(){
    return _VisualContainer;
  }
}