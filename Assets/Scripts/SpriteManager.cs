using UnityEngine;
using System.Collections.Generic;
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

    public void ChangeSprite(string characterName, string spriteName)
    {
        if (characterDict.ContainsKey(characterName))
        {
            characterDict[characterName].ChangeSprite(spriteName);
        }
        else
        {
            Debug.LogWarning($"Character with name {characterName} not found!");
        }
    }

    public void FadeInSprite(string characterName, string spriteName, float duration)
    {
        if (characterDict.ContainsKey(characterName))
        {
            characterDict[characterName].FadeInSprite(spriteName, duration);
        }
        else
        {
            Debug.LogWarning($"Character with name {characterName} not found!");
        }
    }

    public void FadeOutSprite(string characterName, string spriteName, float duration)
    {
        if (characterDict.ContainsKey(characterName))
        {
            characterDict[characterName].FadeOutSprite(spriteName, duration);
        }
        else
        {
            Debug.LogWarning($"Character with name {characterName} not found!");
        }
    }
}
