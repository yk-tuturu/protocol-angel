using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    // ── Event Data ────────────────────────────────────────────────
    public struct ClickData
    {
        public Vector2 ScreenPos;   // raw screen pixels
        public Vector2 WorldPos;    // 2D world space
        public bool    IsOverUI;
    }

    // ── Events ────────────────────────────────────────────────────
    public delegate void KeyInput();
    public event KeyInput OnAnyKeyPressed;

    public delegate void ClickInput(ClickData data);
    public event ClickInput OnClick;        // fires for every click
    public event ClickInput OnWorldClick;   // fires only when NOT over UI
    public event ClickInput OnUIClick;      // fires only when over UI

    // ── Private ───────────────────────────────────────────────────
    private InputAction _clickAction;
    private InputAction _pointerPositionAction;
    private Camera      _camera;

    // ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _camera = Camera.main;

        _pointerPositionAction = new InputAction("PointerPosition", binding: "<Pointer>/position");
        _clickAction           = new InputAction("Click",           binding: "<Pointer>/press");
    }

    void OnEnable()
    {
        _pointerPositionAction.Enable();
        _clickAction.performed += HandleClick;
        _clickAction.Enable();
    }

    void OnDisable()
    {
        _clickAction.performed -= HandleClick;
        _clickAction.Disable();
        _pointerPositionAction.Disable();
    }

    void OnDestroy()
    {
        _clickAction.Dispose();
        _pointerPositionAction.Dispose();
    }

    // ─────────────────────────────────────────────────────────────
    private void HandleClick(InputAction.CallbackContext ctx)
    {
        Vector2 screenPos = _pointerPositionAction.ReadValue<Vector2>();

        var data = new ClickData
        {
            ScreenPos = screenPos,
            WorldPos  = ScreenToWorld2D(screenPos),
            IsOverUI  = IsPointerOverUI(screenPos)
        };

        OnClick?.Invoke(data);

        if (data.IsOverUI) OnUIClick?.Invoke(data);
        else               OnWorldClick?.Invoke(data);
    }

    // ── Helpers ───────────────────────────────────────────────────
    private Vector2 ScreenToWorld2D(Vector2 screenPos)
    {
        return _camera.ScreenToWorldPoint(screenPos); // z ignored in 2D
    }

    private bool IsPointerOverUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;
        var pointerData = new PointerEventData(EventSystem.current) { position = screenPos };
        var results     = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        return results.Count > 0;
    }
}