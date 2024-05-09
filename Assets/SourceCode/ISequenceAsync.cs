using System;
using System.Collections;


public interface ISequenceAsync{
  public void StartTriggerAsync();
  public bool IsTriggering();
}