using UnityEngine;

[CreateAssetMenu(fileName = "ChangeSpriteSO", menuName = "Scriptable Objects/ChangeSpriteSO")]
public class ChangeSpriteSO : StepSO
{
    public string characterName;
    public string spriteName;
    public override DialogueStepType GetStepType()
    {
        return DialogueStepType.ChangeSprite;
    }
}
