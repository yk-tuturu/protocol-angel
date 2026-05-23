using UnityEngine;

public enum DialogueStepType
{
    Dialogue,
    ChangeSprite,
    Simultaneous,
    FadeIn,
    FadeOut
}

[CreateAssetMenu(fileName = "StepSO", menuName = "Scriptable Objects/StepSO")]
public abstract class StepSO : ScriptableObject
{
    public abstract DialogueStepType GetStepType();
}
