
using System.Collections.Generic;
using UnityEngine;

public class WaterworksPreliminaryReport : Interactable
{
    private const string RequestID =
        "chamber_official_examination_requested";

    private const string ReportID =
        "waterworks_preliminary_examination_report";

    private const string Source =
        "Preliminary Examination Report, October 1998";

    public override void Interact()
    {
        JournalManager journal = JournalManager.Instance;
        DialogueManager dialogue = DialogueManager.Instance;

        if (journal == null || dialogue == null)
            return;

        bool requested = journal.HasInformation(RequestID);
        bool collected = journal.HasInformation(ReportID);

        var choices = new List<DialogueChoice>();

        if (!requested)
        {
            choices.Add(new DialogueChoice(
                "Check the document",
                "No examination findings have been issued."
            ));
        }
        else if (!collected)
        {
            choices.Add(new DialogueChoice(
                "Read the preliminary findings",
                "The report confirms that the material " +
                "recovered from Chamber L-02 includes human " +
                "skeletal remains. Their identity has not " +
                "been established. The condition of the " +
                "surrounding material suggests prolonged " +
                "concealment, but a reliable date and cause " +
                "of death cannot yet be determined.",
                () =>
                {
                    journal.AddEntry(
                        ReportID,
                        "Preliminary Examination of Chamber L-02",
                        "A preliminary official examination " +
                        "confirmed human skeletal remains " +
                        "inside the concealed service void " +
                        "adjoining Chamber L-02. The remains " +
                        "were associated with deteriorated " +
                        "material and sediment. Their identity, " +
                        "date of death, and cause of death " +
                        "remain undetermined. The circumstances " +
                        "of the enclosed location require " +
                        "further investigation.",
                        "Evidence",
                        Source
                    );
                }
            ));
        }
        else
        {
            choices.Add(new DialogueChoice(
                "Review the report",
                "The remains are confirmed to be human. " +
                "Identification and cause of death remain " +
                "undetermined. Further examination is pending."
            ));
        }

        dialogue.ShowDialogue(
            "Preliminary Examination Report",
            requested
                ? "An official report concerning Chamber L-02."
                : "There are no examination findings available.",
            choices
        );
    }
}
