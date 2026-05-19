using UnityEngine;

public class CrosshairPlacement : MonoBehaviour
{
    // Layer mask for ground objects
    public LayerMask groundLayerMask;

    void Start()
    {
        //Set Cursor to not be visible
        //Cursor.visible = false;
    }

    void Update()
    {
        // Create a ray that starts from the main camera and passes through the mouse position on the screen
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // RaycastHit provides details about where the ray hits an object
        RaycastHit hit;

        // Cast a ray into the scene and check for collisions
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayerMask))
        {
            // If the ray hits something, place the crosshair at the hit point
            transform.position = new Vector3(hit.point.x, hit.point.y + 1.3f, hit.point.z);
        }
    }
}