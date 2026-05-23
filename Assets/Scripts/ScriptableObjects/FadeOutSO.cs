using UnityEngine;

[CreateAssetMenu(fileName = "FadeOutSO", menuName = "Scriptable Objects/FadeOutSO")]
public class FadeOutSO : StepSO
{
    public string characterName;
    public string spriteName;
    public float duration;

    public override DialogueStepType GetStepType()
    {
        return DialogueStepType.FadeOut;
    }
}
