using TMPro;
using UnityEngine;


public class BossHealthBarUI: MonoBehaviour{
  [SerializeField]
  private HealthBarUI _HealthBarUI;

  [SerializeField]
  private TMP_Text _BossName;

  
  public void BindBossObject(GameObject boss){
    HealthComponent _boss_health = boss.GetComponent<HealthComponent>();
    if(_boss_health == null){
      Debug.LogError(string.Format("Boss Object ({0}) does not have HealthComponent?", boss.name));
      return;
    }

    _HealthBarUI.BindHealthComponent(_boss_health);
  }
}