using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // References to the Player and Crosshair
    public Transform player;
    public Transform crosshair;

    public float followSpeed = 4f;

    private Vector3 offset;

    void Start()
    {
        offset = transform.position - player.position;
    }

    void LateUpdate()
    {
        // Calculate the target position for the camera to move towards
        Vector3 targetPosition = this.PositionBetween(player.position, crosshair.position, 0.4f) + offset;

        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    // Calculate the position between two given positions (between player and crosshair)
    public Vector3 PositionBetween(Vector3 startPosition, Vector3 endPosition, float fraction)
    {
        return Vector3.Lerp(startPosition, endPosition, fraction);
    }
}