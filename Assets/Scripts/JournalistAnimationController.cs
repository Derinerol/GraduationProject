using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class JournalistAnimationController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float smoothTime = 0.12f;
    [SerializeField] private float movementThreshold = 0.05f;

    private Animator animator;
    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;

        if (agent == null)
            agent = GetComponentInParent<NavMeshAgent>();
    }

    private void Update()
    {
        if (agent == null)
            return;

        float velocity = agent.velocity.magnitude;

        float normalizedSpeed =
            agent.enabled &&
            agent.isOnNavMesh &&
            velocity > movementThreshold
                ? Mathf.Clamp01(
                    velocity / Mathf.Max(agent.speed, 0.01f)
                )
                : 0f;

        animator.SetFloat(
            SpeedHash,
            normalizedSpeed,
            smoothTime,
            Time.deltaTime
        );
    }
}