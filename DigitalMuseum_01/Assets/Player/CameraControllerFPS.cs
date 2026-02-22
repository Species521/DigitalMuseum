using UnityEngine;

public class CameraControllerFPS : MonoBehaviour
{
    public float sensitivity = 100f;
    public Transform playerBody;
    private float xRotation = 0f;

    void Start()
    {
        // Initial lock attempt
        LockCursor();
    }

    void Update()
    {
        // 1. Re-lock if the user clicks the screen (Crucial for WebGL)
        if (Input.GetMouseButtonDown(0))
        {
            LockCursor();
        }

        // Only rotate if the cursor is actually locked
        if (Cursor.lockState == CursorLockMode.Locked)
        {
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
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}