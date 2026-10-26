
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JournalUIBinder : MonoBehaviour
{
    [Header("Journal UI References")]
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private TMP_Text journalEntriesText;
    [SerializeField] private Button closeButton;

    [Header("Selectable Entries")]
    [SerializeField] private Transform entryList;
    [SerializeField] private Button entryButtonPrefab;

    [Header("Discovery Notification")]
    [SerializeField] private TMP_Text discoveryNotificationText;

    [Header("Hypothesis Connections")]
    [SerializeField] private Button connectButton;
    [SerializeField] private TMP_Text connectionStatusText;

    private void Start()
    {
        if (JournalManager.Instance == null)
        {
            Debug.LogError(
                "JournalUIBinder: No JournalManager found in the scene."
            );
            return;
        }

        JournalManager.Instance.BindSceneUI(
            journalPanel,
            journalEntriesText,
            closeButton,
            entryList,
            entryButtonPrefab,
            discoveryNotificationText,
            connectButton,
            connectionStatusText
        );
    }
}
