using System.Collections;
using UnityEngine;
using GGJ.Masks;
using Managers;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Chase,
        Attack
    }

    [Header("Identity")]
    [Tooltip("The type of predator this enemy represents. It will ignore players wearing this mask.")]
    [SerializeField]
    private MaskType predatorType;

    [Header("Detection")] [SerializeField] private float detectionRadius = 12f;
    [SerializeField] private float detectionRadius360 = 2.5f;
    [Range(0, 360)] [SerializeField] private float viewAngle = 60f;
    [SerializeField] private Transform visionPoint;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstructionMask;

    [Header("Attack")] [SerializeField] private float catchRadius = 0.8f;

    [Header("Audio")] 
    [Tooltip("Clip to play when this enemy catches the player.")]
    [SerializeField]
    private AudioClip attackSound;

    [Tooltip("If true, wait for the attack sound to finish before triggering Game Over.")]
    [SerializeField]
    private bool waitForSoundBeforeGameOver = true;

    [Header("Movement")] [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;

    [Tooltip("Points to patrol between. If empty, the enemy stands still.")] [SerializeField]
    private Transform[] patrolPoints;

    [Tooltip("How long to wait at each patrol point.")] [SerializeField]
    private float waitTimeAtPoint = 1f;

    [Header("Debug")]
    [SerializeField] private bool alwaysShowFOV = true;

    // State
    public EnemyState CurrentState { get; private set; } = EnemyState.Patrol;

    private int currentPatrolIndex;
    private Transform target;
    private PlayerMaskController playerMaskController;
    private CharacterController characterController;
    private AudioSource audioSource;
    private float waitTimer;
    private bool hasTriggeredAttack = false;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        // Find player
        var player = FindObjectOfType<WormController>();
        if (player != null)
        {
            target = player.transform;
            playerMaskController = player.GetComponent<PlayerMaskController>();
        }
        else
        {
            Debug.LogWarning("EnemyController: No WormController found in scene!");
        }

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            transform.position = patrolPoints[0].position;
        }
    }

    private void Update()
    {
        if (!GameManager.Instance || GameManager.Instance.GetState() != GameState.Gameplay)
            return;

        if (!target) return;

        UpdateState();
        ExecuteState();
    }

    private void UpdateState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);
        bool canSee = CanSeePlayer(distanceToPlayer);

        switch (CurrentState)
        {
            case EnemyState.Patrol:
                if (canSee)
                {
                    CurrentState = EnemyState.Chase;
                }

                break;

            case EnemyState.Chase:
                if (!canSee)
                {
                    // Lost sight -> go back to patrol (could implement a 'Search' state here later)
                    CurrentState = EnemyState.Patrol;
                }
                else if (distanceToPlayer <= catchRadius)
                {
                    CurrentState = EnemyState.Attack;
                }

                break;

            case EnemyState.Attack:
                // Once attacking, we usually stay there until game over logic kicks in
                break;
        }
    }

    private void ExecuteState()
    {
        switch (CurrentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                ChasePlayer();
                break;
            case EnemyState.Attack:
                AttackPlayer();
                break;
        }
    }

    private bool CanSeePlayer(float distance)
    {
        // 1. Distance Check (Global cutoff)
        if (distance > detectionRadius) return false;

        // 2. Disguise Check (Magic logic: if wearing mask, invisible to this predator type)
        if (playerMaskController && playerMaskController.CurrentMaskType == predatorType)
        {
            return false;
        }
        
        // 3. Proximity Check (360 detection if very close)
        // If within the inner circle, we see them regardless of angle
        bool inProximity = distance <= detectionRadius360;

        // Determine eye position and forward
        Vector3 eyePosition = visionPoint != null ? visionPoint.position : transform.position + Vector3.up * 0.5f;
        Vector3 eyeForward = visionPoint != null ? visionPoint.forward : transform.forward;
        
        // Raycast origin
        Vector3 origin = eyePosition;
        Vector3 targetPos = target.position + Vector3.up * 0.5f; // Aim at player center/head
        Vector3 dirToTarget = (targetPos - origin);
        float distToTarget = dirToTarget.magnitude;
        
        // 4. Angle Check (Field of View) - Skipped if in proximity
        if (!inProximity)
        {
            // We flatten the direction to the XZ plane so height differences don't cause the enemy to lose sight.
            Vector3 flatDir = dirToTarget;
            flatDir.y = 0;
            
            // Flatten eye forward
            Vector3 flatEyeForward = eyeForward;
            flatEyeForward.y = 0;

            // Check angle only if valid vectors
            if (flatDir.sqrMagnitude > 0.001f && flatEyeForward.sqrMagnitude > 0.001f)
            {
                 if (Vector3.Angle(flatEyeForward, flatDir) > viewAngle / 2)
                 {
                     return false; // Outside cone
                 }
            }
        }

        // 5. Obstruction Check (Line of Sight)
        // Check for walls between eyes and target
        if (Physics.Raycast(origin, dirToTarget.normalized, out RaycastHit hit, distToTarget, obstructionMask))
        {
            // Hit an obstruction before the player
            return false;
        }

        return true;
    }

    private void ChasePlayer()
    {
        MoveTowards(target.position, chaseSpeed);
    }

    private void AttackPlayer()
    {
        // Stop moving
        MoveTowards(transform.position, 0f);
        Debug.Log("Attacking Player!!!!!");
        if (hasTriggeredAttack) return;
        hasTriggeredAttack = true;

        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
            if (waitForSoundBeforeGameOver)
            {
                StartCoroutine(DelayedGameOver(attackSound.length));
                return;
            }
        }

        // Trigger Game Over
        GameManager.Instance.TriggerGameOver(false);
    }

    private IEnumerator DelayedGameOver(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager.Instance.TriggerGameOver(false);
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Transform wp = patrolPoints[currentPatrolIndex];
        
        // Use horizontal distance check (ignore Y height differences)
        Vector3 enemyPos = transform.position;
        Vector3 targetPos = wp.position;
        enemyPos.y = 0;
        targetPos.y = 0;
        
        float distToWaypoint = Vector3.Distance(enemyPos, targetPos);

        if (distToWaypoint < 0.5f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                waitTimer = 0f;
            }
        }
        else
        {
            MoveTowards(wp.position, moveSpeed);
            waitTimer = 0f;
        }
    }

    private void MoveTowards(Vector3 destination, float speed)
    {
        Vector3 offset = destination - transform.position;
        offset.y = 0; // Flatten FIRST to ensure horizontal movement logic isn't skewed by height
        
        Vector3 direction = offset.normalized; 

        if (direction != Vector3.zero && direction.sqrMagnitude > 0.001f)
        {
            // Rotate to face movement
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f);
        }

        if (characterController)
        {
            characterController.Move(direction * (speed * Time.deltaTime) + Physics.gravity * Time.deltaTime);
        }
        else
        {
            transform.position += direction * (speed * Time.deltaTime);
        }
    }

    private void OnDrawGizmos()
    {
        if (alwaysShowFOV)
        {
            DrawFOV();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // If we are already showing it, don't draw it again (mostly for performance/clarity)
        if (!alwaysShowFOV)
        {
            DrawFOV();
        }

        if (!Application.isPlaying && patrolPoints != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);
            }
        }
    }

    private void DrawFOV()
    {
        // Outer Radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Inner Radius (360 detection)
        Gizmos.color = new Color(1f, 0.5f, 0f); // Orange
        Gizmos.DrawWireSphere(transform.position, detectionRadius360);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchRadius);

        // Determine eye transform for Gizmos
        Transform eye = visionPoint != null ? visionPoint : transform;
        Vector3 eyePos = visionPoint != null ? visionPoint.position : transform.position;
        // Adjust default eyePos for cases where visionPoint is null to match CanSeePlayer logic
        if (visionPoint == null) eyePos += Vector3.up * 0.5f;

        // Draw Field of View lines
        // We use the eye's rotation
        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, eye.eulerAngles.y);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, eye.eulerAngles.y);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(eyePos, eyePos + viewAngleA * detectionRadius);
        Gizmos.DrawLine(eyePos, eyePos + viewAngleB * detectionRadius);
        
        // Optional: Draw a line to the target if visible for debug
        if (Application.isPlaying && target != null && CanSeePlayer(Vector3.Distance(transform.position, target.position)))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(eyePos, target.position);
        }
    }

    private Vector3 DirFromAngle(float angleInDegrees, float globalAngleY)
    {
        angleInDegrees += globalAngleY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
