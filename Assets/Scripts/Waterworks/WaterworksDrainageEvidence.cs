
using System.Collections.Generic;
using UnityEngine;

public class WaterworksDrainageEvidence : Interactable
{

    private const string EvidenceID =
        "chamber_trapped_fabric";

    private const string Source =
        "Waterworks Lower Chamber L-02";

    public override void Interact()
    {
        JournalManager journal = JournalManager.Instance;
        DialogueManager dialogue = DialogueManager.Instance;

        if (journal == null || dialogue == null)
            return;

        bool canInspect = true;

        bool alreadyRecorded =
            journal.HasInformation(EvidenceID);

        var choices = new List<DialogueChoice>();

        if (canInspect && !alreadyRecorded)
        {
            choices.Add(new DialogueChoice(
                "Examine the material beneath the grate",
                "A small piece of dark fabric is caught beneath " +
                "the edge of the drainage grate. It is heavily " +
                "discolored and partially embedded in sediment.",
                () => journal.AddEntry(
                    EvidenceID,
                    "Fabric Trapped in Drainage Grate",
                    "A fragment of dark fabric is trapped beneath " +
"a drainage grate in Chamber L-02, partly embedded " +
"in sediment.",
                    "Observation",
                    Source
                )
            ));
        }
        else if (alreadyRecorded)
        {
            choices.Add(new DialogueChoice(
                "Inspect the fabric again",
                "The dark fabric remains caught beneath the grate. " +
                "There is nothing visible that identifies its origin."
            ));
        }
        else
        {
            choices.Add(new DialogueChoice(
                "Inspect the drainage grate",
                "An old metal grate covers part of the drainage " +
                "channel. Sediment has collected along its edges."
            ));
        }

        dialogue.ShowDialogue(
            "Drainage Channel",
            alreadyRecorded
                ? "You've already examined the material beneath the grate."
                : canInspect
                    ? "The drainage channel may contain traces of activity."
                    : "A narrow drainage channel crosses the chamber floor.",
            choices
        );
    }
}
