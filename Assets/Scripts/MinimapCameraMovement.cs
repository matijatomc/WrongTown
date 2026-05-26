using UnityEngine;

public class MinimapCameraMovement : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 4f;
    public Vector3 offset = new Vector3(0, 100, 0);

    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, player.position + offset, followSpeed * Time.deltaTime);
    }
}