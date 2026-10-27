using System.Collections.Generic;
using UnityEngine;

public class WaterworksServiceBarrier : Interactable
{
    private const string BarrierID = "waterworks_service_barrier";
    private const string HypothesisID = "unverified_lower_access_maintenance";
    private const string FastenersID = "waterworks_barrier_fasteners";
    private const string RecordGapID = "waterworks_barrier_record_gap";
    private const string DetailedInspectionHypothesisID = "changed_service_access";
    private const string OriginalAccessID = "waterworks_original_access_mechanism";
    private const string Source = "North County Waterworks";

    public override void Interact()
    {
        JournalManager journal = JournalManager.Instance;
        DialogueManager dialogue = DialogueManager.Instance;

        if (journal == null || dialogue == null)
        {
            Debug.LogWarning("WaterworksServiceBarrier requires JournalManager and DialogueManager.");
            return;
        }

        if (!journal.HasInformation(BarrierID))
        {
            journal.AddEntry(
                BarrierID,
                "Sealed Lower Service Passage",
                "A metal barrier blocks the lower service passage. " +
"Its fasteners show signs of replacement, but no " +
"closure date is visible.",
                "Observation",
                Source
            );
        }

        var choices = new List<DialogueChoice>();
        bool hasHypothesis = journal.HasHypothesis(HypothesisID);
        bool canInspectOriginalAccess = journal.HasHypothesis(DetailedInspectionHypothesisID);

        if (hasHypothesis && !journal.HasInformation(FastenersID))
        {
            choices.Add(new DialogueChoice(
                "Examine the barrier's fasteners",
                "Several fasteners appear to have been replaced at different times. Scratches around the mounting points suggest the panel has been removed or adjusted before. Nothing visible establishes when those changes occurred.",
                () => journal.AddEntry(
                    FastenersID,
                    "Service Barrier Fasteners",
                   "Several barrier fasteners have been replaced, " +
"with scratches around their mounting points.",
                    "Observation",
                    Source
                )
            ));
        }

        if (hasHypothesis && !journal.HasInformation(RecordGapID))
        {
            choices.Add(new DialogueChoice(
                "Review the barrier's relation to WW-91-184",
                "The work order concerns drainage inspection, clearance, and sealing near the lower access. The documents collected so far do not specify this barrier, describe its installation, or establish whether it formed part of the authorized work.",
                () => journal.AddEntry(
                    RecordGapID,
                    "Undocumented Service Barrier",
                   "WW-91-184 references drainage and sealing near " +
"the lower access but does not identify this barrier.",
                    "Observation",
                    Source
                )
            ));
        }

        if (canInspectOriginalAccess && !journal.HasInformation(OriginalAccessID))
        {
            choices.Add(new DialogueChoice(
                "Examine the original doorway behind the barrier",
                "Looking along the edges of the newer barrier, you can make out an older access door behind it. Its hinges, frame, and locking mechanism appear intact, although you cannot tell whether the door still works without opening the entrance.",
                () => journal.AddEntry(
                    OriginalAccessID,
                    "Obstructed Original Access",
                    "An older doorway remains behind the newer barrier. " +
"Its hinges, frame, and locking mechanism appear intact.",
                    "Observation",
                    Source
                )
            ));
        }

        if (choices.Count == 0)
        {
            choices.Add(new DialogueChoice(
                "Look over the sealed passage",
                canInspectOriginalAccess
                    ? "You've documented the older doorway and what can be seen of its mechanism. The barrier still prevents access to the passage."
                    : hasHypothesis
                        ? "You've documented what can be established from the barrier and the available records. The passage remains inaccessible."
                        : "The passage is sealed. Its surface shows wear and replacement fasteners, but you have no reason yet to connect it to a particular maintenance assignment."
            ));
        }

        dialogue.ShowDialogue(
            "Sealed Service Barrier",
            canInspectOriginalAccess
                ? "Caleb's recollection suggests the entrance once looked different. You examine the barrier for signs of the original access."
                : hasHypothesis
                    ? "The markings and municipal records raise questions about the work performed here. You examine the sealed passage again."
                    : "A heavy metal barrier blocks the older service passage. Its closure date is not marked.",
            choices
        );
    }
}
