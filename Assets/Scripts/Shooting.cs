using UnityEngine;
using System.Collections.Generic;

public class Shooting : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Transform firePoint;

    [Header("Shooting")]
    public float shootRange = 100f;
    public float damage = 25f;

    [Header("Decals")]
    public GameObject decalPrefab;
    public int maxDecals = 10;

    private List<GameObject> instantiatedDecals = new List<GameObject>();

    public void Shoot()
    {
        if (mainCamera == null || firePoint == null)
        {
            Debug.LogWarning("Shooting references are missing!");
            return;
        }

        // Ray iz centra ekrana
        Ray cameraRay = mainCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 aimPoint;

        // Prvo odredimo gdje crosshair cilja
        if (Physics.Raycast(
            cameraRay,
            out RaycastHit cameraHit,
            shootRange))
        {
            aimPoint = cameraHit.point;
        }
        else
        {
            aimPoint =
                cameraRay.origin +
                cameraRay.direction * shootRange;
        }

        // Metak ide iz cijevi prema aim pointu
        Vector3 shootDirection =
            (aimPoint - firePoint.position).normalized;

        if (Physics.Raycast(
            firePoint.position,
            shootDirection,
            out RaycastHit hit,
            shootRange))
        {
            Debug.Log(
                "Hit: " +
                hit.collider.gameObject.name
            );

            // Tražimo HealthSystem i na parent objektu
            HealthSystem healthSystem =
                hit.collider.GetComponentInParent<HealthSystem>();

            if (healthSystem != null &&
                hit.collider.GetComponentInParent<EnemyController>() != null)
            {
                healthSystem.TakeDamage(damage);
            }
            else
            {
                InstantiateDecal(
                    hit.point,
                    hit.normal,
                    hit.transform
                );
            }
        }
    }

    private void InstantiateDecal(
        Vector3 position,
        Vector3 normal,
        Transform hitTransform)
    {
        if (decalPrefab == null)
            return;

        Quaternion rotation =
            Quaternion.FromToRotation(
                Vector3.up,
                normal
            );

        GameObject decal =
            Instantiate(
                decalPrefab,
                position,
                rotation
            );

        decal.transform.position +=
            normal * 0.01f;

        decal.transform.parent =
            hitTransform;

        instantiatedDecals.Add(decal);

        RemoveOldDecals();
    }

    private void RemoveOldDecals()
    {
        if (instantiatedDecals.Count <= maxDecals)
            return;

        int excessDecals =
            instantiatedDecals.Count - maxDecals;

        for (int i = 0; i < excessDecals; i++)
        {
            GameObject decal =
                instantiatedDecals[0];

            instantiatedDecals.RemoveAt(0);

            Destroy(decal);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Debug.DrawRay(
                firePoint.position,
                firePoint.forward * 3f,
                Color.red
            );
        }
    }
}