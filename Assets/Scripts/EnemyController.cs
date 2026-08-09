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

    [Header("Shooting")]
    public float fireRate = 1.2f;
    public float damage = 10f;
    public float shootRayHeight = 1f;

    private Rigidbody rb;
    private float fireCooldown;

    private enum State { Idle, Chasing, Shooting }
    private State currentState = State.Idle;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = distanceToPlayer <= detectionRange && HasLineOfSight();

        //Debug.Log("Udaljenost: " + distanceToPlayer.ToString("F1") + " | Vidi igraèa: " + canSeePlayer + " | State: " + currentState);

        if (!canSeePlayer)
        {
            currentState = State.Idle;
        }
        else if (distanceToPlayer > shootRange)
        {
            currentState = State.Chasing;
        }
        else
        {
            currentState = State.Shooting;
        }

        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;

        if (currentState == State.Shooting)
        {
            FaceTarget();
            if (fireCooldown <= 0f)
            {
                Debug.Log("Enemy puca!");
                Shoot();
                fireCooldown = fireRate;
            }
        }
    }

    void FixedUpdate()
    {
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

    private bool HasLineOfSight()
    {
        Vector3 origin = transform.position + Vector3.up * shootRayHeight;
        Vector3 targetPoint = player.position + Vector3.up * shootRayHeight;
        Vector3 dir = targetPoint - origin;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dir.magnitude, obstructionMask))
        {
            return false;
        }
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

    private void Shoot()
    {
        Vector3 origin = transform.position + Vector3.up * shootRayHeight;
        Vector3 targetPoint = player.position + Vector3.up * shootRayHeight;
        Vector3 dir = (targetPoint - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, shootRange))
        {
            HealthSystem targetHealth = hit.collider.GetComponent<HealthSystem>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
            }
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