using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 20f;
    public Transform mainCamera;
    public float rotateSpeed = 8f;
    public LayerMask groundLayerMask;
    public float jumpForce = 10f;

    [Header("Interaction")]
    public float interactionRange = 3f;
    public LayerMask interactionLayer;
    public TextMeshProUGUI interactionText;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioSource audioSource;
    [Range(0f, 1f)]
    public float shootVolume = 1f;

    [Header("Shooting")]
    public float shootLockDuration = 0.5f;
    public bool useAnimationLength = false;

    private MotorPart currentMotorPart;
    private Motorcycle currentMotorcycle;

    private Vector3 moveDirection;
    private Rigidbody rb;
    private Shooting shooting;
    private Animator animator;

    private bool wasMoving = false;
    private bool inputLocked = false;
    private Coroutine shootLockRoutine;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        shooting = GetComponent<Shooting>();
        animator = GetComponentInChildren<Animator>();

        if (mainCamera == null && Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (inputLocked)
        {
            // Nuliramo smjer da FixedUpdate ne dodaje silu.
            moveDirection = Vector3.zero;

            // Prompt i dalje osvježavamo da tekst ne ostane "zamrznut".
            UpdateInteractionPrompt();
            return;
        }

        HandleMovementInput();
        HandleAnimations();
        HandleJump();
        HandleShooting();
        HandleInteraction();

        UpdateInteractionPrompt();
    }

    private void FixedUpdate()
    {
        MovePlayer();
        AssistWithSlopes();
        RotatePlayerToCamera();
    }

    private void HandleMovementInput()
    {
        if (mainCamera == null)
            return;

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 cameraForward = mainCamera.forward;
        Vector3 cameraRight = mainCamera.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        moveDirection =
            (cameraForward * verticalInput +
             cameraRight * horizontalInput).normalized;
    }

    private void HandleAnimations()
    {
        if (animator == null)
            return;

        bool isMoving = moveDirection.sqrMagnitude > 0.01f;

        if (isMoving && !wasMoving)
        {
            animator.SetTrigger("IsWalking");
        }
        else if (!isMoving && wasMoving)
        {
            animator.SetTrigger("IsStanding");
        }

        wasMoving = isMoving;
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    private void HandleShooting()
    {
        if (!Input.GetButtonDown("Fire1"))
            return;

        if (shooting != null)
        {
            shooting.Shoot();
        }

        if (animator != null)
        {
            animator.SetTrigger("IsShooting");
        }

        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(
                shootSound,
                shootVolume
            );
        }

        StartShootLock();
    }

    private void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    // ---------- INPUT LOCK ----------

    private void StartShootLock()
    {
        if (shootLockRoutine != null)
        {
            StopCoroutine(shootLockRoutine);
        }

        shootLockRoutine = StartCoroutine(ShootLockRoutine());
    }

    private IEnumerator ShootLockRoutine()
    {
        inputLocked = true;

        float duration = shootLockDuration;

        if (useAnimationLength && animator != null)
        {
            // Animator treba jedan frame da uđe u novi state.
            yield return null;

            duration =
                animator.GetCurrentAnimatorStateInfo(0).length;
        }

        yield return new WaitForSeconds(duration);

        UnlockInput();
    }

    /// <summary>
    /// Pozovi ovo iz Animation Eventa na zadnjem frameu shoot klipa
    /// ako želiš da lock završi točno s animacijom.
    /// Napomena: animator je na child objektu, pa event traži metodu
    /// na tom childu - treba ti mali relay skript (vidi dolje u komentaru).
    /// </summary>
    public void OnShootAnimationEnd()
    {
        if (shootLockRoutine != null)
        {
            StopCoroutine(shootLockRoutine);
            shootLockRoutine = null;
        }

        UnlockInput();
    }

    private void UnlockInput()
    {
        inputLocked = false;
        shootLockRoutine = null;

        // Reset da se walk/stand trigger ispravno ponovno okine.
        wasMoving = false;
    }

    // --------------------------------

    private void MovePlayer()
    {
        if (rb == null)
            return;

        if (inputLocked)
        {
            // Zaustavi klizanje, ali pusti gravitaciju da radi.
            // Unity 6: koristi rb.linearVelocity umjesto rb.velocity.
            Vector3 velocity = rb.velocity;
            rb.velocity = new Vector3(0f, velocity.y, 0f);
            return;
        }

        rb.AddForce(
            moveDirection * moveSpeed
        );
    }

    private void AssistWithSlopes()
    {
        if (rb == null)
            return;

        if (inputLocked)
            return;

        if (Physics.Raycast(
            transform.position - transform.up * 0.95f,
            transform.forward,
            1f,
            groundLayerMask))
        {
            rb.AddForce(
                Vector3.up * moveSpeed * 0.4f
            );
        }
    }

    private void RotatePlayerToCamera()
    {
        if (mainCamera == null)
            return;

        // Ako želiš da se igrač NE okreće dok puca,
        // odkomentiraj sljedeće dvije linije:
        // if (inputLocked)
        //     return;

        Vector3 lookDirection =
            mainCamera.forward;

        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(
                lookDirection
            );

        transform.rotation =
            Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                Time.fixedDeltaTime * rotateSpeed
            );
    }

    private void Jump()
    {
        if (rb == null)
            return;

        if (Physics.Raycast(
            transform.position - transform.up * 1f,
            Vector3.down,
            1.2f,
            groundLayerMask))
        {
            rb.AddForce(
                Vector3.up * jumpForce,
                ForceMode.Impulse
            );
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

        Collider[] nearbyColliders =
            Physics.OverlapSphere(
                transform.position,
                interactionRange,
                interactionLayer
            );

        MotorPart closestPart = null;
        Motorcycle closestMotorcycle = null;

        float closestDistance =
            Mathf.Infinity;

        foreach (Collider col in nearbyColliders)
        {
            MotorPart motorPart =
                col.GetComponent<MotorPart>();

            if (motorPart == null)
            {
                motorPart =
                    col.GetComponentInParent<MotorPart>();
            }

            if (motorPart != null)
            {
                float distance =
                    Vector3.Distance(
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

            Motorcycle motorcycle =
                col.GetComponent<Motorcycle>();

            if (motorcycle == null)
            {
                motorcycle =
                    col.GetComponentInParent<Motorcycle>();
            }

            if (motorcycle != null)
            {
                float distance =
                    Vector3.Distance(
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
                "[E] Pick up " +
                currentMotorPart.partName;

            interactionText.gameObject.SetActive(true);
        }
        else if (currentMotorcycle != null)
        {
            interactionText.text =
                "[E] Ride motorcycle";

            interactionText.gameObject.SetActive(true);
        }
        else
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        Debug.DrawRay(
            transform.position -
            transform.up * 0.95f,
            transform.forward,
            Color.green
        );

        Debug.DrawRay(
            transform.position -
            transform.up * 0.95f,
            Vector3.down * 1.2f,
            Color.red
        );

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            interactionRange
        );
    }
}
