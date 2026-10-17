
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class JournalManager : MonoBehaviour
{
    public static JournalManager Instance { get; private set; }

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

        journalPanel.SetActive(false);

        if (discoveryNotificationText != null)
            discoveryNotificationText.gameObject.SetActive(false);

        closeButton.onClick.AddListener(CloseJournal);

        connectButton.onClick.AddListener(ConnectSelectedEntries);

        if (connectionStatusText != null)
        {
            connectionStatusText.text = "";
        }
        if (journalEntriesText != null)
            journalEntriesText.gameObject.SetActive(false);

        UpdateJournalUI();
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
            if (connectionStatusText != null)
            {
                connectionStatusText.text =
                    "You can select only two entries at a time.";
            }
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

        discoveryNotificationText.gameObject.SetActive(false);
        notificationCoroutine = null;
    }

    public void OpenJournal()
    {
        journalPanel.SetActive(true);
    }

    public void CloseJournal()
    {
        journalPanel.SetActive(false);
    }

    public bool IsJournalOpen()
    {
        return journalPanel.activeSelf;
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

        bool validCombination =
            (firstID == "daniel_newspaper" &&
             secondID == "resident_daniel_statement") ||
            (firstID == "resident_daniel_statement" &&
             secondID == "daniel_newspaper");

        if (validCombination)
        {
            formedHypothesisIDs.Add("newspaper_resident_connection");
            AddEntry(
                "hypothesis_newspaper_resident",
                "Hypothesis: The accounts are connected",
                "The newspaper account and the resident's statement " +
                "can be considered together. Further investigation " +
                "may clarify the circumstances of Daniel's disappearance.",
                "Hypothesis",
                "Player interpretation"
            );

            SetConnectionStatus("Hypothesis formed.");
        }
        else
        {
            SetConnectionStatus(
                "These pieces of information do not form a known hypothesis."
            );
        }

        selectedInformationIDs.Clear();
    }

    private void SetConnectionStatus(string message)
    {
        if (connectionStatusText != null)
        {
            connectionStatusText.text = message;
        }
    }
}
