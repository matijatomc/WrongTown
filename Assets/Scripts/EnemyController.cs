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

    [Header("Movement")]
    public float moveSpeed = 4f;

    [Header("Patrol")]
    public bool patrolWhenIdle = true;
    public float patrolRadius = 6f;
    public float patrolSpeed = 1.5f;
    public float patrolWaitTime = 2f;
    public float patrolPointTolerance = 0.4f;
    public int patrolPointAttempts = 8;

    [Header("Animation")]
    public string walkingBool = "IsWalking";
    public string spottedTrigger = "SpottedPlayer";
    public string shootTrigger = "Shoot";
    public string deadTrigger = "Dead";

    [Header("Shooting")]
    public float fireRate = 1.2f;
    public float damage = 10f;
    public float shootRayHeight = 1f;
    public float shootDelay = 0.35f;
    public bool useAnimationEvent = false;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioSource audioSource;

    [Range(0f, 1f)]
    public float shootVolume = 1f;

    [Header("Death")]
    public bool disableCollidersOnDeath = true;

    private Rigidbody rb;
    private HealthSystem health;
    private Animator animator;

    private float fireCooldown;
    private bool isFiring = false;
    private bool isDead = false;

    private Vector3 patrolOrigin;
    private Vector3 patrolTarget;

    private bool hasPatrolTarget = false;
    private float patrolWaitTimer = 0f;

    private enum State
    {
        Idle,
        Chasing,
        Shooting,
        Dead
    }

    private State currentState = State.Idle;
    private State previousState = State.Idle;

    private void Awake()
    {
        health = GetComponent<HealthSystem>();

        if (health != null)
        {
            health.OnDeath += HandleDeath;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeath -= HandleDeath;
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        patrolOrigin = transform.position;

        if (patrolWhenIdle)
        {
            ChooseNewPatrolPoint();
        }

        UpdateWalkingAnimation();
    }

    private void Update()
    {
        if (isDead)
            return;

        if (player == null)
            return;

        float distanceToPlayer =
            Vector3.Distance(
                transform.position,
                player.position
            );

        bool canSeePlayer =
            distanceToPlayer <= detectionRange &&
            HasLineOfSight();

        if (!canSeePlayer)
        {
            currentState = State.Idle;
        }
        else if (distanceToPlayer > shootRange)
        {
            currentState = State.Chasing;
            animator.SetBool(walkingBool,true);
        }
        else
        {
            animator.SetBool(walkingBool,false);
            currentState = State.Shooting;
        }

        HandleStateChange();
        HandlePatrolTimer();
        UpdateWalkingAnimation();

        if (fireCooldown > 0f)
        {
            fireCooldown -= Time.deltaTime;
        }

        if (currentState == State.Shooting)
        {
            FaceTarget();

            if (fireCooldown <= 0f &&
                !isFiring)
            {
                fireCooldown = fireRate;

                StartCoroutine(
                    ShootRoutine()
                );
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;

        if (rb == null)
            return;

        if (currentState == State.Idle)
        {
            Patrol();
        }
        else if (currentState == State.Chasing)
        {
            ChasePlayer();
        }
    }

    // ---------- STATE ----------

    private void HandleStateChange()
    {
        if (currentState == previousState)
            return;

        if (currentState == State.Idle)
        {
            patrolOrigin = transform.position;
            patrolWaitTimer = 0f;
            hasPatrolTarget = false;

            if (patrolWhenIdle)
            {
                ChooseNewPatrolPoint();
            }
        }
        else
        {
            hasPatrolTarget = false;

            if (animator != null &&
                !string.IsNullOrEmpty(spottedTrigger))
            {
                animator.SetTrigger(
                    spottedTrigger
                );
            }
        }

        previousState = currentState;
    }

    // ---------- WALKING ANIMATION ----------

    private void UpdateWalkingAnimation()
    {
        if (animator == null)
            return;

        if (string.IsNullOrEmpty(walkingBool))
            return;

        bool shouldWalk = false;

        if (currentState == State.Chasing)
        {
            shouldWalk = true;
        }
        else if (
            currentState == State.Idle &&
            patrolWhenIdle &&
            hasPatrolTarget)
        {
            shouldWalk = true;
        }

        animator.SetBool(
            walkingBool,
            shouldWalk
        );
    }

    // ---------- PATROL ----------

    private void HandlePatrolTimer()
    {
        if (!patrolWhenIdle)
            return;

        if (currentState != State.Idle)
            return;

        if (hasPatrolTarget)
            return;

        if (patrolWaitTimer > 0f)
        {
            patrolWaitTimer -=
                Time.deltaTime;

            return;
        }

        ChooseNewPatrolPoint();
    }

    private void Patrol()
    {
        if (!patrolWhenIdle)
            return;

        if (!hasPatrolTarget)
            return;

        Vector3 direction =
            patrolTarget -
            rb.position;

        direction.y = 0f;

        float distance =
            direction.magnitude;

        if (distance <=
            patrolPointTolerance)
        {
            hasPatrolTarget = false;

            patrolWaitTimer =
                patrolWaitTime;

            return;
        }

        direction.Normalize();

        Vector3 targetPosition =
            rb.position +
            direction *
            patrolSpeed *
            Time.fixedDeltaTime;

        rb.MovePosition(
            targetPosition
        );

        FaceDirection(direction);
    }

    private void ChooseNewPatrolPoint()
    {
        for (int i = 0;
             i < patrolPointAttempts;
             i++)
        {
            Vector2 randomPoint =
                Random.insideUnitCircle *
                patrolRadius;

            Vector3 candidate =
                patrolOrigin +
                new Vector3(
                    randomPoint.x,
                    0f,
                    randomPoint.y
                );

            Vector3 rayStart =
                transform.position +
                Vector3.up *
                shootRayHeight;

            Vector3 rayEnd =
                candidate +
                Vector3.up *
                shootRayHeight;

            bool blocked =
                Physics.Linecast(
                    rayStart,
                    rayEnd,
                    obstructionMask,
                    QueryTriggerInteraction.Ignore
                );

            if (!blocked)
            {
                patrolTarget =
                    candidate;

                hasPatrolTarget =
                    true;

                return;
            }
        }

        hasPatrolTarget = false;
        patrolWaitTimer = 1f;
    }

    // ---------- CHASE ----------

    private void ChasePlayer()
    {
        animator.SetBool(walkingBool,true);
        if (player == null)
            return;

        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <
            0.01f)
        {
            return;
        }

        direction.Normalize();

        Vector3 targetPosition =
            rb.position +
            direction *
            moveSpeed *
            Time.fixedDeltaTime;

        rb.MovePosition(
            targetPosition
        );

        FaceDirection(direction);
    }

    // ---------- SHOOTING ----------

    private IEnumerator ShootRoutine()
    {
        isFiring = true;

        if (animator != null &&
            !string.IsNullOrEmpty(shootTrigger))
        {
            animator.SetTrigger(
                shootTrigger
            );
        }

        if (audioSource != null &&
            shootSound != null)
        {
            audioSource.PlayOneShot(
                shootSound,
                shootVolume
            );
        }

        if (!useAnimationEvent)
        {
            yield return new WaitForSeconds(
                shootDelay
            );

            if (!isDead)
            {
                FireShot();
            }
        }

        isFiring = false;
    }

    private void FireShot()
    {
        if (player == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (distance > shootRange ||
            !HasLineOfSight())
        {
            return;
        }

        Vector3 origin =
            transform.position +
            Vector3.up *
            shootRayHeight;

        Vector3 targetPoint =
            player.position +
            Vector3.up *
            shootRayHeight;

        Vector3 direction =
            (
                targetPoint -
                origin
            ).normalized;

        if (Physics.Raycast(
            origin,
            direction,
            out RaycastHit hit,
            shootRange))
        {
            HealthSystem targetHealth =
                hit.collider
                    .GetComponent<HealthSystem>();

            if (targetHealth != null)
            {
                targetHealth.TakeDamage(
                    damage
                );
            }
        }
    }

    // ---------- DEATH ----------

    private void HandleDeath()
    {
        if (isDead)
            return;

        isDead = true;
        currentState = State.Dead;

        StopAllCoroutines();

        isFiring = false;
        hasPatrolTarget = false;

        if (animator != null)
        {
            if (!string.IsNullOrEmpty(walkingBool))
            {
                animator.SetBool(
                    walkingBool,
                    false
                );
            }

            if (!string.IsNullOrEmpty(spottedTrigger))
            {
                animator.ResetTrigger(
                    spottedTrigger
                );
            }

            if (!string.IsNullOrEmpty(shootTrigger))
            {
                animator.ResetTrigger(
                    shootTrigger
                );
            }

            if (!string.IsNullOrEmpty(deadTrigger))
            {
                animator.SetTrigger(
                    deadTrigger
                );
            }
        }

        if (rb != null)
        {
            rb.velocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;

            rb.isKinematic =
                true;
        }

        if (disableCollidersOnDeath)
        {
            foreach (
                Collider col
                in GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }
        }

        enabled = false;
    }

    // ---------- LINE OF SIGHT ----------

    private bool HasLineOfSight()
    {
        if (player == null)
            return false;

        Vector3 origin =
            transform.position +
            Vector3.up *
            shootRayHeight;

        Vector3 targetPoint =
            player.position +
            Vector3.up *
            shootRayHeight;

        Vector3 direction =
            targetPoint -
            origin;

        if (Physics.Raycast(
            origin,
            direction.normalized,
            out RaycastHit hit,
            direction.magnitude,
            obstructionMask))
        {
            return false;
        }

        return true;
    }

    // ---------- ROTATION ----------

    private void FaceTarget()
    {
        if (player == null)
            return;

        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        FaceDirection(direction);
    }

    private void FaceDirection(
        Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <
            0.01f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction
            );

        transform.rotation =
            Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime *
                8f
            );
    }

    // ---------- GIZMOS ----------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        Gizmos.color =
            Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            shootRange
        );

        Gizmos.color =
            Color.cyan;

        Vector3 center =
            Application.isPlaying
                ? patrolOrigin
                : transform.position;

        Gizmos.DrawWireSphere(
            center,
            patrolRadius
        );

        if (Application.isPlaying &&
            hasPatrolTarget)
        {
            Gizmos.DrawSphere(
                patrolTarget,
                0.2f
            );

            Gizmos.DrawLine(
                transform.position,
                patrolTarget
            );
        }
    }
}