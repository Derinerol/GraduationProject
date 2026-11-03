
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class JournalistAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;

    [Header("Walking")]
    [SerializeField] private float smoothTime = 0.12f;
    [SerializeField] private float movementThreshold = 0.05f;
    [SerializeField] private float walkingTurnSpeed = 240f;

    [Header("Turn In Place")]
    [SerializeField] private float turnThreshold = 60f;
    [SerializeField] private float turnDuration = 0.7f;
    [SerializeField] private float maxSpeedForTurn = 0.5f;

    private Animator animator;
    private Transform playerRoot;

    private bool isTurning;
    private float turnTimer;
    private Quaternion startRotation;
    private Quaternion targetRotation;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;

        if (agent == null)
            agent = GetComponentInParent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("No NavMeshAgent found.", this);
            enabled = false;
            return;
        }

        playerRoot = agent.transform;
        agent.updateRotation = false;
    }

    private void Update()
    {
        if (!agent.enabled || !agent.isOnNavMesh)
            return;

        if (isTurning)
        {
            UpdateTurn();
            return;
        }

        Vector3 direction = GetTravelDirection();

        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Vector3.SignedAngle(
                playerRoot.forward,
                direction,
                Vector3.up
            );

            bool shouldTurnInPlace =
                agent.hasPath &&
                !agent.pathPending &&
                agent.remainingDistance > agent.stoppingDistance &&
                agent.velocity.magnitude < maxSpeedForTurn &&
                Mathf.Abs(angle) > turnThreshold;

            if (shouldTurnInPlace)
            {
                BeginTurn(angle, direction);
                return;
            }

            Quaternion desiredRotation =
                Quaternion.LookRotation(direction, Vector3.up);

            playerRoot.rotation = Quaternion.RotateTowards(
                playerRoot.rotation,
                desiredRotation,
                walkingTurnSpeed * Time.deltaTime
            );
        }

        UpdateWalkingAnimation();
    }

    private Vector3 GetTravelDirection()
    {
        Vector3 direction = agent.desiredVelocity;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f && agent.hasPath)
        {
            direction = agent.steeringTarget - playerRoot.position;
            direction.y = 0f;
        }

        return direction.normalized;
    }

    private void BeginTurn(float angle, Vector3 direction)
    {
        isTurning = true;
        turnTimer = 0f;

        startRotation = playerRoot.rotation;
        targetRotation =
            Quaternion.LookRotation(direction, Vector3.up);

        agent.isStopped = true;

        animator.SetFloat(SpeedHash, 0f);

        string state = angle < 0f
            ? "TurnLeft"
            : "TurnRight";

        animator.CrossFade(state, 0.08f);
    }

    private void UpdateTurn()
    {
        turnTimer += Time.deltaTime;

        float progress = Mathf.Clamp01(
            turnTimer / Mathf.Max(turnDuration, 0.01f)
        );

        // Smooth turning instead of constant-speed rotation.
        float eased = Mathf.SmoothStep(0f, 1f, progress);

        playerRoot.rotation = Quaternion.Slerp(
            startRotation,
            targetRotation,
            eased
        );

        if (progress >= 1f)
        {
            isTurning = false;
            agent.isStopped = false;

            animator.CrossFade("Locomotion", 0.1f);
        }
    }

    private void UpdateWalkingAnimation()
    {
        float speed = agent.velocity.magnitude;

        float normalizedSpeed = speed > movementThreshold
            ? Mathf.Clamp01(
                speed / Mathf.Max(agent.speed, 0.01f)
            )
            : 0f;

        animator.SetFloat(
            SpeedHash,
            normalizedSpeed,
            smoothTime,
            Time.deltaTime
        );
    }

    private void OnDisable()
    {
        if (agent != null &&
            agent.enabled &&
            agent.isOnNavMesh &&
            isTurning)
        {
            agent.isStopped = false;
        }
    }
}
