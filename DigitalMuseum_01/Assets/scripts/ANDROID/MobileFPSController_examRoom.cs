using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[RequireComponent(typeof(CharacterController))]
public class MobileFPSController_examRoom : MonoBehaviour
{
    [Header("AR References")]
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform cameraOffset;

    [Header("Movement (Touchpad)")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private RectTransform touchpadButton;
    [SerializeField] private float touchpadRadius = 100f;
    [SerializeField] private Canvas canvas;

    [Header("Look (Swipe)")]
    [SerializeField] private float lookSensitivity = 0.15f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float minPitch = -80f;

    [Header("Magnification (Pinch)")]
    [SerializeField] private float zoomSensitivity = 0.1f;
    [SerializeField] private float minFOV = 15f;  // High magnification
    [SerializeField] private float maxFOV = 60f;  // Standard view

    [Header("AR Local Translation Clamp")]
    [SerializeField] private bool clampARLocalOffset = true;
    [SerializeField] private Vector3 maxARLocalOffset = new Vector3(0.25f, 0.15f, 0.35f);

    private CharacterController characterController;
    private Camera camComponent;
    private float cameraPitch = 0f;

    // Touch tracking
    private int moveTouchId = -1;
    private int lookTouchId = -1;
    private Vector2 moveStartPos;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (mainCamera == null && Camera.main != null)
            mainCamera = Camera.main.transform;

        if (mainCamera != null)
        {
            camComponent = mainCamera.GetComponent<Camera>();
            cameraPitch = mainCamera.localEulerAngles.x;
        }
    }

    void OnEnable() => EnhancedTouchSupport.Enable();
    void OnDisable() => EnhancedTouchSupport.Disable();

    void Update()
    {
        HandleInput();
        ApplyGravity();
    }

    void LateUpdate()
    {
        if (clampARLocalOffset)
            ClampARLocalTranslation();
    }

    private void HandleInput()
    {
        // First, handle assignments for new touches
        foreach (var touch in Touch.activeTouches)
        {
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                bool isOnTouchpad = touchpadButton != null &&
                                   RectTransformUtility.RectangleContainsScreenPoint(
                                       touchpadButton, touch.screenPosition, canvas != null ? canvas.worldCamera : null);

                if (isOnTouchpad && moveTouchId == -1)
                {
                    moveTouchId = touch.touchId;
                    moveStartPos = touch.screenPosition;
                }
                else if (!isOnTouchpad && lookTouchId == -1)
                {
                    lookTouchId = touch.touchId;
                }
            }
        }

        // Logic branching: Zoom (2+ fingers) vs Move/Look (1 finger)
        if (Touch.activeTouches.Count >= 2)
        {
            HandlePinchZoom();
        }
        else
        {
            // Process individual touches only if we aren't zooming
            foreach (var touch in Touch.activeTouches)
            {
                if (touch.touchId == moveTouchId) ProcessMovement(touch);
                if (touch.touchId == lookTouchId) ProcessLook(touch);
            }
        }

        // Cleanup
        foreach (var touch in Touch.activeTouches)
        {
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended || touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                if (touch.touchId == moveTouchId) moveTouchId = -1;
                if (touch.touchId == lookTouchId) lookTouchId = -1;
            }
        }
    }

    private void HandlePinchZoom()
    {
        if (camComponent == null) return;

        // Use the first two active touches for pinch
        var t0 = Touch.activeTouches[0];
        var t1 = Touch.activeTouches[1];

        if (t0.phase == UnityEngine.InputSystem.TouchPhase.Moved || t1.phase == UnityEngine.InputSystem.TouchPhase.Moved)
        {
            Vector2 prevPos0 = t0.screenPosition - t0.delta;
            Vector2 prevPos1 = t1.screenPosition - t1.delta;

            float prevDistance = Vector2.Distance(prevPos0, prevPos1);
            float currentDistance = Vector2.Distance(t0.screenPosition, t1.screenPosition);
            float deltaDistance = currentDistance - prevDistance;

            // Update FOV (Zoom in when fingers spread apart)
            float newFOV = camComponent.fieldOfView - (deltaDistance * zoomSensitivity);
            camComponent.fieldOfView = Mathf.Clamp(newFOV, minFOV, maxFOV);
        }
    }

    private void ProcessMovement(Touch touch)
    {
        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved || touch.phase == UnityEngine.InputSystem.TouchPhase.Stationary)
        {
            Vector2 delta = touch.screenPosition - moveStartPos;
            Vector2 dir = delta.sqrMagnitude > 0.0001f ? delta.normalized : Vector2.zero;
            float strength = Mathf.Clamp01(delta.magnitude / touchpadRadius);

            // Use full camera orientation — forward includes pitch (up/down tilt)
            Vector3 camForward = mainCamera != null ? mainCamera.forward : transform.forward;
            Vector3 camRight = mainCamera != null ? mainCamera.right : transform.right;

            Vector3 move = (camForward * dir.y) + (camRight * dir.x);
            characterController.Move(move * moveSpeed * strength * Time.deltaTime);
        }
    }

    private void ProcessLook(Touch touch)
    {
        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
        {
            Vector2 delta = touch.delta;
            transform.Rotate(Vector3.up, delta.x * lookSensitivity);

            cameraPitch -= delta.y * lookSensitivity;
            cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

            if (mainCamera != null)
                mainCamera.localEulerAngles = new Vector3(cameraPitch, mainCamera.localEulerAngles.y, 0f);
        }
    }

    private void ApplyGravity()
    {
        if (!characterController.isGrounded)
            characterController.Move(Physics.gravity * Time.deltaTime);
    }

    private void ClampARLocalTranslation()
    {
        if (cameraOffset == null) return;
        Vector3 p = cameraOffset.localPosition;
        p.x = Mathf.Clamp(p.x, -maxARLocalOffset.x, maxARLocalOffset.x);
        p.y = Mathf.Clamp(p.y, -maxARLocalOffset.y, maxARLocalOffset.y);
        p.z = Mathf.Clamp(p.z, -maxARLocalOffset.z, maxARLocalOffset.z);
        cameraOffset.localPosition = p;
    }
}