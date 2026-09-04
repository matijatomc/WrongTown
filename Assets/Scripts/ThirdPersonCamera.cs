using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Camera Position")]
    public float distance = 5f;
    public float height = 2f;
    public float shoulderOffset = 1f;

    [Header("Camera Rotation")]
    public float mouseSensitivity = 3f;
    public float minPitch = -30f;
    public float maxPitch = 60f;

    [Header("Camera Collision")]
    public LayerMask collisionMask;
    public float collisionRadius = 0.25f;
    public float collisionPadding = 0.15f;
    public float collisionSmoothSpeed = 12f;

    private float yaw;
    private float pitch;

    private float currentDistance;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 angles = transform.eulerAngles;

        yaw = angles.y;
        pitch = angles.x;

        currentDistance = distance;
    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0f)
            return;

        if (player == null)
            return;

        HandleRotation();
        HandlePosition();
    }

    private void HandleRotation()
    {
        float mouseX =
            Input.GetAxis("Mouse X") *
            mouseSensitivity;

        float mouseY =
            Input.GetAxis("Mouse Y") *
            mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(
            pitch,
            minPitch,
            maxPitch
        );
    }

    private void HandlePosition()
    {
        Quaternion rotation =
            Quaternion.Euler(
                pitch,
                yaw,
                0f
            );

        Vector3 pivotPosition =
            player.position +
            Vector3.up * height;

        Vector3 fullCameraOffset =
            rotation *
            new Vector3(
                shoulderOffset,
                0f,
                -distance
            );

        Vector3 desiredPosition =
            pivotPosition +
            fullCameraOffset;

        Vector3 direction =
            desiredPosition -
            pivotPosition;

        float desiredDistance =
            direction.magnitude;

        direction.Normalize();

        float targetDistance =
            desiredDistance;

        RaycastHit hit;

        if (Physics.SphereCast(
            pivotPosition,
            collisionRadius,
            direction,
            out hit,
            desiredDistance,
            collisionMask,
            QueryTriggerInteraction.Ignore))
        {
            targetDistance =
                Mathf.Max(
                    hit.distance -
                    collisionPadding,
                    0.3f
                );
        }

        currentDistance =
            Mathf.Lerp(
                currentDistance,
                targetDistance,
                Time.deltaTime *
                collisionSmoothSpeed
            );

        transform.position =
            pivotPosition +
            direction *
            currentDistance;

        transform.rotation =
            rotation;
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null)
            return;

        Vector3 pivotPosition =
            player.position +
            Vector3.up * height;

        Gizmos.color =
            Color.cyan;

        Gizmos.DrawWireSphere(
            pivotPosition,
            collisionRadius
        );
    }
}