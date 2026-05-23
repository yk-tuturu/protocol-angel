using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class DialogueManager : MonoBehaviour
{
    // public Queue<Dialogue> sentences = new Queue<Dialogue>();
    public bool currentlyInDialogue = false;
    public bool currentlyTyping = false;

    public string currentSpeaker;
    public string currentSentence;

    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI speechText;
    public GameObject dialoguePanel;

    public float textDelay = 0.02f;

    public static DialogueManager Instance { get; private set; }

    private Action _onComplete;

    void Awake()
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
        // sentences = new Queue<Dialogue>();
    }

    void OnEnable()
    {
        InputManager.Instance.OnClick += HandleClick;
    }

    void OnDisable()
    {
        InputManager.Instance.OnClick -= HandleClick;
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
                speakerText.text = "";
                speechText.text = "";
                CompleteLine();
            }
        }
    }

    public void ShowPanel()
    {
        dialoguePanel.SetActive(true);
    }

    public void HidePanel()
    {
        dialoguePanel.SetActive(false);
    }

    public void DisplayNextSentence(string speaker, string sentence, float textDelay = 0.02f, Action onComplete = null)
    {
        _onComplete = onComplete;
        currentSpeaker = speaker;
        currentSentence = sentence;
        this.textDelay = textDelay;

        speakerText.text = speaker;

        currentlyInDialogue = true;
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        speechText.text = "";
        currentlyTyping = true;

        float timer = 0f;
        int charIndex = 0;
        char[] letters = sentence.ToCharArray();

        while (charIndex < letters.Length)
        {
            timer += Time.deltaTime;

            // how many chars should have appeared by now
            int charsToShow = Mathf.FloorToInt(timer / textDelay);
            charsToShow = Mathf.Min(charsToShow, letters.Length);

            speechText.text = sentence.Substring(0, charsToShow);
            charIndex = charsToShow;

            yield return null; // run every frame, no WaitForSeconds
        }

        currentlyTyping = false;
    }

    public void EndDialogue()
    {
        Debug.Log("End of conversation");
        currentlyInDialogue = false;

        speakerText.text = "";
        speechText.text = "";
    }

    void CompleteLine()
    {
        Action callback = _onComplete;
        _onComplete = null;         // clear first so double-clicks can't fire it twice
        callback?.Invoke();
    }
}
