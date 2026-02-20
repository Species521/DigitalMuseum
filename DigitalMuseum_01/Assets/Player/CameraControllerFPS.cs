using UnityEngine;

public class CameraControllerFPS : MonoBehaviour
{
    public float sensitivity = 2f;
    public Transform playerBody; // Drag the Player object here in the Inspector

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Multiply by sensitivity and Time.deltaTime for frame-rate independence
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        // Vertical rotation (Camera only)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Horizontal rotation (Rotate the whole Player)
        // This ensures your "forward" direction changes as you look around
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}