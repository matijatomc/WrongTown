using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 20f;
    public Transform crosshair;
    public float rotateSpeed = 8f;
    public LayerMask groundLayerMask;
    public float jumpForce = 10f;

    [Header("Interaction")]
    public float interactionRange = 3f;
    public Transform interactionOrigin;
    public LayerMask interactionLayer;

    private Vector3 moveDirection;
    private Rigidbody rb;
    private Shooting shooting;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        shooting = GetComponent<Shooting>();
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

        // Check for left mouse button click
        if (Input.GetButtonDown("Fire1"))
        {
            shooting.Shoot();
        }

        // Check for interaction input
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void FixedUpdate()
    {
        // Update player position
        rb.AddForce(moveDirection * moveSpeed);

        // Raycast forward to detect ground in front of the player's feet
        if (Physics.Raycast(
            transform.position - transform.up * 0.95f,
            transform.forward,
            1f,
            groundLayerMask))
        {
            // Add upward force to help the player ascend
            rb.AddForce(Vector3.up * moveSpeed * 0.4f);
        }

        // Rotate the player to face the crosshair
        Vector3 lookDirection = crosshair.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                Time.fixedDeltaTime * rotateSpeed
            );
        }
    }

    private void Jump()
    {
        // Raycast down to detect ground below the player
        if (Physics.Raycast(
            transform.position - transform.up * 1f,
            Vector3.down * 0.2f,
            1f,
            groundLayerMask))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void TryInteract()
    {
        if (interactionOrigin == null)
        {
            Debug.LogWarning("Interaction Origin is not assigned!");
            return;
        }

        Ray ray = new Ray(
            interactionOrigin.position,
            interactionOrigin.forward
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionRange,
            interactionLayer))
        {
            MotorPart motorPart = hit.collider.GetComponent<MotorPart>();

            if (motorPart != null)
            {
                motorPart.Collect();
            }
        }
    }

    // Draw the raycasts for visualization
    private void OnDrawGizmos()
    {
        Debug.DrawRay(
            transform.position - transform.up * 0.95f,
            transform.forward * 1f,
            Color.green
        );

        Debug.DrawRay(
            transform.position - transform.up * 0.95f,
            Vector3.down * 0.2f,
            Color.red
        );

        if (interactionOrigin != null)
        {
            Debug.DrawRay(
                interactionOrigin.position,
                interactionOrigin.forward * interactionRange,
                Color.yellow
            );
        }
    }
}