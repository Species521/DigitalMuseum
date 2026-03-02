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
        // Start in FPS (locked cursor) mode
        IsInCursorMode = false;
        LockCursor();
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
        // Toggle cursor mode when RMB is pressed (not held)
        if (Input.GetMouseButtonDown(1))
        {
            IsInCursorMode = !IsInCursorMode;

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