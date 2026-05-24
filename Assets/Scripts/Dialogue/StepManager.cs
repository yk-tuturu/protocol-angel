using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class StepManager : MonoBehaviour
{
    public static StepManager Instance { get; private set; }

    public Scene scene;

    public Queue<Step> stepQueue = new Queue<Step>();
    public List<Step> stepList = new List<Step>();

    public Dictionary<string, int> stepIdToIndex = new Dictionary<string, int>();

    public int stepIndex = 0;

    public Step currentStep;
    public DialogueStepType currentStepType;

    public int simultaneousStepsExecuted = 0;
    public int totalSimultaneousSteps = 0;

    // Dialogue managers
    // this probably shouldnt be managed here but for now its fine ig
    public DialogueManager defaultDialogueManager;
    public DialogueManager blackDialogueManager;
    public DialogueManager scpDialogueManager;

    public ChoiceManager choiceManager;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadSceneFromJson("prologue.json");

        ExecuteNextStep();
    }

    private void ExecuteNextStep()
    {
        if (stepIndex < stepList.Count)
        {
            currentStep = stepList[stepIndex];
            currentStepType = currentStep.stepType;
            StartCoroutine(ExecuteWithDelay(currentStep, ExecuteNextStep));
        }
    }

    // wrapper to handle delays
    IEnumerator ExecuteWithDelay(Step step, Action onComplete = null, bool isSubstep = false)
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

        if (!isSubstep)
        {
            Debug.Log($"Raising EndStep event for step: {step.type}");
            GameEventManager.Raise_EndStep(currentStep);
        }

        // handling jumps and branching logic 
        if (currentStep.jumpTargetId != null)
        {
            stepIndex = stepIdToIndex[currentStep.jumpTargetId];
        }
        else if (!isSubstep && currentStepType != DialogueStepType.Choice) // dont increment if is choice, since choice steps will handle their own jumps
        {
            stepIndex++;
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
            case DialogueStepType.ShowSprite:
                HandleShowSpriteStep(step, onComplete);
                break;
            case DialogueStepType.HideSprite:
                HandleHideSpriteStep(step, onComplete);
                break;
            case DialogueStepType.ShowPanel:
                HandleShowPanelStep(step, onComplete);
                break;
            case DialogueStepType.HidePanel:
                HandleHidePanelStep(step, onComplete);
                break;
            case DialogueStepType.ClearDialogue:
                HandleClearDialogueStep(step, onComplete);
                break;
            case DialogueStepType.SetFlag:
                HandleSetFlagStep(step, onComplete);
                break;
            case DialogueStepType.Choice:
                HandleChoiceStep(step, onComplete);
                break;
            case DialogueStepType.ConditionalJump:
                HandleConditionalJumpStep(step, onComplete);
                break;
            default:
                Debug.LogWarning("Unknown step type!");
                break;
        }
    }
    
    //================================
    // Dialogue related handlers
    // ===============================
    private DialogueManager GetDialogueManager(string dialoguePanel)
    {
        switch (dialoguePanel.ToLower())
        {
            case "default":
                return defaultDialogueManager;
            case "black":
                return blackDialogueManager;
            case "scp":
                return scpDialogueManager;
            default:
                Debug.LogWarning($"Unknown dialogue panel: {dialoguePanel}, defaulting to defaultDialogueManager");
                return defaultDialogueManager;
        }
    }

    private void HandleDialogueStep(Step step, Action onComplete)
    {
        var dialogueManager = GetDialogueManager(step.dialoguePanel);

        dialogueManager.DisplayNextSentence(
            step.speaker, step.sentence, step.textDelay, step.append,
            () => onComplete?.Invoke());

        if (!string.IsNullOrEmpty(step.characterName) && !string.IsNullOrEmpty(step.spriteName))
        {
            SpriteManager.Instance.ChangeSprite(step.characterName, step.spriteName);
        }
    }

    private void HandleShowPanelStep(Step step, Action onComplete)
    {
        var dialogueManager = GetDialogueManager(step.dialoguePanel);
        dialogueManager.ShowPanel(step.duration, () => onComplete());
    }

    private void HandleClearDialogueStep(Step step, Action onComplete)
    {
        var dialogueManager = GetDialogueManager(step.dialoguePanel);
        dialogueManager.ClearDialogue(() => onComplete());
    }

    private void HandleHidePanelStep(Step step, Action onComplete)
    {
        var dialogueManager = GetDialogueManager(step.dialoguePanel);
        dialogueManager.HidePanel(step.duration, () => onComplete());
    }

    // ================================
    // Simultaneous step handler
    // ================================
    
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
                if (completed >= total)
                    onComplete?.Invoke();
            }, isSubstep: true));
        }
    }

    //================================
    // Sprite related handlers
    // ===============================
    private void HandleShowSpriteStep(Step step, Action onComplete)
    {
        SpriteManager.Instance.ShowSprite(step.characterName, step.spriteName, () => onComplete());
    }

    private void HandleHideSpriteStep(Step step, Action onComplete)
    {
        SpriteManager.Instance.HideSprite(step.characterName, step.spriteName, () => onComplete());
    }

    private void HandleChangeSpriteStep(Step step, Action onComplete)
    {
        SpriteManager.Instance.ChangeSprite(step.characterName, step.spriteName, true, () => onComplete());
    }


    private void HandleFadeInStep(Step fadeInStep, Action onComplete)
    {
        SpriteManager.Instance.FadeIn(
            fadeInStep.characterName, fadeInStep.spriteName, fadeInStep.duration,
            () => onComplete?.Invoke());
    }

    private void HandleFadeOutStep(Step fadeOutStep, Action onComplete)
    {
        SpriteManager.Instance.FadeOut(
            fadeOutStep.characterName, fadeOutStep.spriteName, fadeOutStep.duration,
            () => onComplete?.Invoke());
    }

    private void HandleJumpStep(Step jumpStep, Action onComplete)
    {
        SpriteManager.Instance.Jump(
            jumpStep.characterName, jumpStep.spriteName, jumpStep.duration, jumpStep.jumpPower, jumpStep.numJumps, 
            () => onComplete?.Invoke());
    }

    //================================
    // Branching handlers
    // ===============================
    private void HandleSetFlagStep(Step step, Action onComplete)
    {
        FlagManager.Instance.SetFlag(step.flag, true);
        onComplete?.Invoke();
    }

    private void HandleChoiceStep(Step step, Action onComplete)
    {
        choiceManager.ShowChoices(step.choices, choiceId => {
            Choice selectedChoice = Array.Find(step.choices, c => c.id == choiceId);
            if (selectedChoice != null)
            {
                step.jumpTargetId = selectedChoice.jumpTargetId;
            }
            else
            {
                Debug.LogWarning($"Selected choice ID {choiceId} not found in choices!");
            }

            onComplete?.Invoke();
        });
    }

    private void HandleConditionalJumpStep(Step step, Action onComplete)
    {
        foreach (Condition condition in step.conditions)
        {
            bool allFlagsSet = true;
            foreach (string flag in condition.flags)
            {
                if (!FlagManager.Instance.GetFlag(flag))
                {
                    allFlagsSet = false;
                    break;
                }
            }

            if (allFlagsSet)
            {
                step.jumpTargetId = condition.jumpTargetId;
                onComplete?.Invoke();
                return;
            }
        }

        // if no conditions met, jump to default
        step.jumpTargetId = step.defaultJumpTargetId;
        onComplete?.Invoke();
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
            case "showsprite":
                return DialogueStepType.ShowSprite;
            case "hidesprite":
                return DialogueStepType.HideSprite;
            case "showpanel":
                return DialogueStepType.ShowPanel;
            case "hidepanel":
                return DialogueStepType.HidePanel;
            case "cleardialogue":
                return DialogueStepType.ClearDialogue;
            case "setflag":
                return DialogueStepType.SetFlag;
            case "conditionaljump":
                return DialogueStepType.ConditionalJump;
            case "choice":
                return DialogueStepType.Choice;
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
            postDelay = subStep.postDelay,
            dialoguePanel = subStep.dialoguePanel,
            append = subStep.append,
            choiceId = subStep.choiceId,
            choices = subStep.choices,
            jumpTargetId = subStep.jumpTargetId,
            flag = subStep.flag,
            conditions = subStep.conditions,
            defaultJumpTargetId = subStep.defaultJumpTargetId
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

            stepList.Add(step);
            stepQueue.Enqueue(step);

            if (step.id != null)
            {
                stepIdToIndex[step.id] = stepList.Count - 1;
            }
        }
    }
}
