
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class JournalManager : MonoBehaviour
{
    public static JournalManager Instance { get; private set; }

    [System.Serializable]
    private class HypothesisDefinition
    {
        [Header("Identity")]
        public string hypothesisID;
        public string informationEntryID;
        public string title;

        [TextArea(3, 8)]
        public string description;

        [Header("Required Information")]
        public string requiredInformationID1;
        public string requiredInformationID2;
    }

    [Header("Journal UI")]
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private TMP_Text journalEntriesText;
    [SerializeField] private Button closeButton;

    [Header("Selectable Entries")]
    [SerializeField] private Transform entryList;
    [SerializeField] private Button entryButtonPrefab;

    [Header("Discovery Notification")]
    [SerializeField] private TMP_Text discoveryNotificationText;
    [SerializeField] private float notificationDuration = 3f;

    [Header("Hypothesis Connections")]
    [SerializeField] private Button connectButton;
    [SerializeField] private TMP_Text connectionStatusText;

    [SerializeField]
    private List<HypothesisDefinition> hypothesisDefinitions =
        new List<HypothesisDefinition>();

    private readonly List<string> selectedInformationIDs =
        new List<string>();

    private Coroutine notificationCoroutine;

    private readonly List<JournalEntry> entries =
        new List<JournalEntry>();

    private readonly HashSet<string> discoveredIDs =
        new HashSet<string>();

    private readonly List<Button> entryButtons =
        new List<Button>();

    private readonly HashSet<string> formedHypothesisIDs =
        new HashSet<string>();

    public string SelectedInformationID { get; private set; }

    private class JournalEntry
    {
        public string ID;
        public string Title;
        public string Description;
        public string Type;
        public string Source;
    }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Keep the journal manager and its attached UI between scenes.
        DontDestroyOnLoad(gameObject);

        if (journalPanel != null)
            journalPanel.SetActive(false);

        if (discoveryNotificationText != null)
            discoveryNotificationText.gameObject.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseJournal);

        if (connectButton != null)
            connectButton.onClick.AddListener(ConnectSelectedEntries);

        if (connectionStatusText != null)
            connectionStatusText.text = "";

        if (journalEntriesText != null)
            journalEntriesText.gameObject.SetActive(false);

        UpdateJournalUI();
    }

    public void BindSceneUI(
        GameObject newJournalPanel,
        TMP_Text newJournalEntriesText,
        Button newCloseButton,
        Transform newEntryList,
        Button newEntryButtonPrefab,
        TMP_Text newDiscoveryNotificationText,
        Button newConnectButton,
        TMP_Text newConnectionStatusText)
    {
        // Connect the persistent manager to this scene's UI.
        journalPanel = newJournalPanel;
        journalEntriesText = newJournalEntriesText;
        closeButton = newCloseButton;
        entryList = newEntryList;
        entryButtonPrefab = newEntryButtonPrefab;
        discoveryNotificationText = newDiscoveryNotificationText;
        connectButton = newConnectButton;
        connectionStatusText = newConnectionStatusText;

        // Prevent duplicate listeners when reconnecting UI.
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseJournal);
            closeButton.onClick.AddListener(CloseJournal);
        }

        if (connectButton != null)
        {
            connectButton.onClick.RemoveListener(ConnectSelectedEntries);
            connectButton.onClick.AddListener(ConnectSelectedEntries);
        }

        if (journalPanel != null)
            journalPanel.SetActive(false);

        if (discoveryNotificationText != null)
            discoveryNotificationText.gameObject.SetActive(false);

        if (journalEntriesText != null)
            journalEntriesText.gameObject.SetActive(false);

        if (connectionStatusText != null)
            connectionStatusText.text = "";

        // Recreate the entry buttons from the persistent journal data.
        UpdateJournalUI();

        Debug.Log("JournalManager connected to the current scene's UI.");
    }

    public bool HasHypothesis(string hypothesisID)
    {
        return formedHypothesisIDs.Contains(hypothesisID);
    }

    public bool HasInformation(string informationID)
    {
        return discoveredIDs.Contains(informationID);
    }

    public void AddEntry(
        string informationID,
        string title,
        string description,
        string informationType,
        string informationSource)
    {
        if (string.IsNullOrWhiteSpace(informationID))
        {
            Debug.LogError(
                "JournalManager: Cannot add an entry without an ID."
            );
            return;
        }

        if (discoveredIDs.Contains(informationID))
        {
            Debug.Log("Already recorded: " + title);
            return;
        }

        discoveredIDs.Add(informationID);

        entries.Add(new JournalEntry
        {
            ID = informationID,
            Title = title,
            Description = description,
            Type = informationType,
            Source = informationSource
        });

        UpdateJournalUI();
        ShowDiscoveryNotification(title);

        Debug.Log("New information recorded: " + title);
    }

    public void RecordClaim(
        string informationID,
        string title,
        string description,
        string informationSource)
    {
        AddEntry(
            informationID,
            title,
            description,
            "Claim",
            informationSource
        );
    }

    private void UpdateJournalUI()
    {
        foreach (Button button in entryButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }

        entryButtons.Clear();

        if (entryList == null || entryButtonPrefab == null)
            return;

        foreach (JournalEntry entry in entries)
        {
            Button button = Instantiate(
                entryButtonPrefab,
                entryList
            );

            TMP_Text buttonText =
                button.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
                buttonText.text = entry.Title;

            button.onClick.AddListener(
                () => SelectEntry(entry.ID)
            );

            entryButtons.Add(button);
        }
    }

    private void SelectEntry(string informationID)
    {
        SelectedInformationID = informationID;

        if (selectedInformationIDs.Contains(informationID))
        {
            selectedInformationIDs.Remove(informationID);
        }
        else if (selectedInformationIDs.Count < 2)
        {
            selectedInformationIDs.Add(informationID);
        }
        else
        {
            SetConnectionStatus(
                "You can select only two entries at a time."
            );
        }

        JournalEntry entry = entries.Find(
            item => item.ID == informationID
        );

        if (entry == null)
            return;

        Debug.Log("Selected journal entry: " + entry.Title);

        if (journalEntriesText != null)
        {
            journalEntriesText.gameObject.SetActive(true);

            journalEntriesText.text =
                "[" + entry.Type + "]\n" +
                "Source: " + entry.Source + "\n" +
                entry.Title + "\n" +
                entry.Description;
        }

        if (selectedInformationIDs.Count == 0)
            SetConnectionStatus("No entries selected.");
        else if (selectedInformationIDs.Count == 1)
            SetConnectionStatus("Select one more entry.");
        else
            SetConnectionStatus("Two entries selected.");
    }

    private void ShowDiscoveryNotification(string title)
    {
        if (discoveryNotificationText == null)
            return;

        discoveryNotificationText.text =
            "Information recorded: " + title;

        discoveryNotificationText.gameObject.SetActive(true);

        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        notificationCoroutine =
            StartCoroutine(HideDiscoveryNotification());
    }

    private IEnumerator HideDiscoveryNotification()
    {
        yield return new WaitForSeconds(notificationDuration);

        if (discoveryNotificationText != null)
            discoveryNotificationText.gameObject.SetActive(false);

        notificationCoroutine = null;
    }

    public void OpenJournal()
    {
        if (journalPanel != null)
            journalPanel.SetActive(true);
    }

    public void CloseJournal()
    {
        if (journalPanel != null)
            journalPanel.SetActive(false);
    }

    public bool IsJournalOpen()
    {
        return journalPanel != null && journalPanel.activeSelf;
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            if (IsJournalOpen())
                CloseJournal();
            else
                OpenJournal();
        }
    }

    private void ConnectSelectedEntries()
    {
        if (selectedInformationIDs.Count != 2)
        {
            SetConnectionStatus("Select two entries first.");
            return;
        }

        string firstID = selectedInformationIDs[0];
        string secondID = selectedInformationIDs[1];

        HypothesisDefinition matchedHypothesis = null;

        foreach (HypothesisDefinition definition
                 in hypothesisDefinitions)
        {
            if (definition == null)
                continue;

            bool matches =
                (firstID == definition.requiredInformationID1 &&
                 secondID == definition.requiredInformationID2) ||
                (firstID == definition.requiredInformationID2 &&
                 secondID == definition.requiredInformationID1);

            if (matches)
            {
                matchedHypothesis = definition;
                break;
            }
        }

        selectedInformationIDs.Clear();

        if (matchedHypothesis == null)
        {
            SetConnectionStatus(
                "These pieces of information do not form a known hypothesis."
            );
            return;
        }

        if (formedHypothesisIDs.Contains(
                matchedHypothesis.hypothesisID))
        {
            SetConnectionStatus(
                "You have already formed this hypothesis."
            );
            return;
        }

        formedHypothesisIDs.Add(
            matchedHypothesis.hypothesisID
        );

        AddEntry(
            matchedHypothesis.informationEntryID,
            matchedHypothesis.title,
            matchedHypothesis.description,
            "Hypothesis",
            "Player interpretation"
        );

        SetConnectionStatus("Hypothesis formed.");
    }

    private void SetConnectionStatus(string message)
    {
        if (connectionStatusText != null)
            connectionStatusText.text = message;
    }
}
