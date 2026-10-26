
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FinalReportUI : MonoBehaviour
{
    public static FinalReportUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject reportPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button publishButton;
    [SerializeField] private TMP_Text articleBody;
    public static event Action OnReportPublished;

    private const string FinalReportID =
        "final_report_completed";

    public bool IsOpen =>
        reportPanel != null && reportPanel.activeSelf;

    private void Awake()
    {
        Instance = this;

        if (reportPanel != null)
            reportPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (publishButton != null)
            publishButton.onClick.AddListener(Publish);
    }

    public void Open()
    {
        if (reportPanel == null)
            return;

        if (CaseResolutionManager.Instance == null ||
            !CaseResolutionManager.Instance.IsCaseResolved)
            return;

        reportPanel.SetActive(true);

        if (publishButton != null)
        {
            bool alreadyPublished =
                JournalManager.Instance != null &&
                JournalManager.Instance.HasInformation(
                    FinalReportID);

            publishButton.interactable = !alreadyPublished;
        }
    }

    public void Close()
    {
        if (reportPanel != null)
            reportPanel.SetActive(false);
    }

    private void Publish()
    {
        JournalManager journal = JournalManager.Instance;

        if (journal == null)
            return;

        if (journal.HasInformation(FinalReportID))
            return;

        if (CaseResolutionManager.Instance == null ||
            !CaseResolutionManager.Instance.IsCaseResolved)
            return;

        journal.AddEntry(
            FinalReportID,
            "Final Investigation Report",
            "The journalist published an investigative " +
            "report documenting Daniel Mercer's death, " +
            "the concealment of his remains, and the " +
            "suppression of the original investigation " +
            "by Frank Bell and Michael Dwyer. " +
            "The report distinguishes established " +
            "evidence from Bell's account of an " +
            "accidental fall.",
            "Interpretation",
            "Published Investigation");

        if (publishButton != null)
            publishButton.interactable = false;

        Close();

        Debug.Log("Final investigation report published.");

        OnReportPublished?.Invoke();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
