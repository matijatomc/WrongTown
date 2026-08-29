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

    private float yaw;
    private float pitch;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0f)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(
            pitch,
            yaw,
            0f
        );

        Vector3 pivotPosition =
            player.position +
            Vector3.up * height;

        Vector3 cameraOffset =
            rotation * new Vector3(
                shoulderOffset,
                0f,
                -distance
            );

        transform.position = pivotPosition + cameraOffset;

        transform.rotation = rotation;
    }
}