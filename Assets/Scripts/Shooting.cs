using UnityEngine;
using System.Collections.Generic;

public class Shooting : MonoBehaviour
{
    public Transform crosshair;
    public float shootRange = 100f;
    public GameObject decalPrefab; // Prefab for the bullet hole decal
    public int maxDecals = 10; // Maximum number of decals allowed

    private List<GameObject> instantiatedDecals = new List<GameObject>();

    public void Shoot()
    {
        // Calculate the direction from the player to the crosshair
        Vector3 shootDirection = crosshair.position - transform.position;

        // Cast a ray in the shoot direction
        RaycastHit hit;
        if (Physics.Raycast(transform.position, shootDirection, out hit, shootRange))
        {
            Debug.Log("Pogodio: " + hit.collider.gameObject.name + " | Tag: " + hit.collider.gameObject.tag);

            if (!hit.collider.gameObject.CompareTag("Character"))
            {
                InstantiateDecal(hit.point, hit.normal, hit.transform);
            }

            ShotController shotController = hit.collider.gameObject.GetComponent<ShotController>();
            Debug.Log("ShotController pronaðen: " + (shotController != null));
            if (shotController != null)
            {
                shotController.Shot();
            }
        }
    }

    void InstantiateDecal(Vector3 position, Vector3 normal, Transform hitTransform)
    {
        // Determine the rotation to align with the surface
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);

        // Instantiate the decal prefab
        GameObject decal = Instantiate(decalPrefab, position, rotation);

        // Adjust the decal's position slightly to prevent z-fighting
        decal.transform.position += normal * 0.01f;

        // Parent the decal to the hit object to keep it in the correct position
        decal.transform.parent = hitTransform;

        // Add the decal to the list of instantiated decals
        instantiatedDecals.Add(decal);

        // Ensure that only the newest decals are kept
        RemoveOldDecals();
    }

    void RemoveOldDecals()
    {
        // If the number of decals exceeds the maximum limit
        if (instantiatedDecals.Count > maxDecals)
        {
            // Calculate the number of excess decals
            int excessDecals = instantiatedDecals.Count - maxDecals;

            // Remove the oldest excess decals from the list and destroy them
            for (int i = 0; i < excessDecals; i++)
            {
                GameObject decal = instantiatedDecals[0]; // Get the oldest decal
                instantiatedDecals.RemoveAt(0); // Remove it from the list
                Destroy(decal); // Destroy the GameObject
            }
        }
    }

    // Draw the raycast for visualization
    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, (crosshair.position - transform.position) * shootRange, Color.red);
    }
}
