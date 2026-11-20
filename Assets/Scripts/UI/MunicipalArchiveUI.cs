
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MunicipalArchiveUI : MonoBehaviour
{
    public static MunicipalArchiveUI Instance
    {
        get;
        private set;
    }

    [Header("UI References")]
    [SerializeField] private GameObject archivePanel;
    [SerializeField] private TMP_Text archiveTitleText;
    [SerializeField] private Transform documentContainer;
    [SerializeField] private Button documentButtonPrefab;
    [SerializeField] private TMP_Text emptyMessageText;
    [SerializeField] private Button closeButton;

    public bool IsOpen =>
        archivePanel != null && archivePanel.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError(
                "Only one MunicipalArchiveUI should exist.",
                this
            );
            enabled = false;
            return;
        }

        Instance = this;

        if (archivePanel == null ||
            archiveTitleText == null ||
            documentContainer == null ||
            documentButtonPrefab == null ||
            closeButton == null)
        {
            Debug.LogError(
                "MunicipalArchiveUI: Missing UI references.",
                this
            );
            enabled = false;
            return;
        }

        archivePanel.SetActive(false);
        closeButton.onClick.AddListener(Close);
    }

    public void Open(MunicipalRecordsArchive archive)
    {
        if (archive == null || !enabled)
            return;

        ClearDocumentButtons();

        archiveTitleText.text = archive.ArchiveTitle;

        int availableCount = 0;

        foreach (MunicipalRecord record in archive.Documents)
        {
            if (record == null || !record.IsAvailable)
                continue;

            availableCount++;

            MunicipalRecord selectedRecord = record;

            Button button = Instantiate(
                documentButtonPrefab,
                documentContainer
            );

            TMP_Text label =
                button.GetComponentInChildren<TMP_Text>(true);

            if (label != null)
                label.text = record.DisplayTitle;

            button.onClick.AddListener(() =>
            {
                Close();
                selectedRecord.Interact();
            });
        }

        if (emptyMessageText != null)
        {
            emptyMessageText.gameObject.SetActive(
                availableCount == 0
            );

            emptyMessageText.text =
                "No records matching the current inquiry.";
        }

        archivePanel.SetActive(true);
    }

    private void ClearDocumentButtons()
    {
        for (int i = documentContainer.childCount - 1;
             i >= 0; i--)
        {
            Destroy(
                documentContainer.GetChild(i).gameObject
            );
        }
    }

    public void Close()
    {
        if (archivePanel != null)
            archivePanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
