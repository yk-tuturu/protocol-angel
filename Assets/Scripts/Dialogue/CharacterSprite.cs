using UnityEngine;
using System.Collections;   
using System.Collections.Generic;
using DG.Tweening;

public class CharacterSprite : MonoBehaviour
{
    public string name;
    public SpriteRenderer[] sprites;

    public Dictionary<string, SpriteRenderer> spriteDict = new Dictionary<string, SpriteRenderer>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (SpriteRenderer sprite in sprites)
        {
            spriteDict[sprite.name] = sprite;
        }

        foreach (SpriteRenderer sprite in sprites)
        {
            sprite.gameObject.SetActive(false);
        }
    }

    public void ChangeSprite(string spriteName)
    {
        foreach (SpriteRenderer sprite in sprites)
        {
            sprite.gameObject.SetActive(false);
        }

        if (spriteDict.ContainsKey(spriteName))
        {
            spriteDict[spriteName].gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Sprite with name {spriteName} not found!");
        }

        GameEventManager.Raise_NextStep();
    }

    public void FadeInSprite(string spriteName, float duration)
    {
        if (spriteDict.ContainsKey(spriteName))
        {
            SpriteRenderer sprite = spriteDict[spriteName];
            StartCoroutine(FadeInCoroutine(sprite, duration));
        }
        else
        {
            Debug.LogWarning($"Sprite with name {spriteName} not found!");
        }
    }

    public void FadeOutSprite(string spriteName, float duration)
    {
        if (spriteDict.ContainsKey(spriteName))
        {
            SpriteRenderer sprite = spriteDict[spriteName];
            StartCoroutine(FadeOutCoroutine(sprite, duration));
        }
        else
        {
            Debug.LogWarning($"Sprite with name {spriteName} not found!");
        }
    }

    IEnumerator FadeInCoroutine(SpriteRenderer sprite, float duration, float delay = 0f)
    {
        sprite.gameObject.SetActive(true);
        sprite.color = new Color(
                sprite.color.r,
                sprite.color.g,
                sprite.color.b, 0f);

        sprite
            .DOFade(1f, duration)
            .SetDelay(delay)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                sprite.color = new Color(
                    sprite.color.r,
                    sprite.color.g,
                    sprite.color.b, 1f);
                GameEventManager.Raise_NextStep();
            });
        
        yield return null;
    }

    IEnumerator FadeOutCoroutine(SpriteRenderer sprite, float duration, float delay = 0f)
    {
        sprite.gameObject.SetActive(true);
        sprite.color = new Color(
                sprite.color.r,
                sprite.color.g,
                sprite.color.b, 1f);

        sprite
            .DOFade(0f, duration)
            .SetDelay(delay)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                sprite.color = new Color(
                    sprite.color.r,
                    sprite.color.g,
                    sprite.color.b, 0f);
                GameEventManager.Raise_NextStep();
            });
        
        yield return null;

    }


}
