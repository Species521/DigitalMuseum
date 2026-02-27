using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SpaceFlyCameraController : MonoBehaviour
{
    [Header("References")]
    public Transform painting;
    public float maxDistanceFromPainting = 20f;

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float sprintMultiplier = 2f;

    [Header("Pan")]
    public float panSpeed = 0.5f;   // 👈 Public pan speed

    [Header("Look")]
    public float mouseSensitivity = 200f;
    public float verticalLookLimit = 85f;

    private CharacterController controller;
    private float verticalRotation = 0f;
    private bool cursorUnlocked = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        LockCursor();
    }

    void Update()
    {
        HandleCursorToggle();

        bool isPanning = Input.GetMouseButton(2);

        if (!cursorUnlocked)
        {
            // Only allow look if NOT panning
            if (!isPanning)
                HandleMouseLook();

            HandleMovement();
        }

        // Pan works independently of cursor state
        HandleMiddleMousePan();
    }

    // ---------------- CURSOR ----------------

    void HandleCursorToggle()
    {
        if (Input.GetMouseButtonDown(1))
            UnlockCursor();

        if (Input.GetMouseButtonUp(1))
            LockCursor();
    }

    void LockCursor()
    {
        cursorUnlocked = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        cursorUnlocked = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ---------------- LOOK ----------------

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalLookLimit, verticalLookLimit);

        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        if (transform.parent != null)
            transform.parent.Rotate(Vector3.up * mouseX);
        else
            transform.Rotate(Vector3.up * mouseX, Space.World);
    }

    // ---------------- MOVEMENT ----------------

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float moveY = 0f;
        if (Input.GetKey(KeyCode.E)) moveY += 1f;
        if (Input.GetKey(KeyCode.Q)) moveY -= 1f;

        Vector3 moveDirection =
            transform.right * moveX +
            transform.forward * moveZ +
            transform.up * moveY;

        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();

        float speed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
            speed *= sprintMultiplier;

        Vector3 desiredMove = moveDirection * speed * Time.deltaTime;

        Vector3 currentPosition = transform.parent
            ? transform.parent.position
            : transform.position;

        Vector3 nextPosition = currentPosition + desiredMove;

        float nextDistance =
            Vector3.Distance(nextPosition, painting.position);

        if (nextDistance > maxDistanceFromPainting)
        {
            Vector3 toCenter =
                (painting.position - currentPosition).normalized;

            desiredMove = Vector3.Project(desiredMove, toCenter);
        }

        controller.Move(desiredMove);
    }

    // ---------------- MIDDLE MOUSE PAN ----------------

    void HandleMiddleMousePan()
    {
        if (Input.GetMouseButton(2)) // Hold middle mouse
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // World-axis only pan (absolute X and Y)
            Vector3 pan =
                (Vector3.right * mouseX +
                 Vector3.up * mouseY) * panSpeed;

            controller.Move(pan);
        }
    }
}