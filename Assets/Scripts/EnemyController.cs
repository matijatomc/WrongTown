using System.Collections;
using UnityEngine;

[RequireComponent(typeof(HealthSystem))]
public class EnemyController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public LayerMask obstructionMask;

    [Header("Detection")]
    public float detectionRange = 15f;
    public float shootRange = 10f;
    private Animator animator;

    [Header("Movement")]
    public float moveSpeed = 4f;

    [Header("Shooting")]
    public float fireRate = 1.2f;
    public float damage = 10f;
    public float shootRayHeight = 1f;
    public string shootTrigger = "Shoot";
    public float shootDelay = 0.35f;

    public bool useAnimationEvent = false;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioSource audioSource;
    [Range(0f, 1f)] public float shootVolume = 1f;

    [Header("Death")]
    public bool disableCollidersOnDeath = true;

    private Rigidbody rb;
    private HealthSystem health;
    private float fireCooldown;
    private bool isFiring = false;

    private enum State { Idle, Chasing, Shooting, Dead }
    private State currentState = State.Idle;
    private State previousState = State.Idle;

    private bool isDead = false;

    void Awake()
    {
        health = GetComponent<HealthSystem>();

        if (health != null)
            health.OnDeath += HandleDeath;
    }

    void OnDestroy()
    {
        if (health != null)
            health.OnDeath -= HandleDeath;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = distanceToPlayer <= detectionRange && HasLineOfSight();

        if (!canSeePlayer)
            currentState = State.Idle;
        else if (distanceToPlayer > shootRange)
            currentState = State.Chasing;
        else
            currentState = State.Shooting;

        // SpottedPlayer okidamo SAMO na promjeni stanja, ne svaki frame.
        if (currentState != previousState)
        {
            if (currentState != State.Idle && animator != null)
                animator.SetTrigger("SpottedPlayer");

            previousState = currentState;
        }

        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;

        if (currentState == State.Shooting)
        {
            FaceTarget();

            if (fireCooldown <= 0f && !isFiring)
            {
                fireCooldown = fireRate;
                StartCoroutine(ShootRoutine());
            }
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (currentState == State.Chasing && rb != null)
        {
            Vector3 direction = (player.position - transform.position);
            direction.y = 0f;
            direction.Normalize();

            Vector3 targetPos = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPos);

            FaceTarget();
        }
    }

    // ---------- SHOOTING ----------

        private IEnumerator ShootRoutine()
    {
        isFiring = true;

        if (animator != null && !string.IsNullOrEmpty(shootTrigger))
            animator.SetTrigger(shootTrigger);

        // Zvuk krece u istom frameu kao i animacija.
        if (audioSource != null && shootSound != null)
            audioSource.PlayOneShot(shootSound, shootVolume);

        if (!useAnimationEvent)
        {
            yield return new WaitForSeconds(shootDelay);

            if (!isDead)
                FireShot();
        }

        isFiring = false;
    }

    private void FireShot()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > shootRange || !HasLineOfSight())
            return;

        Vector3 origin = transform.position + Vector3.up * shootRayHeight;
        Vector3 targetPoint = player.position + Vector3.up * shootRayHeight;
        Vector3 dir = (targetPoint - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, shootRange))
        {
            HealthSystem targetHealth = hit.collider.GetComponent<HealthSystem>();

            if (targetHealth != null)
                targetHealth.TakeDamage(damage);
        }
    }

    // ---------- DEATH ----------

    private void HandleDeath()
    {
        if (isDead) return;

        isDead = true;
        currentState = State.Dead;

        // Prekini hitac koji je mozda u tijeku.
        StopAllCoroutines();
        isFiring = false;

        if (animator != null)
        {
            animator.ResetTrigger("SpottedPlayer");

            if (!string.IsNullOrEmpty(shootTrigger))
                animator.ResetTrigger(shootTrigger);

            animator.SetTrigger("Dead");
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (disableCollidersOnDeath)
        {
            foreach (Collider col in GetComponentsInChildren<Collider>())
                col.enabled = false;
        }

        enabled = false;
    }

    // ---------------------------

    private bool HasLineOfSight()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * shootRayHeight;
        Vector3 targetPoint = player.position + Vector3.up * shootRayHeight;
        Vector3 dir = targetPoint - origin;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dir.magnitude, obstructionMask))
            return false;

        return true;
    }

    private void FaceTarget()
    {
        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}
