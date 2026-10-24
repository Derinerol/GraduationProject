using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class ClickToMove : MonoBehaviour
{
    [SerializeField] private Camera gameCamera;
    [SerializeField] private float interactionDistance = 1.5f;

    private NavMeshAgent agent;
    private Interactable targetInteractable;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (gameCamera == null)
            gameCamera = Camera.main;
    }

    private void Update()
    {

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            bool dialogueOpen =
                DialogueManager.Instance != null &&
                DialogueManager.Instance.IsDialogueOpen();

            bool journalOpen =
                JournalManager.Instance != null &&
                JournalManager.Instance.IsJournalOpen();

            if (dialogueOpen || journalOpen)
                return;

            if (NewspaperReaderUI.Instance != null &&
    NewspaperReaderUI.Instance.IsOpen)
                {
                return;
            }
            HandleClick();
        }

        CheckInteractionDistance();
    }

    private void HandleClick()
    {
        Ray ray = gameCamera.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        if (!Physics.Raycast(ray, out RaycastHit hit, 500f))
            return;

        Interactable interactable =
            hit.collider.GetComponentInParent<Interactable>();

        if (interactable != null)
        {
            targetInteractable = interactable;

            Vector3 direction =
                transform.position - interactable.transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                direction = Vector3.forward;

            Vector3 destination =
                interactable.transform.position +
                direction.normalized * interactionDistance;

            if (NavMesh.SamplePosition(
                destination, out NavMeshHit navHit,
                2f, agent.areaMask))
            {
                if (agent.isOnNavMesh)
                    agent.SetDestination(navHit.position);
            }

            return;
        }

        // Clicking ordinary scenery or ground cancels the NPC target
        targetInteractable = null;

        if (NavMesh.SamplePosition(
            hit.point, out NavMeshHit groundHit,
            1f, agent.areaMask))
        {
            if (agent.isOnNavMesh)
                agent.SetDestination(groundHit.position);
        }
    }

    private void CheckInteractionDistance()
    {
        if (targetInteractable == null)
        {
            return;
        }

        if (!agent.isOnNavMesh)
        {
            return;
        }

        Vector3 playerPosition = transform.position;
        Vector3 targetPosition = targetInteractable.transform.position;

        playerPosition.y = 0f;
        targetPosition.y = 0f;

        float distance =
            Vector3.Distance(playerPosition, targetPosition);

        if (distance <= interactionDistance + 0.2f &&
            !agent.pathPending)
        {
            agent.ResetPath();

            Interactable interactable = targetInteractable;
            targetInteractable = null;

            interactable.Interact();
        }
    }
}