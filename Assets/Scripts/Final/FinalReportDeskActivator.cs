
using UnityEngine;

public class FinalReportDeskActivator : MonoBehaviour
{
    [SerializeField]
    private GameObject finalReportDesk;

    private void Start()
    {
        RefreshDesk();
    }

    private void Update()
    {
        if (finalReportDesk == null)
            return;

        bool shouldBeActive =
            CaseResolutionManager.Instance != null &&
            CaseResolutionManager.Instance.IsCaseResolved;

        if (finalReportDesk.activeSelf != shouldBeActive)
        {
            finalReportDesk.SetActive(shouldBeActive);
        }
    }

    private void RefreshDesk()
    {
        if (finalReportDesk == null)
        {
            Debug.LogWarning(
                "FinalReportDeskActivator: Desk not assigned.");
            return;
        }

        bool unlocked =
            CaseResolutionManager.Instance != null &&
            CaseResolutionManager.Instance.IsCaseResolved;

        finalReportDesk.SetActive(unlocked);
    }
}
