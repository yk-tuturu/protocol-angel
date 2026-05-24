using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;
using DG.Tweening;

public class DialogueManager : MonoBehaviour
{
    // State
    public bool currentlyInDialogue = false;
    public bool currentlyTyping = false;
    public bool showUI = false;
    public bool append = false; // if true, the current sentence will not be cleared when a new line is displayed

    // private state
    private string targetSpeaker;
    private string targetSentence;
    private int targetSentenceLength;
    private Action _onComplete;

    // gameobject refs
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI speechText;
    public CanvasGroup dialoguePanel;
    
    // indicator
    public bool useNextIndicator = false;
    public Image nextIndicator;

    public ScrollRect scrollRect;

    public float defaultTextDelay = 0.01f;

    void Start()
    {
        
        dialoguePanel.alpha = 0f;

        speakerText.text = "";
        speechText.text = "";
        targetSpeaker = "";
        targetSentence = "";

        targetSentenceLength = 0;
        currentlyInDialogue = false;
        currentlyTyping = false;
        showUI = false;
        append = false;

        HideIndicator();
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
            Debug.Log("DialogueManager detected click during dialogue");
            if (currentlyTyping)
            {
                SkipTyping();
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
            ClearDialogue();
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
                    Debug.Log("Hiding dialogue panel on complete");
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
        targetSpeaker = "";
        targetSentence = "";
        targetSentenceLength = 0;
        HideIndicator();

        onComplete?.Invoke();
    }

    public void DisplayNextSentence(string speaker, string sentence, float? textDelay = null, bool append = false, Action onComplete = null)
    {
        if (!showUI)
        {
            ShowPanel();
        }

        _onComplete = onComplete;
        targetSpeaker = speaker;
        targetSentence = this.append ? speechText.text + sentence : sentence;

        this.append = append;

        speakerText.text = speaker;

        currentlyInDialogue = true;
        StopAllCoroutines();
        StartCoroutine(TypeSentence(targetSpeaker, targetSentence, textDelay ?? defaultTextDelay));
    }

    IEnumerator TypeSentence(string speaker, string sentence, float textDelay)
    {
        speakerText.text = speaker;
        currentlyTyping = true;

        Debug.Log($"Typing sentence: {sentence}");
        Debug.Log($"Current speechText before typing: {speechText.text}");
        Debug.Log($"Current maxVisibleCharacters: {speechText.maxVisibleCharacters}");

        speechText.text = sentence;  
        speechText.ForceMeshUpdate();

        int totalChars = speechText.textInfo.characterCount;
        targetSentenceLength = totalChars;
    
        int prevMaxVisible = speechText.maxVisibleCharacters;

        Debug.Log($"Total characters in sentence: {totalChars}, starting from: {prevMaxVisible}");

        for (int i = prevMaxVisible + 1; i <= totalChars; i++)
        {
            speechText.maxVisibleCharacters = i;
            ScrollToBottom();
            yield return new WaitForSeconds(textDelay);
        }

        currentlyTyping = false;
        ScrollToBottom();

        if (useNextIndicator)
        {
            ShowIndicator();
        }
    }

    private void PositionIndicator()
    {
        speechText.ForceMeshUpdate();
        var textInfo = speechText.textInfo;

        var localPos = Vector3.zero;

        // Walk backwards to find the last visible character
        for (int i = speechText.maxVisibleCharacters - 1; i >= 0; i--)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            localPos = charInfo.topRight;
            break;
        }

        var lineCount = textInfo.lineCount;
        var lineInfo = textInfo.lineInfo[lineCount - 1];

        var lineHeight = lineInfo.lineHeight;
        var descender = lineInfo.descender;

        Debug.Log("lineHeight: " + lineHeight);
        localPos.y = lineInfo.descender + lineHeight / 2f;
        localPos.x += 25f;

        Vector3 worldPos = speechText.transform.TransformPoint(localPos);
        nextIndicator.transform.position = worldPos;
    }

    private void ShowIndicator()
    {
        if (!useNextIndicator) return;
        
        DOTween.Kill(nextIndicator);
        nextIndicator.gameObject.SetActive(true);
        PositionIndicator();
        nextIndicator.DOFade(0f, 0.6f)
            .From(1f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void HideIndicator()
    {
        if (!useNextIndicator) return;

        DOTween.Kill(nextIndicator);
        nextIndicator.gameObject.SetActive(false);
    }

    void SkipTyping()
    {
        StopAllCoroutines();
        speechText.maxVisibleCharacters = targetSentenceLength;
        currentlyTyping = false;
        ScrollToBottom();
        ShowIndicator();
    }

    void CompleteLine()
    {
        Action callback = _onComplete;
        _onComplete = null;         
        callback?.Invoke();
        currentlyInDialogue = false;
        HideIndicator();
    }

    private void ScrollToBottom()
    {
        if (!scrollRect) return;

        // Must happen after layout updates — defer to end of frame
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
