using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections.Generic;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[RequireComponent(typeof(CharacterController))]
public class MobileFPSController : MonoBehaviour
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

    // New Helper: Checks if a specific touch is over any UI element
    private bool IsTouchOverUI(Touch touch)
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = touch.screenPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    private void HandleInput()
    {
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
                // FIX: Only allow a "Look" touch if it is NOT on the touchpad AND NOT over any other UI (like the slider)
                else if (!isOnTouchpad && !IsTouchOverUI(touch) && lookTouchId == -1)
                {
                    lookTouchId = touch.touchId;
                }
            }
        }

        if (Touch.activeTouches.Count >= 2)
        {
            HandlePinchZoom();
        }
        else
        {
            foreach (var touch in Touch.activeTouches)
            {
                if (touch.touchId == moveTouchId) ProcessMovement(touch);
                if (touch.touchId == lookTouchId) ProcessLook(touch);
            }
        }

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

        var t0 = Touch.activeTouches[0];
        var t1 = Touch.activeTouches[1];

        if (t0.phase == UnityEngine.InputSystem.TouchPhase.Moved || t1.phase == UnityEngine.InputSystem.TouchPhase.Moved)
        {
            Vector2 prevPos0 = t0.screenPosition - t0.delta;
            Vector2 prevPos1 = t1.screenPosition - t1.delta;

            float prevDistance = Vector2.Distance(prevPos0, prevPos1);
            float currentDistance = Vector2.Distance(t0.screenPosition, t1.screenPosition);
            float deltaDistance = currentDistance - prevDistance;

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

            float yaw = mainCamera != null ? mainCamera.eulerAngles.y : transform.eulerAngles.y;
            Quaternion yawOnly = Quaternion.Euler(0f, yaw, 0f);

            Vector3 move = (yawOnly * Vector3.forward * dir.y) + (yawOnly * Vector3.right * dir.x);
            characterController.Move(move * moveSpeed * strength * Time.deltaTime);
        }
    }

    private void ProcessLook(Touch touch)
    {
        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
        {
            Vector2 delta = touch.delta;

            transform.Rotate(Vector3.up, -delta.x * lookSensitivity);

            cameraPitch += delta.y * lookSensitivity;
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