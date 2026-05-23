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

    public float textDelay = 0.01f;
    public bool append = false; // if true, the current sentence will not be cleared when a new line is displayed

    private Action _onComplete;

    void Start()
    {
        showUI = false;
        dialoguePanel.alpha = 0f;
        speakerText.text = "";
        speechText.text = "";
        currentSpeaker = "";
        currentSentence = "";
        currentlyInDialogue = false;
        currentlyTyping = false;
    }

    void OnEnable()
    {
        Debug.Log("DialogueManager OnEnable");
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
                speechText.maxVisibleCharacters = int.MaxValue; 
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
        if (!append)
        {
            speakerText.text = "";
            speechText.text = "";
            speechText.maxVisibleCharacters = 0;
        }
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
            Debug.Log("Showing dialogue panel immediately");
            dialoguePanel.gameObject.SetActive(true);
            dialoguePanel.alpha = 1f;
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
                    ClearDialogue();
                    showUI = false;
                    onComplete?.Invoke();
                });
        }
        else
        {
            dialoguePanel.alpha = 0f;
            dialoguePanel.gameObject.SetActive(false);
            ClearDialogue();
            showUI = false;
            onComplete?.Invoke();
        }
    }

    // not sure if this will be needed, can prolly encode a function for this in json
    public void ClearDialogue(Action onComplete = null)
    {
        speakerText.text = "";
        speechText.text = "";
        speechText.maxVisibleCharacters = 0;
        currentSpeaker = "";
        currentSentence = "";
        onComplete?.Invoke();
    }

    public void DisplayNextSentence(string speaker, string sentence, float textDelay = 0.01f, bool append = false, Action onComplete = null)
    {
        if (!showUI)
        {
            ShowPanel();
        }

        _onComplete = onComplete;
        currentSpeaker = speaker;
        currentSentence = sentence;
        this.textDelay = textDelay;
        this.append = append;

        speakerText.text = speaker;

        currentlyInDialogue = true;
        StopAllCoroutines();
        StartCoroutine(TypeSentence(speaker, sentence));
    }

    IEnumerator TypeSentence(string speaker, string sentence)
    {
        speakerText.text = speaker;
        currentlyTyping = true;

        speechText.text += sentence;  
        speechText.ForceMeshUpdate();
        int totalChars = speechText.textInfo.characterCount;      
        int prevMaxVisible = speechText.maxVisibleCharacters;

        Debug.Log($"Total characters in sentence: {totalChars}, starting from: {prevMaxVisible}");

        for (int i = prevMaxVisible + 1; i <= totalChars; i++)
        {
            speechText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(textDelay);
        }

        currentlyTyping = false;
    }

    public int GetSentenceLength(string sentence)
    {
        int counter = 0;
        for (int i = 0; i < sentence.Length; i++)
        {
            if (sentence[i] == '<')
            {
                int closingIndex = sentence.IndexOf('>', i);
                if (closingIndex != -1)
                {
                    i = closingIndex; 
                    continue;
                }
            }

            counter++;
        }
        return counter;
    }

    void CompleteLine()
    {
        Action callback = _onComplete;
        _onComplete = null;         
        callback?.Invoke();
        currentlyInDialogue = false;
    }
}
