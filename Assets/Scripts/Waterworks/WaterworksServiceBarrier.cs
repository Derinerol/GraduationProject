using System.Collections.Generic;
using UnityEngine;

public class WaterworksServiceBarrier : Interactable
{
    private const string BarrierID = "waterworks_service_barrier";
    private const string HypothesisID = "unverified_lower_access_maintenance";
    private const string FastenersID = "waterworks_barrier_fasteners";
    private const string RecordGapID = "waterworks_barrier_record_gap";
    private const string DetailedInspectionHypothesisID = "frank_lower_access_familiarity";
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
                "A heavy metal barrier closes off an older section of the lower service passage. Its surface shows corrosion and several replacement fasteners. No maintenance notice or closure date is visible. The barrier appears intended to prevent access, but its installation date and purpose are unknown.",
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
                    "The sealed service barrier has replacement fasteners and scratches around several mounting points. These suggest the panel has been adjusted or removed previously, but the visible condition does not establish when or why.",
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
                    "WW-91-184 references drainage and sealing work near the lower access, but the available records do not identify this particular barrier or explain its installation. Its connection to the assignment remains uncertain.",
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
                    "A closer inspection of the lower service barrier reveals an older access door behind it. The original hinges, frame, and locking mechanism appear intact, although their functionality cannot be confirmed without opening the entrance. The newer barrier obstructs this existing access arrangement rather than visibly replacing a damaged doorway.",
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
                ? "Frank's remarks draw your attention back to the lower service entrance. You inspect the edges of the barrier more carefully."
                : hasHypothesis
                    ? "The markings and municipal records raise questions about the work performed here. You examine the sealed passage again."
                    : "A heavy metal barrier blocks the older service passage. Its closure date is not marked.",
            choices
        );
    }
}
