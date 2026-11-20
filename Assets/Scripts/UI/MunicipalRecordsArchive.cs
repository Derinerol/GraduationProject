
using System.Collections.Generic;
using UnityEngine;

public class MunicipalRecordsArchive : Interactable
{
    [Header("Archive")]
    [SerializeField]
    private string archiveTitle =
        "Records Archive";

    [SerializeField] private MunicipalRecord[] documents;

    [Header("Optional Access Requirement")]
    [SerializeField]
    private string requiredAccessInformationID =
        "helen_records_procedure";

    [TextArea(2, 4)]
    [SerializeField]
    private string accessDeniedMessage =
        "I should speak with the clerk before searching these records.";

    public string ArchiveTitle => archiveTitle;
    public MunicipalRecord[] Documents => documents;

    public override void Interact()
    {
        JournalManager journal = JournalManager.Instance;

        if (journal == null)
        {
            Debug.LogError(
                "JournalManager is missing.", this
            );
            return;
        }

        // An empty access ID means the archive
        // has no additional access requirement.
        if (!string.IsNullOrWhiteSpace(requiredAccessInformationID) &&
            !journal.HasInformation(requiredAccessInformationID))
        {
            DialogueManager dialogue =
                DialogueManager.Instance;

            if (dialogue != null)
            {
                dialogue.ShowDialogue(
                    archiveTitle,
                    accessDeniedMessage,
                    new List<DialogueChoice>
                    {
                        new DialogueChoice(
                            "Step away",
                            "I'll come back when I know more."
                        )
                    }
                );
            }

            return;
        }

        MunicipalArchiveUI archiveUI =
            MunicipalArchiveUI.Instance;

        if (archiveUI == null)
        {
            Debug.LogError(
                "MunicipalArchiveUI is missing.",
                this
            );
            return;
        }

        archiveUI.Open(this);
    }
}
