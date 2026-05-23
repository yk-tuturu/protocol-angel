using UnityEngine;

[CreateAssetMenu(fileName = "FadeInSO", menuName = "Scriptable Objects/FadeInSO")]
public class FadeInSO : StepSO
{
    public string characterName;
    public string spriteName;
    public float duration;

    public override DialogueStepType GetStepType()
    {
        return DialogueStepType.FadeIn;
    }
}
