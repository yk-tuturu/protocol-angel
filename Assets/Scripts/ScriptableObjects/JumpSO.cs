using UnityEngine;

[CreateAssetMenu(fileName = "JumpSO", menuName = "Scriptable Objects/JumpSO")]
public class JumpSO : StepSO
{
    public string characterName;
    public string spriteName;
    public float duration;
    public float jumpPower;
    public int numJumps;

    public override DialogueStepType GetStepType()
    {
        return DialogueStepType.Jump;
    }
}
