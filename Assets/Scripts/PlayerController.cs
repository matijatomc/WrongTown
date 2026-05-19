using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Speed at which the player moves
    public float moveSpeed = 5f;

    // Update is called once per frame
    void Update()
    {
        // Get input from the player
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Calculate the movement direction based on input
        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // Move the player based on the input direction
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }
}