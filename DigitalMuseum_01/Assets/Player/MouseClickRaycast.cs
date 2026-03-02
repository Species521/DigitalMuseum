using UnityEngine;

public class MouseClickRaycast : MonoBehaviour
{
    public float maxDistance = 50f;
    public LayerMask interactLayer = ~0;

    private Camera cam;

    // 1) Put this at the top level of the class (like shown here)
    //    Awake runs once when the object becomes active.
    private void Awake()
    {
        // 2) This ensures we raycast from THIS camera, not whatever happens to be tagged MainCamera.
        cam = GetComponent<Camera>();

        Debug.Log($"[MouseClickRaycast] Awake on '{gameObject.name}'. " +
                  $"HasCameraComponent={(cam != null)}. " +
                  $"Camera.main={(Camera.main ? Camera.main.name : "NULL")}");
    }

    void Update()
    {
        // 3) Put this at the VERY TOP of Update() to prove Update() is actually running.
        if (Time.frameCount % 60 == 0) // log about once per second
        {
            Debug.Log($"[MouseClickRaycast] Update running on '{gameObject.name}'. " +
                      $"enabled={enabled}, activeInHierarchy={gameObject.activeInHierarchy}, " +
                      $"IsInCursorMode={CameraControllerFPS.IsInCursorMode}");
        }

        // 4) Keep your guard, but log when it blocks interaction.
        if (!CameraControllerFPS.IsInCursorMode)
        {
            if (Input.GetMouseButtonDown(0))
                Debug.Log("[MouseClickRaycast] Click ignored because IsInCursorMode is FALSE.");
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

                // 5) Use InParent so it works whether the script is on the hit object or its parent.
                SceneLoader button = hit.collider.GetComponent<SceneLoader>();

                if (button != null)
                {
                    Debug.Log("[MouseClickRaycast] museum_switch_script FOUND. Calling LoadScene().");
                    button.LoadScene();
                }
                else
                {
                    Debug.Log("[MouseClickRaycast] museum_switch_script NOT found on hit object or parents.");
                }
            }
            else
            {
                Debug.Log("[MouseClickRaycast] Raycast hit nothing.");
            }
        }
    }
}