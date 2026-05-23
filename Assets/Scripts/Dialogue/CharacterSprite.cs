using UnityEngine;
using System.Collections;   
using System.Collections.Generic;
using DG.Tweening;
using System;

public class CharacterSprite : MonoBehaviour
{
    public string name;
    public SpriteRenderer[] sprites;

    public SpriteRenderer activeSprite;
    public bool visible = false;

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

    // ==================================
    // Sprite animation functions
    // ==================================
    public void ChangeSprite(string spriteName, bool visible = true, Action onComplete = null)
    {
        foreach (SpriteRenderer sprite in sprites)
        {
            sprite.gameObject.SetActive(false);
        }

        if (spriteDict.ContainsKey(spriteName))
        {
            activeSprite = spriteDict[spriteName];
            activeSprite.gameObject.SetActive(visible);
            this.visible = visible;
            
        }
        else
        {
            Debug.LogWarning($"Sprite with name {spriteName} not found!");
        }
        onComplete?.Invoke();
    }

    public void FadeInSprite(float duration, Action onComplete = null)
    {
        if (activeSprite != null)
        {
            StartCoroutine(FadeInCoroutine(activeSprite, duration, onComplete: onComplete));
        }
        else
        {
            Debug.LogWarning($"No active sprite to fade in!");
        }
    }

    public void FadeOutSprite(float duration, Action onComplete = null)
    {
        if (activeSprite != null)
        {
            StartCoroutine(FadeOutCoroutine(activeSprite, duration, onComplete: onComplete));
        }
        else
        {
            Debug.LogWarning($"No active sprite to fade out!");
        }
    }

    public void JumpSprite(float duration, float jumpPower, int numJumps, Action onComplete = null)
    {
        if (activeSprite != null)
        {
            StartCoroutine(Jump(activeSprite, duration, jumpPower, numJumps, onComplete));
        }
        else
        {
            Debug.LogWarning($"No active sprite to jump!");
        }
    }

    // ==================================
    // Sprite animation coroutines
    // ==================================

    IEnumerator FadeInCoroutine(SpriteRenderer sprite, float duration, float delay = 0f, Action onComplete = null)
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
                Debug.Log("Fade in complete");
                onComplete?.Invoke();
                visible = true;
            });
        
        yield return null;
    }

    IEnumerator FadeOutCoroutine(SpriteRenderer sprite, float duration, float delay = 0f, Action onComplete = null)
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
                onComplete?.Invoke();
                visible = false;
            });
        
        yield return null;

    }

    IEnumerator Jump(SpriteRenderer sprite, float duration, float jumpPower, int numJumps, Action onComplete = null)
    {
        Vector3 originalPosition = sprite.transform.position;

        sprite
            .transform
            .DOJump(
                originalPosition,
                jumpPower,
                numJumps,
                duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                sprite.transform.position = originalPosition;
                onComplete?.Invoke();
            });

        yield return null;
    }


}
