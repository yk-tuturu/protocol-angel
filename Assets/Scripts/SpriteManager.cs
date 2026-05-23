using UnityEngine;
using System.Collections.Generic;
using System;
/**
* Stupid hack, but this is a list mapping character names to their corresponding sprite object 
*/
public class SpriteManager : MonoBehaviour
{
    public static SpriteManager Instance { get; private set; }

    public CharacterSprite[] characters;
    public Dictionary<string, CharacterSprite> characterDict = new Dictionary<string, CharacterSprite>();

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
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (CharacterSprite character in characters)
        {
            characterDict[character.name] = character;
        }   
    }

    public void ChangeSprite(string characterName, string spriteName, bool visible = true, Action onComplete = null)
    {
        if (characterDict.ContainsKey(characterName))
        {
            characterDict[characterName].ChangeSprite(spriteName, visible: visible, onComplete: onComplete);
        }
        else
        {
            Debug.LogWarning($"Character with name {characterName} not found!");
        }
    }

    public void ShowSprite(string characterName, string spriteName, Action onComplete = null)
    {
        if (characterDict.ContainsKey(characterName))
        {
            characterDict[characterName].ChangeSprite(spriteName, visible: true, onComplete: onComplete);
        }
        else
        {
            Debug.LogWarning($"Character with name {characterName} not found!");
        }
    }

    public void HideSprite(string characterName, string spriteName, Action onComplete = null)
    {
        if (characterDict.ContainsKey(characterName))
        {
            characterDict[characterName].Hide(spriteName, onComplete);
        }
        else
        {
            Debug.LogWarning($"Character with name {characterName} not found!");
        }
    }

    public void FadeIn(string characterName, string spriteName, float duration, Action onComplete = null)
    {
        if (characterDict.ContainsKey(characterName))
        {
            characterDict[characterName].FadeIn(duration, spriteName,onComplete);
        }
        else
        {
            Debug.LogWarning($"Character with name {characterName} not found!");
        }
    }

    public void FadeOut(string characterName, string spriteName, float duration, Action onComplete = null)
    {
        if (characterDict.ContainsKey(characterName))
        {
            characterDict[characterName].FadeOut(duration, spriteName, onComplete);
        }
        else
        {
            Debug.LogWarning($"Character with name {characterName} not found!");
        }
    }

    public void Jump(string characterName, string spriteName, float duration, float jumpPower, int numJumps, Action onComplete = null)
    {
        if (characterDict.ContainsKey(characterName))
        {
            characterDict[characterName].Jump(duration, jumpPower, numJumps, spriteName, onComplete);
        }
        else
        {
            Debug.LogWarning($"Character with name {characterName} not found!");
        }
    }
}
