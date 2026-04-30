using UnityEngine;

public class MouseClickRaycast : MonoBehaviour
{
    public float maxDistance = 50f;
    public LayerMask interactLayer = ~0;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        // Ignore clicks if the cursor is locked (e.g., in first-person mode)
        if (Cursor.lockState != CursorLockMode.None)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxDistance, interactLayer))
            {
                SceneLoader button = hit.collider.GetComponentInParent<SceneLoader>();

                if (button != null)
                {
                    button.LoadScene();
                }
            }
        }
    }
}