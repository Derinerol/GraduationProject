
using System.Collections;
using TMPro;
using UnityEngine;

public class CaseResolutionNotification : MonoBehaviour
{
    [SerializeField]
    private TMP_Text notificationText;

    [SerializeField]
    private float displayDuration = 6f;

    private Coroutine hideRoutine;

    private void Start()
    {
        if (notificationText != null)
            notificationText.gameObject.SetActive(false);

        if (CaseResolutionManager.Instance != null)
        {
            CaseResolutionManager.Instance.OnCaseResolved +=
                ShowResolutionNotification;
        }
    }

    private void OnDestroy()
    {
        if (CaseResolutionManager.Instance != null)
        {
            CaseResolutionManager.Instance.OnCaseResolved -=
                ShowResolutionNotification;
        }
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
