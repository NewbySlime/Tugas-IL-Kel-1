using JetBrains.Annotations;
using TMPro;
using UnityEngine;


public class BossHealthBarUI: MonoBehaviour{
  [SerializeField]
  private HealthBarUI _HealthBarUI;
  public HealthBarUI HealthBar{get => _HealthBarUI;}

  [SerializeField]
  private TMP_Text _BossName;


  private CharacterDatabase _character_database;


  public void Start(){
    _character_database = FindAnyObjectByType<CharacterDatabase>();
    if(_character_database == null){
      Debug.LogError("Cannot find database for Characters.");
      throw new MissingReferenceException();
    }
  }

  
  public void BindBossObject(GameObject boss){
    _HealthBarUI.UnbindHealthComponent();
    _BossName.text = "";

    CharacterComponent _character_component = boss.GetComponent<CharacterComponent>();
    if(_character_component == null){
      Debug.LogError(string.Format("Boss Object ({0}) does not have CharacterComponent?", boss.name));
      return;
    }

    HealthComponent _boss_health = boss.GetComponent<HealthComponent>();
    if(_boss_health == null){
      Debug.LogError(string.Format("Boss Object ({0}) does not have HealthComponent?", boss.name));
      return;
    }


    _HealthBarUI.BindHealthComponent(_boss_health);

    TypeDataStorage _character_data = _character_database.GetDataStorage(_character_component.CharacterID);
    if(_character_data == null){
      Debug.LogError(string.Format("Cannot find data for Character with ID: '{0}'.", _character_component.CharacterID));
      return;
    }

    CharacterMetadata.CharacterData _char_metadata = _character_data.GetData<CharacterMetadata.CharacterData>();
    if(_char_metadata == null){
      Debug.LogError(string.Format("CharacterMetadata is not found in Character with ID: '{0}'.", _character_component.CharacterID));
      return;
    }

    _BossName.text = _char_metadata.CharacterName;
  }
}