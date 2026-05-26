using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 20f;
    public Transform crosshair;
    public float rotateSpeed = 8f;
    public LayerMask groundLayerMask;
    public float jumpForce = 10f;

    private Vector3 moveDirection;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Get input from the player
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Calculate movement direction
        moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // Check for jump input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        // Update player position
        rb.AddForce(moveDirection * moveSpeed);

        // Raycast forward to detect ground in front of the player's feet
        if (Physics.Raycast(transform.position - transform.up * 0.95f, transform.forward, 1f, groundLayerMask))
        {
            // Add upward force to help the player ascend
            rb.AddForce(Vector3.up * moveSpeed * 0.4f);
        }

        // Rotate the player to face the crosshair
        Vector3 lookDirection = crosshair.position - transform.position;
        lookDirection.y = 0f; // Ensure the player doesn't tilt up or down
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotateSpeed);
        }
    }

    private void Jump()
    {
        // Raycast down to detect ground below the player
        if (Physics.Raycast(transform.position - transform.up * 1f, Vector3.down * 0.2f, 1f, groundLayerMask))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    // Draw the raycast for visualization
    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position - transform.up * 0.95f, transform.forward * 1f, Color.green);
        Debug.DrawRay(transform.position - transform.up * 0.95f, Vector3.down * 0.2f, Color.red);
    }
}