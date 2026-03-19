using UnityEngine;

public class MouseClickRaycast : MonoBehaviour
{
    public float maxDistance = 50f;
    public LayerMask interactLayer = ~0;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        Debug.Log($"[MouseClickRaycast] Awake on '{gameObject.name}'. " +
                  $"HasCameraComponent={(cam != null)}. " +
                  $"Camera.main={(Camera.main ? Camera.main.name : "NULL")}");
    }

    void Update()
    {
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[MouseClickRaycast] Update running on '{gameObject.name}'. " +
                      $"enabled={enabled}, activeInHierarchy={gameObject.activeInHierarchy}, " +
                      $"Cursor.lockState={Cursor.lockState}");
        }

        // 🔥 FIX: Use Unity cursor state instead of old IsInCursorMode
        if (Cursor.lockState != CursorLockMode.None)
        {
            if (Input.GetMouseButtonDown(0))
                Debug.Log("[MouseClickRaycast] Click ignored because cursor is not unlocked.");

            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[MouseClickRaycast] Left click detected.");

            if (cam == null)
            {
                Debug.LogError("[MouseClickRaycast] No Camera component found on this GameObject.");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green, 3f);

            if (Physics.Raycast(ray, out hit, maxDistance, interactLayer))
            {
                Debug.Log("[MouseClickRaycast] Hit collider: " + hit.collider.gameObject.name);

                SceneLoader button =
                    hit.collider.GetComponentInParent<SceneLoader>();

                if (button != null)
                {
                    Debug.Log("[MouseClickRaycast] SceneLoader FOUND. Calling LoadScene().");
                    button.LoadScene();
                }
                else
                {
                    Debug.Log("[MouseClickRaycast] SceneLoader NOT found on hit object or parents.");
                }
            }
            else
            {
                Debug.Log("[MouseClickRaycast] Raycast hit nothing.");
            }
        }
    }
}