using UnityEngine;
using System.Collections.Generic;
using System;

public class StepManager : MonoBehaviour
{
    public static StepManager Instance { get; private set; }

    public Scene scene;

    public Queue<Step> stepQueue = new Queue<Step>();

    public Step currentStep;
    public DialogueStepType currentStepType;

    public int simultaneousStepsExecuted = 0;
    public int totalSimultaneousSteps = 0;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadSceneFromJson("testScene.json");

        ExecuteNextStep();
    }

    private void ExecuteNextStep()
    {
        if (stepQueue.Count > 0)
        {
            currentStep = stepQueue.Dequeue();
            currentStepType = currentStep.stepType;
            Execute(currentStep);
        }
    }

    void Execute(Step step)
    {
        switch (step.stepType)
        {
            case DialogueStepType.Dialogue:
                HandleDialogueStep(step);
                break;
            case DialogueStepType.ChangeSprite:
                HandleChangeSpriteStep(step);
                break;
            case DialogueStepType.Simultaneous:
                HandleSimultaneousStep(step);
                break;
            case DialogueStepType.FadeIn:
                HandleFadeInStep(step);
                break;
            case DialogueStepType.FadeOut:
                HandleFadeOutStep(step);
                break;
            case DialogueStepType.Jump:
                HandleJumpStep(step);
                break;
            default:
                Debug.LogWarning("Unknown step type!");
                break;
        }
    }

    void OnStepComplete()
    {
        if (currentStepType == DialogueStepType.Simultaneous)
        {
            simultaneousStepsExecuted++;

            if (simultaneousStepsExecuted >= totalSimultaneousSteps)
            {
                ExecuteNextStep();
            }
        } 
        
        else
        {
            ExecuteNextStep();
        }
    }

    private void HandleDialogueStep(Step step)
    {
        DialogueManager.Instance.DisplayNextSentence(
            step.speaker, step.sentence, step.textDelay,
            () => OnStepComplete());

        if (!string.IsNullOrEmpty(step.characterName) && !string.IsNullOrEmpty(step.spriteName))
        {
            SpriteManager.Instance.ChangeSprite(step.characterName, step.spriteName);
        }
    }

    private void HandleChangeSpriteStep(Step step)
    {
        SpriteManager.Instance.ChangeSprite(step.characterName, step.spriteName, true, () => OnStepComplete());
    }

    private void HandleSimultaneousStep(Step step)
    {   
        Debug.Log(step.steps);
        simultaneousStepsExecuted = 0;
        totalSimultaneousSteps = step.steps.Length;
        foreach (Step subStep in step.steps)
        {
            Execute(subStep);
        }
    }

    private void HandleFadeInStep(Step fadeInStep)
    {
        if (!string.IsNullOrEmpty(fadeInStep.characterName) && !string.IsNullOrEmpty(fadeInStep.spriteName))
        {
            SpriteManager.Instance.ChangeSprite(fadeInStep.characterName, fadeInStep.spriteName, visible: false);
        }

        SpriteManager.Instance.FadeInSprite(
            fadeInStep.characterName, fadeInStep.spriteName, fadeInStep.duration,
            () => OnStepComplete());
    }

    private void HandleFadeOutStep(Step fadeOutStep)
    {
        if (!string.IsNullOrEmpty(fadeOutStep.characterName) && !string.IsNullOrEmpty(fadeOutStep.spriteName))
        {
            SpriteManager.Instance.ChangeSprite(fadeOutStep.characterName, fadeOutStep.spriteName, visible: true);
        }

        SpriteManager.Instance.FadeOutSprite(
            fadeOutStep.characterName, fadeOutStep.spriteName, fadeOutStep.duration,
            () => OnStepComplete());
    }

    private void HandleJumpStep(Step jumpStep)
    {
        if (!string.IsNullOrEmpty(jumpStep.characterName) && !string.IsNullOrEmpty(jumpStep.spriteName))
        {
            SpriteManager.Instance.ChangeSprite(jumpStep.characterName, jumpStep.spriteName);
        }

        SpriteManager.Instance.JumpSprite(
            jumpStep.characterName, jumpStep.spriteName, jumpStep.duration, jumpStep.jumpPower, jumpStep.numJumps, 
            () => OnStepComplete());
    }

    // ==================================
    // For parsing scenes from JSON
    // ==================================
    private DialogueStepType ParseStepType(string type)
    {
        switch (type.ToLower())
        {
            case "dialogue":
                return DialogueStepType.Dialogue;
            case "changesprite":
                return DialogueStepType.ChangeSprite;
            case "simultaneous":
                return DialogueStepType.Simultaneous;
            case "fadein":
                return DialogueStepType.FadeIn;
            case "fadeout":
                return DialogueStepType.FadeOut;
            case "jump":
                return DialogueStepType.Jump;
            default:
                Debug.LogWarning($"Unknown step type: {type}");
                return DialogueStepType.Dialogue; // default to dialogue
        }
    }

    // this is fucking disgusting and i hate it and my cs2030s profs will probably die looking at this shit
    private Step createStepFromSubstep(Substep subStep)
    {
        Step step = new Step
        {
            type = subStep.type,
            stepType = subStep.stepType,
            speaker = subStep.speaker,
            sentence = subStep.sentence,
            textDelay = subStep.textDelay,
            characterName = subStep.characterName,
            spriteName = subStep.spriteName,
            duration = subStep.duration,
            jumpPower = subStep.jumpPower,
            numJumps = subStep.numJumps
        };

        return step;
    }

    private void LoadSceneFromJson(string path)
    {
        string filePath = "scenes/" + path.Replace(".json", "");

        TextAsset targetFile = Resources.Load<TextAsset>(filePath);
        Debug.Log(targetFile.text);

        scene = JsonUtility.FromJson<Scene>(targetFile.text);

        Debug.Log(scene.steps);

        foreach (Step step in scene.steps)
        {
            step.stepType = ParseStepType(step.type);

            if (step.steps != null && step.steps.Length > 0)
            {
                for (int i = 0; i < step.steps.Length; i++)
                {
                    Substep subStep = step.steps[i];
                    subStep.stepType = ParseStepType(subStep.type);
                    Step simultaneousStep = createStepFromSubstep(subStep);
                    step.steps[i] = simultaneousStep;
                }
            }
            stepQueue.Enqueue(step);
        }
    }
}
