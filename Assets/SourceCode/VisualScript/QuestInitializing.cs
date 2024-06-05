using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;



[UnitCategory("Quest/QuestUtility")]
public class CreateQuest: Unit{
  [DoNotSerialize]
  private ControlOutput _quest_output_flow;

  [DoNotSerialize]
  private ValueOutput _quest_result;
  [DoNotSerialize]
  private ValueOutput _quest_raw;

  [DoNotSerialize]
  private ValueInput _quest_title;
  [DoNotSerialize]
  private ValueInput _quest_description;


  private QuestHandlerVS.InitQuestInfo _quest_info;


  protected override void Definition(){
    _quest_result = ValueOutput("QuestResult", (flow) => {
      _quest_info = new QuestHandlerVS.InitQuestInfo{
        QuestData = new QuestHandlerVS.QuestData{
          QuestTitle = flow.GetValue<string>(_quest_title),
          QuestDescription = flow.GetValue<string>(_quest_description)
        },

        SubquestList = new List<QuestHandlerVS.InitQuestInfo>()
      };

      flow.Invoke(_quest_output_flow);

      return _quest_info;
    });

    _quest_output_flow = ControlOutput("QuestFlow");
    _quest_raw = ValueOutput("QuestRaw", (flow) => { return _quest_info; });

    _quest_title = ValueInput("QuestTitle", "TemplateTitle");
    _quest_description = ValueInput("QuestDescription", "");
  }
}


[UnitCategory(".Interface")]
public class AddQuest: Unit{
  [DoNotSerialize]
  private ControlInput _quest_input_flow;
  [DoNotSerialize]
  private ControlOutput _quest_output_flow;

  [DoNotSerialize]
  private ValueInput _quest_input;
  [DoNotSerialize]
  private ValueOutput _quest_output;


  private QuestHandlerVS.InitQuestInfo _init_data;

  protected override void Definition(){
    _quest_input_flow = ControlInput("InputFlow", (flow) => {
      _init_data = flow.GetValue<QuestHandlerVS.InitQuestInfo>(_quest_input);

      QuestHandlerVS.InitQuestInfo _new_init_data;
      AddData(flow, out _new_init_data);
      if(_new_init_data != null)
        _init_data.SubquestList.Add(_new_init_data);

      return _quest_output_flow;
    });

    _quest_output_flow = ControlOutput("NextQuest");

    _quest_input = ValueInput<QuestHandlerVS.InitQuestInfo>("QuestData");
    _quest_output = ValueOutput("QuestData", (flow) => { return _init_data; });
  }

  protected virtual void AddData(Flow flow, out QuestHandlerVS.InitQuestInfo init_data){init_data = null;}
}