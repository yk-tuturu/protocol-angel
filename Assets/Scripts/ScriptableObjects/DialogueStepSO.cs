using UnityEngine;




[CreateAssetMenu(fileName = "DialogueStepSO", menuName = "Scriptable Objects/DialogueStepSO")]
public class DialogueStepSO : StepSO
{
    public string speaker;
    public string sentence;

    public DialogueStepSO[] simultaneousSteps;

    public override DialogueStepType GetStepType()
    {
        return DialogueStepType.Dialogue;
    }
}
