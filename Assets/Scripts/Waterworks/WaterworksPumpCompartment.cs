
using System.Collections.Generic;
using UnityEngine;

public class WaterworksPumpCompartment : Interactable
{
    private const string RequiredHypothesis =
        "unexplained_chamber_disturbance";

    private const string CoverEvidenceID =
        "chamber_pump_cover_marks";

    private const string EnvelopeEvidenceID =
        "chamber_concealed_access_note";

    private const string Source =
        "Waterworks Lower Chamber L-02";

    private static bool coverInspected;
    private static bool fastenerReleased;

    public override void Interact()
    {
        JournalManager journal = JournalManager.Instance;
        DialogueManager dialogue = DialogueManager.Instance;

        if (journal == null || dialogue == null)
            return;

        bool hasCoverObservation =
            journal.HasInformation(CoverEvidenceID);

        bool canOpen =
            journal.HasHypothesis(RequiredHypothesis);

        bool hasEnvelope =
            journal.HasInformation(EnvelopeEvidenceID);

        var choices = new List<DialogueChoice>();

        // Preserve the existing pump observation.
        if (!hasCoverObservation)
        {
            choices.Add(new DialogueChoice(
                "Examine the pump maintenance cover",
                "The cover is coated in dust, but the metal " +
                "around several fasteners is scratched. " +
                "It doesn't sit entirely flush.",
                () => journal.AddEntry(
                    CoverEvidenceID,
                    "Marks on Pump Maintenance Cover",
                    "The old pump's maintenance cover has " +
                    "scratches around its fasteners and a slight " +
                    "misalignment with the housing. These marks " +
                    "are consistent with the cover having been " +
                    "removed or adjusted, but their age and " +
                    "purpose are unknown.",
                    "Observation",
                    Source
                )
            ));
        }

        if (canOpen && !hasEnvelope)
        {
            if (!coverInspected)
            {
                choices.Add(new DialogueChoice(
                    "Inspect the cover's retaining mechanism",
                    "Most fasteners are loose or missing. " +
                    "One retaining fastener still holds the " +
                    "cover in place.",
                    () => coverInspected = true
                ));
            }
            else if (!fastenerReleased)
            {
                choices.Add(new DialogueChoice(
                    "Release the remaining fastener",
                    "The fastener turns with some resistance. " +
                    "The maintenance cover is now free.",
                    () => fastenerReleased = true
                ));
            }
            else
            {
                choices.Add(new DialogueChoice(
                    "Remove the cover and inspect inside",
                    "Behind the cover is a shallow service " +
                    "compartment. A deteriorated paper envelope " +
                    "has been pushed behind a section of piping. " +
                    "Inside is a folded maintenance note dated " +
                    "October 17, 1991.",
                    () => journal.AddEntry(
                        EnvelopeEvidenceID,
                        "Concealed October 1991 Access Note",
                        "A deteriorated envelope was found behind " +
                        "the pump maintenance cover in Chamber L-02. " +
                        "It contains a folded note dated October " +
                        "17, 1991, referring to an evening access " +
                        "request for the lower service area. " +
                        "The requester is not identified, and " +
                        "the note does not confirm that the " +
                        "requested access actually occurred.",
                        "Evidence",
                        Source
                    )
                ));
            }
        }

        if (hasEnvelope)
        {
            choices.Add(new DialogueChoice(
                "Reexamine the service compartment",
                "The shallow compartment is otherwise empty. " +
                "The note's date and reference to evening " +
                "access remain its most significant details."
            ));
        }

        if (choices.Count == 0)
        {
            choices.Add(new DialogueChoice(
                "Inspect the old pump",
                "The machinery is inactive. Its maintenance " +
                "cover shows signs of previous handling."
            ));
        }

        dialogue.ShowDialogue(
            "Old Pump Equipment",
            hasEnvelope
                ? "The maintenance compartment has been examined."
                : canOpen
                    ? "The cover may conceal a small service space."
                    : "An old pump assembly stands against the wall.",
            choices
        );
    }
}
