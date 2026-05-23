using System;

public static class GameEventManager
{
    public static event Action NextStep;

    public static event Action<Step> EndStep;

    public static void Raise_NextStep()
    {
        NextStep?.Invoke();
        
    }

    public static void Raise_EndStep(Step step)
    {
        EndStep?.Invoke(step);
    }
       
}
