
using UnityEngine;

public class NewspaperEvidence : Interactable
{
    [Header("Newspaper Content")]
    [SerializeField] private string publication = "THE GRAYFORD GAZETTE";
    [SerializeField] private string publicationDate;
    [SerializeField] private string headline;

    [TextArea(8, 20)]
    [SerializeField] private string articleBody;

    [Header("Journal Evidence")]
    [SerializeField] private string evidenceID;
    [SerializeField] private string evidenceTitle;

    [TextArea(2, 5)]
    [SerializeField] private string journalNote;

    public override void Interact()
    {
        if (NewspaperReaderUI.Instance == null)
        {
            Debug.LogError("NewspaperReaderUI is missing from the scene.");
            return;
        }

        NewspaperReaderUI.Instance.Open(
            publication,
            publicationDate,
            headline,
            articleBody,
            evidenceID,
            evidenceTitle,
            journalNote
        );
    }
}
