using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ClickHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [Header("Settings")]
    [SerializeField] private float maxRayDistance = 100f;
    [SerializeField] private LayerMask worldLayerMask = ~0; // everything by default

    [SerializeField] private InputActionAsset inputActions;

    private InputAction _clickAction;
    private InputAction _pointerPositionAction;

    private void Awake()
    {
        var map = inputActions.FindActionMap("UI", throwIfNotFound: true);
        _clickAction          = map.FindAction("Click",   throwIfNotFound: true);
        _pointerPositionAction = map.FindAction("Point",  throwIfNotFound: true);
    }

    private void OnEnable()
    {
        _pointerPositionAction.Enable();

        _clickAction.performed += OnClick;
        _clickAction.Enable();
    }

    private void OnDisable()
    {
        _clickAction.performed -= OnClick;
        _clickAction.Disable();
        _pointerPositionAction.Disable();
    }

    private void OnDestroy()
    {
        _clickAction.Dispose();
        _pointerPositionAction.Dispose();
    }

    // ─────────────────────────────────────────────────────────────
    private void OnClick(InputAction.CallbackContext ctx)
    {
        Vector2 screenPos = _pointerPositionAction.ReadValue<Vector2>();

        // ── 1. Screen / UI position ───────────────────────────────
        Debug.Log($"[Click] Screen pos: {screenPos}");

        // ── 2. Block when clicking on UI ──────────────────────────
        if (IsPointerOverUI(screenPos))
        {
            Debug.Log("[Click] Over UI — skipping world raycast");
            HandleUIClick(screenPos);
            return;
        }

        // ── 3. World position via Raycast ─────────────────────────
        Ray ray = mainCamera.ScreenPointToRay(screenPos);

        // 3D hit
        if (Physics.Raycast(ray, out RaycastHit hit3D, maxRayDistance, worldLayerMask))
        {
            Debug.Log($"[Click] 3D world pos: {hit3D.point}  |  object: {hit3D.collider.gameObject.name}");
            HandleWorldClick3D(hit3D);
        }

        // 2D hit  (remove if you don't use 2D physics)
        RaycastHit2D hit2D = Physics2D.Raycast(ray.origin, ray.direction, maxRayDistance, worldLayerMask);
        if (hit2D.collider != null)
        {
            Debug.Log($"[Click] 2D world pos: {hit2D.point}  |  object: {hit2D.collider.gameObject.name}");
            HandleWorldClick2D(hit2D);
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────

    /// <summary>True when the pointer is over any UI element.</summary>
    private bool IsPointerOverUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;

        var pointerData = new PointerEventData(EventSystem.current) { position = screenPos };
        var results     = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        return results.Count > 0;
    }

    /// <summary>Called when clicking on a UI element.</summary>
    private void HandleUIClick(Vector2 screenPos)
    {
        // screenPos IS the UI position in pixels (same space as RectTransform)
        // Convert to viewport if needed:
        Vector2 viewportPos = mainCamera.ScreenToViewportPoint(screenPos);
        Debug.Log($"[Click] UI viewport pos: {viewportPos}");
    }

    private void HandleWorldClick3D(RaycastHit hit) { /* your game logic */ }
    private void HandleWorldClick2D(RaycastHit2D hit) { /* your game logic */ }
}