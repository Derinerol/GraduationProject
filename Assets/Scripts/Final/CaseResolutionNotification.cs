using System.Collections;
using TMPro;
using UnityEngine;

public class CaseResolutionNotification : MonoBehaviour
{
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private float displayDuration = 6f;

    private Coroutine hideRoutine;
    private CaseResolutionManager subscribedManager;

    private void Start()
    {
        if (notificationText != null)
            notificationText.gameObject.SetActive(false);

        subscribedManager = CaseResolutionManager.Instance;
        if (subscribedManager == null)
        {
            Debug.LogWarning("CaseResolutionNotification: CaseResolutionManager not found.");
            return;
        }

        subscribedManager.OnCaseResolved += ShowResolutionNotification;

        // The resolution may have happened in a different scene.
        if (subscribedManager.IsCaseResolved)
            ShowResolutionNotification();
    }

    private void OnDestroy()
    {
        if (subscribedManager != null)
            subscribedManager.OnCaseResolved -= ShowResolutionNotification;
    }

    private void ShowResolutionNotification()
    {
        if (notificationText == null)
            return;

        notificationText.text =
            "Investigation Complete\n" +
            "Return to the motel room to publish your findings.";

        notificationText.gameObject.SetActive(true);

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        if (notificationText != null)
            notificationText.gameObject.SetActive(false);

        hideRoutine = null;
    }
}
