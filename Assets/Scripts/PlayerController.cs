using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 20f;
    public Transform crosshair;
    public Transform mainCamera;
    public float rotateSpeed = 8f;
    public LayerMask groundLayerMask;
    public float jumpForce = 10f;

    [Header("Interaction")]
    public float interactionRange = 3f;
    public LayerMask interactionLayer;
    public TextMeshProUGUI interactionText;

    private MotorPart currentMotorPart;
    private Motorcycle currentMotorcycle;

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
        Vector3 cameraForward = mainCamera.forward;
        Vector3 cameraRight = mainCamera.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        moveDirection = (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;

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

        UpdateInteractionPrompt();
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
        if (currentMotorPart != null)
        {
            currentMotorPart.Collect();
            return;
        }

        if (currentMotorcycle != null)
        {
            currentMotorcycle.Ride();
        }
    }

    private void UpdateInteractionPrompt()
    {
        if (interactionText == null)
            return;

        Collider[] nearbyColliders = Physics.OverlapSphere(
            transform.position,
            interactionRange,
            interactionLayer
        );

        MotorPart closestPart = null;
        Motorcycle closestMotorcycle = null;

        float closestDistance = Mathf.Infinity;

        foreach (Collider col in nearbyColliders)
        {
            MotorPart motorPart = col.GetComponent<MotorPart>();

            if (motorPart == null)
            {
                motorPart = col.GetComponentInParent<MotorPart>();
            }

            if (motorPart != null)
            {
                float distance = Vector3.Distance(
                    transform.position,
                    motorPart.transform.position
                );

                if (distance < closestDistance)
                {
                    closestDistance = distance;

                    closestPart = motorPart;
                    closestMotorcycle = null;
                }

                continue;
            }

            Motorcycle motorcycle = col.GetComponent<Motorcycle>();

            if (motorcycle == null)
            {
                motorcycle = col.GetComponentInParent<Motorcycle>();
            }

            if (motorcycle != null)
            {
                float distance = Vector3.Distance(
                    transform.position,
                    motorcycle.transform.position
                );

                if (distance < closestDistance)
                {
                    closestDistance = distance;

                    closestMotorcycle = motorcycle;
                    closestPart = null;
                }
            }
        }

        currentMotorPart = closestPart;
        currentMotorcycle = closestMotorcycle;

        if (currentMotorPart != null)
        {
            interactionText.text =
                "[E] Pick up " + currentMotorPart.partName;

            interactionText.gameObject.SetActive(true);
        }
        else if (currentMotorcycle != null)
        {
            interactionText.text = "[E] Ride motorcycle";
            interactionText.gameObject.SetActive(true);
        }
        else
        {
            interactionText.gameObject.SetActive(false);
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}