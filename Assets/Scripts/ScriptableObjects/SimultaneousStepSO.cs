using UnityEngine;

[CreateAssetMenu(fileName = "SimultaneousStepSO", menuName = "Scriptable Objects/SimultaneousStepSO")]
public class SimultaneousStepSO : StepSO
{
    public StepSO[] steps;

    public override DialogueStepType GetStepType()
    {
        return DialogueStepType.Simultaneous;
    }
    
}
