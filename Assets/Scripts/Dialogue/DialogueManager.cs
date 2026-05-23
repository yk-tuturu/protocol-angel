using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    public static DialogueManager Instance { get; private set; }

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
                GameEventManager.Raise_NextStep();
            }
        }
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Mouse0) && currentlyInDialogue)
        // {
        //     if (currentlyTyping)
        //     {
        //         StopAllCoroutines();
        //         speechText.text = currentSentence;
        //         currentlyTyping = false;
        //     }
        //     else
        //     {
        //         GameEventManager.Raise_NextStep();
        //     }
        // }
    }

    // public void StartDialogue(List<Dialogue> story)
    // {
    //     sentences.Clear();
    //     dialoguePanel.SetActive(true);
    //     currentlyInDialogue = true;

    //     foreach (Dialogue dialogue in story) {
    //         sentences.Enqueue(dialogue);    
    //     }

    //     DisplayNextSentence();
    // }

    public void ShowPanel()
    {
        dialoguePanel.SetActive(true);
    }

    public void HidePanel()
    {
        dialoguePanel.SetActive(false);
    }

    public void DisplayNextSentence(string speaker, string sentence)
    {
        currentSpeaker = speaker;
        currentSentence = sentence;

        speakerText.text = speaker;

        currentlyInDialogue = true;
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        // play text audio here
        speechText.text = "";
        currentlyTyping = true;
        foreach(char letter in sentence.ToCharArray())
        {
            speechText.text += letter;
            yield return null;
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
}
