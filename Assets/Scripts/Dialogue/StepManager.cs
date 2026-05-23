using UnityEngine;
using System.Collections.Generic;
using System.Collections;
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
            StartCoroutine(ExecuteWithDelay(currentStep, ExecuteNextStep));
        }
    }

    // wrapper to handle delays
    IEnumerator ExecuteWithDelay(Step step, Action onComplete = null)
    {
        if (step.preDelay > 0f)
        {
            yield return new WaitForSeconds(step.preDelay);
        }

        bool done = false;
        Execute(step, () => done = true);
        yield return new WaitUntil(() => done);

        if (step.postDelay > 0f)
        {
            yield return new WaitForSeconds(step.postDelay);
        }

        onComplete?.Invoke();
        yield return null;
    }

    // actual execution logic 
    void Execute(Step step, Action onComplete)
    {
        switch (step.stepType)
        {
            case DialogueStepType.Dialogue:
                HandleDialogueStep(step, onComplete);
                break;
            case DialogueStepType.ChangeSprite:
                HandleChangeSpriteStep(step, onComplete);
                break;
            case DialogueStepType.Simultaneous:
                HandleSimultaneousStep(step, onComplete);
                break;
            case DialogueStepType.FadeIn:
                HandleFadeInStep(step, onComplete);
                break;
            case DialogueStepType.FadeOut:
                HandleFadeOutStep(step, onComplete);
                break;
            case DialogueStepType.Jump:
                HandleJumpStep(step, onComplete);
                break;
            default:
                Debug.LogWarning("Unknown step type!");
                break;
        }
    }

    

    private void HandleDialogueStep(Step step, Action onComplete)
    {
        DialogueManager.Instance.DisplayNextSentence(
            step.speaker, step.sentence, step.textDelay,
            () => onComplete?.Invoke());

        if (!string.IsNullOrEmpty(step.characterName) && !string.IsNullOrEmpty(step.spriteName))
        {
            SpriteManager.Instance.ChangeSprite(step.characterName, step.spriteName);
        }
    }

    private void HandleChangeSpriteStep(Step step, Action onComplete)
    {
        SpriteManager.Instance.ChangeSprite(step.characterName, step.spriteName, true, () => onComplete());
    }

    private void HandleSimultaneousStep(Step step, Action onComplete)
    {
        if (step.steps == null || step.steps.Length == 0)
        {
            Debug.LogWarning("Simultaneous step has no substeps!");
            onComplete?.Invoke();
            return;
        }

        int total = step.steps.Length;
        int completed = 0;

        foreach (Step subStep in step.steps)
        {
            StartCoroutine(ExecuteWithDelay(subStep, () => {
                completed++;
                Debug.Log($"Completed simultaneous substep {completed}/{total}");
                if (completed >= total)
                    onComplete?.Invoke();
            }));
        }
    }

    private void HandleFadeInStep(Step fadeInStep, Action onComplete)
    {
        if (!string.IsNullOrEmpty(fadeInStep.characterName) && !string.IsNullOrEmpty(fadeInStep.spriteName))
        {
            SpriteManager.Instance.ChangeSprite(fadeInStep.characterName, fadeInStep.spriteName, visible: false);
        }

        SpriteManager.Instance.FadeInSprite(
            fadeInStep.characterName, fadeInStep.spriteName, fadeInStep.duration,
            () => onComplete?.Invoke());
    }

    private void HandleFadeOutStep(Step fadeOutStep, Action onComplete)
    {
        if (!string.IsNullOrEmpty(fadeOutStep.characterName) && !string.IsNullOrEmpty(fadeOutStep.spriteName))
        {
            SpriteManager.Instance.ChangeSprite(fadeOutStep.characterName, fadeOutStep.spriteName, visible: true);
        }

        SpriteManager.Instance.FadeOutSprite(
            fadeOutStep.characterName, fadeOutStep.spriteName, fadeOutStep.duration,
            () => onComplete?.Invoke());
    }

    private void HandleJumpStep(Step jumpStep, Action onComplete)
    {
        if (!string.IsNullOrEmpty(jumpStep.characterName) && !string.IsNullOrEmpty(jumpStep.spriteName))
        {
            SpriteManager.Instance.ChangeSprite(jumpStep.characterName, jumpStep.spriteName);
        }

        SpriteManager.Instance.JumpSprite(
            jumpStep.characterName, jumpStep.spriteName, jumpStep.duration, jumpStep.jumpPower, jumpStep.numJumps, 
            () => onComplete?.Invoke());
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
            numJumps = subStep.numJumps,
            preDelay = subStep.preDelay,
            postDelay = subStep.postDelay
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
