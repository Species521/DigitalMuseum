using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ExamRoom_cameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float sprintMultiplier = 2f;

    [Header("Look")]
    public float mouseSensitivity = 200f;
    public float verticalLookLimit = 85f;

    [Header("Middle Mouse Pan")]
    public float panSpeed = 0.5f;

    private CharacterController controller;
    private Transform player;
    private float verticalRotation;

    void Start()
    {
        controller = GetComponentInParent<CharacterController>();
        player = transform.parent;

        LockCursor();
    }

    void Update()
    {
        HandleCursorMode();

        // If cursor is unlocked → no camera control
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        // Middle mouse = WORLD PAN ONLY
        if (Input.GetMouseButton(2))
        {
            HandleWorldPan();
            return;
        }

        HandleLook();
        HandleMovement();
    }

    // ---------------- CURSOR MODE ----------------

    void HandleCursorMode()
    {
        if (Input.GetMouseButton(1))
            UnlockCursor();
        else
            LockCursor();
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

    // ---------------- LOOK ----------------

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // YAW → rotate PLAYER
        if (player != null)
            player.Rotate(Vector3.up * mouseX, Space.World);

        // PITCH → rotate CAMERA
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalLookLimit, verticalLookLimit);

        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    // ---------------- MOVEMENT ----------------

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float moveY = 0f;
        if (Input.GetKey(KeyCode.E)) moveY += 1f;
        if (Input.GetKey(KeyCode.Q)) moveY -= 1f;

        Vector3 move =
            transform.right * moveX +
            transform.forward * moveZ +
            transform.up * moveY;

        if (move.magnitude > 1f)
            move.Normalize();

        float speed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
            speed *= sprintMultiplier;

        controller.Move(move * speed * Time.deltaTime);
    }

    // ---------------- WORLD PAN ----------------

    void HandleWorldPan()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Vector3 pan =
            (Vector3.right * mouseX +
             Vector3.up * mouseY) * panSpeed;

        controller.Move(pan);
    }
}