
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private Texture2D npcCursor;
    [SerializeField] private Texture2D objectCursor;

    [Header("Cursor Settings")]
    [SerializeField] private Vector2 hotspot = Vector2.zero;
    [SerializeField] private float rayDistance = 100f;

    private Interactable currentInteractable;

    private void Awake()
    {
        if (gameCamera == null)
            gameCamera = Camera.main;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void Update()
    {
        if (gameCamera == null)
            return;

        bool dialogueOpen =
            DialogueManager.Instance != null &&
            DialogueManager.Instance.IsDialogueOpen();

        bool journalOpen =
            JournalManager.Instance != null &&
            JournalManager.Instance.IsJournalOpen();

        if (dialogueOpen || journalOpen)
        {
            SetCursor(null);
            currentInteractable = null;
            return;
        }

        if (Mouse.current == null)
            return;

        Ray ray = gameCamera.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            Interactable interactable =
                hit.collider.GetComponentInParent<Interactable>();

            if (interactable != null)
            {
                currentInteractable = interactable;

                if (interactable.IsInformationSource)
                    SetCursor(objectCursor);
                else
                    SetCursor(npcCursor);

                return;
            }
        }

        currentInteractable = null;
        SetCursor(null);
    }

    private void SetCursor(Texture2D cursorTexture)
    {
        Cursor.SetCursor(
            cursorTexture,
            cursorTexture == null ? Vector2.zero : hotspot,
            CursorMode.Auto
        );
    }
}
