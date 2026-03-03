using UnityEngine;
using UnityEngine.EventSystems;

public class CameraControllerFPS : MonoBehaviour
{
    public float sensitivity = 100f;
    public Transform playerBody;

    private float xRotation = 0f;

    public static bool IsInCursorMode { get; private set; }

    void Start()
    {
        // Start in FPS mode (cursor locked)
        IsInCursorMode = false;
        LockCursor();

#if UNITY_WEBGL
        // Prevent browser right-click menu
        Application.ExternalEval(
            "document.addEventListener('contextmenu', function(e){ e.preventDefault(); });"
        );
#endif
    }

    void Update()
    {
        HandleCursorMode();

        // Stop camera movement if in cursor mode
        if (IsInCursorMode)
            return;

        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }

    void HandleCursorMode()
    {
        // HOLD Right Mouse Button = Cursor Mode
        bool shouldBeInCursorMode = Input.GetMouseButton(1);

        // Only change state if necessary
        if (shouldBeInCursorMode != IsInCursorMode)
        {
            IsInCursorMode = shouldBeInCursorMode;

            if (IsInCursorMode)
                UnlockCursor();
            else
                LockCursor();
        }
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}