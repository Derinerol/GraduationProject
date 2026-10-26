
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewspaperReaderUI : MonoBehaviour
{
    public static NewspaperReaderUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject readerPanel;
    [SerializeField] private TMP_Text publicationText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text headlineText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button recordButton;
    [SerializeField] private Button closeButton;

    private string currentEvidenceID;
    private string currentEvidenceTitle;
    private string currentJournalNote;
    private string currentSource;

    public bool IsOpen => readerPanel != null && readerPanel.activeSelf;

    private void Awake()
    {
        Instance = this;

        readerPanel.SetActive(false);

        recordButton.onClick.AddListener(RecordEvidence);
        closeButton.onClick.AddListener(Close);
    }

    public void Open(
        string publication,
        string date,
        string headline,
        string body,
        string evidenceID,
        string evidenceTitle,
        string journalNote)
    {
        currentEvidenceID = evidenceID;
        currentEvidenceTitle = evidenceTitle;
        currentJournalNote = journalNote;
        currentSource = publication + ", " + date;

        publicationText.text = publication;
        dateText.text = date;
        headlineText.text = headline;
        bodyText.text = body;

        readerPanel.SetActive(true);
    }

    private void RecordEvidence()
    {
        if (JournalManager.Instance == null)
        {
            Debug.LogError("JournalManager is missing.");
            return;
        }

        JournalManager.Instance.AddEntry(
            currentEvidenceID,
            currentEvidenceTitle,
            currentJournalNote,
            "Evidence",
            currentSource
        );

        Close();
    }

    public void Close()
    {
        readerPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
