
using UnityEngine;

public class FinalReportDesk : Interactable
{
    public override void Interact()
    {
        if (CaseResolutionManager.Instance == null ||
            !CaseResolutionManager.Instance.IsCaseResolved)
            return;

        if (FinalReportUI.Instance == null)
        {
            Debug.LogWarning(
                "FinalReportUI was not found in the scene.");
            return;
        }

        FinalReportUI.Instance.Open();
    }
}
