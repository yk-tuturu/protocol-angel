using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System;

public enum SpriteState
{
    Hidden,     // SetActive(false)
    Visible,    // fully opaque
    FadingIn,
    FadingOut
}

public class CharacterSprite : MonoBehaviour
{
    public string characterName;
    public SpriteRenderer[] sprites;

    private Dictionary<string, SpriteRenderer> _spriteDict  = new Dictionary<string, SpriteRenderer>();
    private Dictionary<string, SpriteState>    _spriteState = new Dictionary<string, SpriteState>();
    private Dictionary<string, Tween>          _activeTween = new Dictionary<string, Tween>();

    public SpriteRenderer activeSprite { get; private set; }
    public SpriteRenderer defaultSprite { get; private set; }
    public SpriteState activeState => activeSprite != null
                                        ? _spriteState[activeSprite.name]
                                        : SpriteState.Hidden;

    void Start()
    {
        foreach (SpriteRenderer sr in sprites)
        {
            _spriteDict[sr.name]  = sr;
            _spriteState[sr.name] = SpriteState.Hidden;
            sr.gameObject.SetActive(false);
        }
        defaultSprite = sprites[0];
    }

    // ── Core transition ───────────────────────────────────────────

    public void ChangeSprite(string spriteName, bool visible = true, Action onComplete = null)
    {
        if (activeSprite != null)
            SetHidden(activeSprite);

        if (!_spriteDict.TryGetValue(spriteName, out SpriteRenderer target))
        {
            Debug.LogWarning($"[CharacterSprite] '{spriteName}' not found on {characterName}");
            onComplete?.Invoke();
            return;
        }

        activeSprite = target;
        activeSprite.gameObject.SetActive(true);
        SetAlpha(activeSprite, visible ? 1f : 0f);
        SetState(activeSprite, visible ? SpriteState.Visible : SpriteState.Hidden);
        onComplete?.Invoke();
    }

    public void Hide(string spriteName = null, Action onComplete = null)
    {
        SpriteRenderer sr = ResolveSprite(spriteName);
        if (sr == null) return;

        SetHidden(sr);

        if (activeSprite == sr)
            activeSprite = null;

        onComplete?.Invoke();
    }

    public void FadeIn(float duration, string spriteName = null, Action onComplete = null)
    {
        // sprite starts invisible before fading in
        SpriteRenderer sr = ResolveSprite(spriteName, visible: false);

        if (_spriteState[sr.name] == SpriteState.Visible) { onComplete?.Invoke(); return; }

        KillTween(sr);
        SetState(sr, SpriteState.FadingIn);
        sr.gameObject.SetActive(true);
        SetAlpha(sr, 0f);

        var tween = sr.DOFade(1f, duration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                SetState(sr, SpriteState.Visible);
                onComplete?.Invoke();
            });

        _activeTween[sr.name] = tween;
    }

    public void FadeOut(float duration, string spriteName = null, Action onComplete = null)
    {
        SpriteRenderer sr = ResolveSprite(spriteName, visible: true);

        if (_spriteState[sr.name] == SpriteState.Hidden) { onComplete?.Invoke(); return; }

        KillTween(sr);
        SetState(sr, SpriteState.FadingOut);

        var tween = sr.DOFade(0f, duration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                SetHidden(sr);
                onComplete?.Invoke();
            });

        _activeTween[sr.name] = tween;
    }

    public void Jump(float duration, float jumpPower, int numJumps, string spriteName = null, Action onComplete = null)
    {
        SpriteRenderer sr = ResolveSprite(spriteName, visible: true);

        Vector3 originalPos = sr.transform.position;

        sr.transform
            .DOJump(originalPos, jumpPower, numJumps, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                sr.transform.position = originalPos;
                onComplete?.Invoke();
            });
    }

    // ── Utility ───────────────────────────────────────────────────

    // ── Resolve sprite ────────────────────────────────────────────

    /// <summary>
    /// Resolves which sprite to use:
    /// 1. Named sprite if provided
    /// 2. Current active sprite
    /// 3. Default sprite as fallback
    /// If a named sprite is provided and differs from active, switches to it first.
    /// </summary>
    private SpriteRenderer ResolveSprite(string spriteName, bool visible = true)
    {
        // named sprite provided
        if (!string.IsNullOrEmpty(spriteName))
        {
            if (_spriteDict.TryGetValue(spriteName, out SpriteRenderer named))
            {
                if (activeSprite != named)
                    ChangeSprite(spriteName, visible);
                return named;
            }
            Debug.LogWarning($"[CharacterSprite] '{spriteName}' not found, falling back.");
        }

        // use active sprite if available
        if (activeSprite != null)
            return activeSprite;

        // fallback to default
        Debug.LogWarning($"[CharacterSprite] No active sprite, falling back to default.");
        ChangeSprite(defaultSprite.name, visible);
        return defaultSprite;
    }

    private void SetHidden(SpriteRenderer sr)
    {
        KillTween(sr);
        sr.gameObject.SetActive(false);
        SetAlpha(sr, 0f);
        SetState(sr, SpriteState.Hidden);
    }

    private void SetState(SpriteRenderer sr, SpriteState state)
        => _spriteState[sr.name] = state;

    private void SetAlpha(SpriteRenderer sr, float alpha)
        => sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);

    private void KillTween(SpriteRenderer sr)
    {
        if (_activeTween.TryGetValue(sr.name, out Tween t) && t.IsActive())
            t.Kill();
        _activeTween.Remove(sr.name);
    }
}