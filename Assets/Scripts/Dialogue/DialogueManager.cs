using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;
using DG.Tweening;

public class DialogueManager : MonoBehaviour
{
    // public Queue<Dialogue> sentences = new Queue<Dialogue>();
    public bool currentlyInDialogue = false;
    public bool currentlyTyping = false;

    public bool showUI = false;

    public string currentSpeaker;
    public string currentSentence;

    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI speechText;
    public CanvasGroup dialoguePanel;

    public float textDelay = 0.02f;

    private Action _onComplete;

    void Start()
    {
        showUI = false;
        dialoguePanel.gameObject.SetActive(false);
        speakerText.text = "";
        speechText.text = "";
    }

    void OnEnable()
    {
        InputManager.Instance.OnClick += HandleClick;
        GameEventManager.EndStep += HandleEndStep;
    }

    void OnDisable()
    {
        InputManager.Instance.OnClick -= HandleClick;
        GameEventManager.EndStep -= HandleEndStep;
    }

    void HandleClick(InputManager.ClickData data)
    {
        if (currentlyInDialogue)
        {
            if (currentlyTyping)
            {
                StopAllCoroutines();
                speechText.text = currentSentence;
                currentlyTyping = false;
            }
            else
            {
                CompleteLine();
            }
        }
    }

    void HandleEndStep(Step step)
    {
        speakerText.text = "";
        speechText.text = "";
    }

    public void ShowPanel(float duration = 0f, Action onComplete = null)
    {
        if (showUI) return;

        if (duration > 0f)
        {
            dialoguePanel.gameObject.SetActive(true);
            dialoguePanel.alpha = 0f;
            dialoguePanel
                .DOFade(1f, duration)
                .OnComplete(() =>
                {
                    showUI = true;
                    onComplete?.Invoke();
                });
        } 
        else
        {
            dialoguePanel.gameObject.SetActive(true);
            showUI = true;
            onComplete?.Invoke();
        }
        
        
    }

    public void HidePanel(float duration = 0f, Action onComplete = null)
    {
        if (duration > 0f)
        {
            
            dialoguePanel.alpha = 1f;
            dialoguePanel
                .DOFade(0f, duration)
                .OnComplete(() =>
                {
                    dialoguePanel.gameObject.SetActive(false);
                    showUI = false;
                    onComplete?.Invoke();
                });
        }
        else
        {
            dialoguePanel.gameObject.SetActive(false);
            showUI = false;
            onComplete?.Invoke();
        }
    }

    public void DisplayNextSentence(string speaker, string sentence, float textDelay = 0.02f, Action onComplete = null)
    {
        if (!showUI)
        {
            ShowPanel();
        }

        _onComplete = onComplete;
        currentSpeaker = speaker;
        currentSentence = sentence;
        this.textDelay = textDelay;

        speakerText.text = speaker;

        currentlyInDialogue = true;
        StopAllCoroutines();
        StartCoroutine(TypeSentence(speaker, sentence));
    }

    IEnumerator TypeSentence(string speaker,string sentence)
    {
        speakerText.text = speaker;
        currentlyTyping = true;

        float timer = 0f;
        int charIndex = 0;
        char[] letters = sentence.ToCharArray();

        while (charIndex < letters.Length)
        {
            timer += Time.deltaTime;

            int charsToShow = Mathf.FloorToInt(timer / textDelay);
            charsToShow = Mathf.Min(charsToShow, letters.Length);

            speakerText.text = speaker;
            speechText.text = sentence.Substring(0, charsToShow);
            charIndex = charsToShow;

            yield return null; 
        }

        currentlyTyping = false;
    }

    public void EndDialogue()
    {
        Debug.Log("End of conversation");
        currentlyInDialogue = false;
    }

    void CompleteLine()
    {
        Action callback = _onComplete;
        _onComplete = null;         
        callback?.Invoke();
        currentlyInDialogue = false;
    }
}
