using UnityEngine;


namespace SequenceHelper{
  namespace Debugging{
    public class LoggingSequence: MonoBehaviour, ISequenceAsync{
      public enum LogType{
        Normal,
        Warning,
        Error
      }

      [SerializeField]
      private LogType _LogType;

      [SerializeField]
      private string _LogMessage;
      

      public void StartTriggerAsync(){
        switch(_LogType){
          case LogType.Normal:{
            Debug.Log(_LogMessage);
          }break;

          case LogType.Warning:{
            Debug.LogWarning(_LogMessage);
          }break;

          case LogType.Error:{
            Debug.LogError(_LogMessage);
          }break;
        }
      }

      public bool IsTriggering(){
        return false;
      }
    }
  }
}