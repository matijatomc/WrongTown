using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shooting : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Transform firePoint;

    [Header("Shooting")]
    public float shootRange = 100f;
    public float damage = 25f;

    [Header("Feedback")]
    public HitMarkerUI hitMarkerUI;

    [Header("Timing")]
    public float shootDelay = 0.5f;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioSource audioSource;
    [Range(0f, 1f)]
    public float shootVolume = 1f;

    [Header("Decals")]
    public GameObject decalPrefab;
    public int maxDecals = 10;

    private List<GameObject> instantiatedDecals = new List<GameObject>();
    private Coroutine shootRoutine;

    /// <summary>
    /// Poziva se na klik. Ne puca odmah - pokreće odgodu.
    /// </summary>
    public void Shoot()
    {
        if (shootRoutine != null)
        {
            StopCoroutine(shootRoutine);
        }

        shootRoutine = StartCoroutine(ShootAfterDelay());
    }

    private IEnumerator ShootAfterDelay()
    {
        yield return new WaitForSeconds(shootDelay);

        FireShot();

        shootRoutine = null;
    }

    /// <summary>
    /// Stvarni hitscan. Pozovi ovo direktno iz Animation Eventa
    /// ako želiš da bude vezano uz frame animacije umjesto uz timer.
    /// </summary>
    public void FireShot()
    {
        if (mainCamera == null || firePoint == null)
        {
            Debug.LogWarning("Shooting references are missing!");
            return;
        }

        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound, shootVolume);
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

            EnemyController enemy =
                hit.collider.GetComponentInParent<EnemyController>();

            if (healthSystem != null && enemy != null)
            {
                healthSystem.TakeDamage(damage);

                if (hitMarkerUI != null)
                {
                    hitMarkerUI.ShowHit();
                }
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

            // Decal je mogao biti uništen zajedno s parentom
            // (npr. enemy je umro), pa provjeravamo.
            if (decal != null)
            {
                Destroy(decal);
            }
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
