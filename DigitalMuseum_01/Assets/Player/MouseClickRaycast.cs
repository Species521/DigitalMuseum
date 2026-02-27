using UnityEngine;
using Unity.VisualScripting;

public class MouseClickRaycast : MonoBehaviour
{
    public float maxDistance = 50f;
    public LayerMask interactLayer = ~0;

    void Update()
    {
        // Must hold RMB to be in "Interaction Mode"
        if (!CameraControllerFPS.IsInCursorMode)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            // Use the current mouse position on the screen
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // VISUAL CHECK: Switch to 'Scene' view while playing. 
            // You will see a green line showing exactly where Unity thinks you clicked.
            Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green, 3f);

            if (Physics.Raycast(ray, out hit, maxDistance, interactLayer))
            {
                Debug.Log("Hit: " + hit.collider.gameObject.name);
                examroom_switch_script button = hit.collider.GetComponent<examroom_switch_script>();

                if (button != null)
                {
                    button.LoadScene();
                }
            }
            else
            {
                // This will tell us if the ray missed or if the script isn't firing
                Debug.Log("Left Click registered, but the Raycast hit nothing at: " + Input.mousePosition);
            }
        }
    }
}