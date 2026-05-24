using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class ChoiceButton : MonoBehaviour
{
    public string choiceId;
    public string choiceText;

    public TextMeshProUGUI choiceTextUI;
    public Action<string> onChoiceSelected;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        choiceTextUI.text = choiceText;
    }

    public void OnClick()
    {
        onChoiceSelected?.Invoke(choiceId);
    }
}
