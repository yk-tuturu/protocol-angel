using UnityEngine;
using System.Collections.Generic;

public class StepManager : MonoBehaviour
{
    public static StepManager Instance { get; private set; }

    public SceneSO scene;

    public Queue<StepSO> stepQueue = new Queue<StepSO>();

    public StepSO currentStep;
    public DialogueStepType currentStepType;

    public int simultaneousStepsExecuted = 0;
    public int totalSimultaneousSteps = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (StepSO step in scene.steps)
        {
            stepQueue.Enqueue(step);
        }

        ExecuteNextStep();
    }

    void OnEnable()
        => GameEventManager.NextStep += HandleNextStep;

    void OnDisable()
        => GameEventManager.NextStep -= HandleNextStep;   // critical!

    
    void HandleNextStep()
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

    void ExecuteNextStep()
    {
        if (stepQueue.Count > 0)
        {
            currentStep = stepQueue.Dequeue();
            currentStepType = currentStep.GetStepType();
            Execute(currentStep);
        }
    }

    void Execute(StepSO step)
    {
        switch (step.GetStepType())
        {
            case DialogueStepType.Dialogue:
                HandleDialogueStep((DialogueStepSO)step);
                break;
            case DialogueStepType.ChangeSprite:
                HandleChangeSpriteStep((ChangeSpriteSO)step);
                break;
            case DialogueStepType.Simultaneous:
                simultaneousStepsExecuted = 0;
                break;
            case DialogueStepType.FadeIn:
                HandleFadeInStep((FadeInSO)step);
                break;
            case DialogueStepType.FadeOut:
                HandleFadeOutStep((FadeOutSO)step);
                break;
            default:
                Debug.LogWarning("Unknown step type!");
                break;
        }
    }

    void HandleDialogueStep(DialogueStepSO dialogueStep)
    {
        DialogueManager.Instance.DisplayNextSentence(dialogueStep.speaker, dialogueStep.sentence);
    }

    void HandleChangeSpriteStep(ChangeSpriteSO changeSpriteStep)
    {
        SpriteManager.Instance.ChangeSprite(changeSpriteStep.characterName, changeSpriteStep.spriteName);
    }

    void HandleSimultaneousStep(SimultaneousStepSO simultaneousStep)
    {   
        simultaneousStepsExecuted = 0;
        totalSimultaneousSteps = simultaneousStep.steps.Length;
        foreach (StepSO step in simultaneousStep.steps)
        {
            Execute(step);
        }
    }

    void HandleFadeInStep(FadeInSO fadeInStep)
    {
        SpriteManager.Instance.FadeInSprite(fadeInStep.characterName, fadeInStep.spriteName, fadeInStep.duration);
    }

    void HandleFadeOutStep(FadeOutSO fadeOutStep)
    {
        SpriteManager.Instance.FadeOutSprite(fadeOutStep.characterName, fadeOutStep.spriteName, fadeOutStep.duration);
    }
}
