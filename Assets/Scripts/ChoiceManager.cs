using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class ChoiceManager : MonoBehaviour
{
    public CanvasGroup choicePanel;

    public Transform buttonContainer;
    public GameObject choiceButtonPrefab;

    public Action<string> choiceSelectedCallback;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        choicePanel.alpha = 0;
        choicePanel.gameObject.SetActive(false);
    }

    public void ShowChoices(Choice[] choices, Action<string> onChoiceSelected)
    {
        // Clear existing buttons
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // Create new buttons for each choice
        foreach (Choice choice in choices)
        {
            bool hideButton = FlagManager.Instance.GetFlag(choice.hideOnFlag);

            if (hideButton)
                continue;
            
            GameObject buttonObj = Instantiate(choiceButtonPrefab, buttonContainer);
            ChoiceButton choiceButton = buttonObj.GetComponent<ChoiceButton>();
            choiceButton.choiceId = choice.id;
            choiceButton.choiceText = choice.text;
            choiceButton.onChoiceSelected = HandleChoiceSelected;
        }

        // Show the panel
        Debug.Log("Showing choices: " + choices.Length);
        
        choicePanel.gameObject.SetActive(true);
        choicePanel.alpha = 1;
        choiceSelectedCallback = onChoiceSelected;
    }

    public void HandleChoiceSelected(string choiceId)
    {
        Debug.Log("Choice selected: " + choiceId);
        // Hide the panel
        choicePanel.alpha = 0;
        choicePanel.gameObject.SetActive(false);

        // Invoke the callback if it exists
        choiceSelectedCallback?.Invoke(choiceId);
        choiceSelectedCallback = null; // Clear the callback after invoking
    }
}
