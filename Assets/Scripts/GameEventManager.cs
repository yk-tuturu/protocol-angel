using System;

public static class GameEventManager
{
    public static event Action NextStep;

    public static void Raise_NextStep()
    {
        NextStep?.Invoke();
        
    }
       
}
