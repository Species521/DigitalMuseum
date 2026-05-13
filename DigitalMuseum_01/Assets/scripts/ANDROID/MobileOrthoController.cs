using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// Attach to the GameObject that holds your orthographic camera.
/// - Drag anywhere on screen to pan
/// - Pinch to zoom in/out
/// - Camera is clamped to defined world-space bounds
///
/// INSPECTOR SETUP:
///   - panSpeed        : How fast the camera pans (tune to taste)
///   - zoomSensitivity : How fast pinch zoom works
///   - minOrthoSize    : Closest zoom (small value = more zoomed in)
///   - maxOrthoSize    : Furthest zoom
///   - minBounds       : Bottom-left world-space limit of camera movement (X, Y)
///   - maxBounds       : Top-right world-space limit of camera movement (X, Y)
/// </summary>
[RequireComponent(typeof(Camera))]
public class MobileOrthoController : MonoBehaviour
{
    [Header("Pan")]
    public float panSpeed = 0.01f;

    [Header("Zoom (Pinch)")]
    public float zoomSensitivity = 0.05f;
    public float minOrthoSize = 1f;
    public float maxOrthoSize = 20f;

    [Header("Pan Bounds (World Space)")]
    public Vector2 minBounds = new Vector2(-5f, -3f);
    public Vector2 maxBounds = new Vector2(5f, 3f);

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void OnEnable() => EnhancedTouchSupport.Enable();
    void OnDisable() => EnhancedTouchSupport.Disable();

    void Update()
    {
        if (Touch.activeTouches.Count >= 2)
        {
            HandlePinchZoom();
        }
        else if (Touch.activeTouches.Count == 1)
        {
            HandlePan(Touch.activeTouches[0]);
        }
    }

    private void HandlePan(Touch touch)
    {
        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Moved) return;

        // Scale pan speed based on current zoom level (more zoomed in = slower pan)
        float scaledSpeed = panSpeed * cam.orthographicSize;

        // Move camera - X inverted so drag left moves camera right (and vice versa)
        Vector3 newPos = transform.position - new Vector3(-touch.delta.x * scaledSpeed, touch.delta.y * scaledSpeed, 0f);

        // Clamp to bounds
        newPos.x = Mathf.Clamp(newPos.x, minBounds.x, maxBounds.x);
        newPos.y = Mathf.Clamp(newPos.y, minBounds.y, maxBounds.y);

        transform.position = newPos;
    }

    private void HandlePinchZoom()
    {
        var t0 = Touch.activeTouches[0];
        var t1 = Touch.activeTouches[1];

        if (t0.phase != UnityEngine.InputSystem.TouchPhase.Moved &&
            t1.phase != UnityEngine.InputSystem.TouchPhase.Moved) return;

        // Calculate previous and current distance between touches
        Vector2 prevPos0 = t0.screenPosition - t0.delta;
        Vector2 prevPos1 = t1.screenPosition - t1.delta;

        float prevDistance = Vector2.Distance(prevPos0, prevPos1);
        float currentDistance = Vector2.Distance(t0.screenPosition, t1.screenPosition);
        float deltaDistance = currentDistance - prevDistance;

        // Adjust orthographic size (negative delta = pinch in = zoom in = smaller size)
        cam.orthographicSize = Mathf.Clamp(
            cam.orthographicSize - (deltaDistance * zoomSensitivity),
            minOrthoSize,
            maxOrthoSize
        );
    }

    // Visualize the bounds in the Editor for easy setup
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2f, (minBounds.y + maxBounds.y) / 2f, transform.position.z);
        Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}