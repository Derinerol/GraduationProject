
using UnityEngine;

public class MotelEvidenceReveal : MonoBehaviour
{
    [Header("Package")]
    [SerializeField] private GameObject anonymousPackage;

    [Header("Documents")]
    [SerializeField] private GameObject maintenanceRecord;
    [SerializeField] private GameObject workOrderFragment;
    [SerializeField] private GameObject anonymousNote;

    private const string PackageID = "anonymous_package";

    private void Start()
    {
        RefreshEvidenceVisibility();
    }

    public void RevealDocuments()
    {
        if (JournalManager.Instance == null)
        {
            Debug.LogError(
                "MotelEvidenceReveal: JournalManager not found.",
                this
            );
            return;
        }

        if (!JournalManager.Instance.HasInformation(PackageID))
            return;

        RefreshEvidenceVisibility();
    }

    private void RefreshEvidenceVisibility()
    {
        if (JournalManager.Instance == null)
        {
            Debug.LogWarning(
                "MotelEvidenceReveal: Waiting for JournalManager.",
                this
            );
            return;
        }

        bool packageDiscovered =
            JournalManager.Instance.HasInformation(PackageID);

        if (anonymousPackage != null)
            anonymousPackage.SetActive(!packageDiscovered);

        if (maintenanceRecord != null)
            maintenanceRecord.SetActive(packageDiscovered);

        if (workOrderFragment != null)
            workOrderFragment.SetActive(packageDiscovered);

        if (anonymousNote != null)
            anonymousNote.SetActive(packageDiscovered);
    }
}
