using System;

// fucking hacky workaround to allow for one layer of step nesting for simultaneous steps. nkys!
[System.Serializable]
public class Step : Substep
{
    // simultaneous steps
    public Substep[] steps;
}

[System.Serializable]
public class Substep
{
    public string type;
    public DialogueStepType stepType;

    // Dialogue step fields
    public string speaker;
    public string sentence;
    public float textDelay = 0.02f;

    // if present, this step will also change sprites
    public string characterName; 
    public string spriteName;

    // fade in/out 
    public float duration;

    // jump 
    public float jumpPower;
    public int numJumps;

    // pre and post-delay -- common to all steps
    public float preDelay = 0f;
    public float postDelay = 0f;
}

[System.Serializable]
public class Scene
{
    public Step[] steps;
}