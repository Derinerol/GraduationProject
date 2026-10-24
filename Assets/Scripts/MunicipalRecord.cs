using UnityEngine;

public class MunicipalRecord : Interactable
{
    [Header("Prerequisite")]
    [SerializeField] private string requiredInformationID;
    [TextArea(2, 5)]
    [SerializeField]
    private string unavailableMessage =
        "The records are indexed by reference number. I need a specific lead first.";

    [Header("Document")]
    [SerializeField] private string department = "GRAYFORD MUNICIPAL RECORDS";
    [SerializeField] private string documentTitle;
    [SerializeField] private string reference = "WW-91-184";
    [SerializeField] private string documentDate = "08 OCT 1991";
    [TextArea(6, 18)][SerializeField] private string documentBody;

    [Header("Journal Evidence")]
    [SerializeField] private string evidenceID;
    [SerializeField] private string evidenceTitle;
    [TextArea(2, 6)][SerializeField] private string journalNote;
    [SerializeField] private string evidenceSource = "Grayford Municipal Records Office";

    public override void Interact()
    {
        var reader = MunicipalDocumentReaderUI.Instance;
        if (reader == null)
        {
            Debug.LogError("MunicipalDocumentReaderUI is missing from this scene.", this);
            return;
        }
        var journal = JournalManager.Instance;
        if (journal == null)
        {
            Debug.LogError("JournalManager is missing.", this);
            return;
        }
        if (!string.IsNullOrWhiteSpace(requiredInformationID) &&
            !journal.HasInformation(requiredInformationID))
        {
            reader.ShowUnavailable(unavailableMessage);
            return;
        }
        reader.Open(department, documentTitle, reference, documentDate,
            documentBody, evidenceID, evidenceTitle, journalNote, evidenceSource);
    }
}
