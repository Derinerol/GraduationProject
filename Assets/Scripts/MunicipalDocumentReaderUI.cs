using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MunicipalDocumentReaderUI : MonoBehaviour
{
    public static MunicipalDocumentReaderUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject readerPanel;
    [SerializeField] private TMP_Text departmentText;
    [SerializeField] private TMP_Text documentTitleText;
    [SerializeField] private TMP_Text referenceText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text documentBodyText;
    [SerializeField] private Button recordButton;
    [SerializeField] private Button closeButton;

    private string evidenceID;
    private string evidenceTitle;
    private string journalNote;
    private string evidenceSource;

    public bool IsOpen => readerPanel != null && readerPanel.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Only one MunicipalDocumentReaderUI should exist per scene.", this);
            enabled = false;
            return;
        }
        Instance = this;
        if (readerPanel == null || recordButton == null || closeButton == null)
        {
            Debug.LogError("MunicipalDocumentReaderUI: assign panel and both buttons.", this);
            enabled = false;
            return;
        }
        readerPanel.SetActive(false);
        recordButton.onClick.AddListener(RecordEvidence);
        closeButton.onClick.AddListener(Close);
    }

    public void Open(string department, string title, string reference, string date,
        string body, string entryID, string entryTitle, string entryNote, string source)
    {
        evidenceID = entryID;
        evidenceTitle = entryTitle;
        journalNote = entryNote;
        evidenceSource = source;

        departmentText.text = department;
        documentTitleText.text = title;
        referenceText.text = reference;
        dateText.text = date;
        documentBodyText.text = body;
        recordButton.gameObject.SetActive(true);
        recordButton.interactable = true;
        readerPanel.SetActive(true);
    }

    public void ShowUnavailable(string message)
    {
        evidenceID = null;
        departmentText.text = "GRAYFORD MUNICIPAL RECORDS";
        documentTitleText.text = "Records Search";
        referenceText.text = "";
        dateText.text = "";
        documentBodyText.text = message;
        recordButton.gameObject.SetActive(false);
        readerPanel.SetActive(true);
    }

    private void RecordEvidence()
    {
        if (JournalManager.Instance == null || string.IsNullOrWhiteSpace(evidenceID))
        {
            Debug.LogError("Cannot record municipal evidence: missing JournalManager or evidence ID.", this);
            return;
        }
        JournalManager.Instance.AddEntry(evidenceID, evidenceTitle, journalNote,
            "Evidence", evidenceSource);
        Close();
    }

    public void Close()
    {
        if (readerPanel != null) readerPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
