
using System.Collections.Generic;
using UnityEngine;

public class WaterworksChamberEvidence : Interactable
{
    public enum EvidenceType
    {
        DisturbedFloor,
        MaintenanceCabinet,
        PumpEquipment
    }

    [Header("Chamber Evidence")]
    [SerializeField] private EvidenceType evidenceType;

    private const string Source = "Waterworks Lower Chamber L-02";

    public override void Interact()
    {
        JournalManager journal = JournalManager.Instance;
        DialogueManager dialogue = DialogueManager.Instance;

        if (journal == null || dialogue == null)
            return;

        string evidenceID;
        string title;
        string description;
        string inspectionText;

        if (evidenceType == EvidenceType.DisturbedFloor)
        {
            evidenceID = "chamber_disturbed_floor";
            title = "Disturbed Chamber Floor";

            inspectionText =
                "A section of the concrete floor is noticeably different " +
                "from the surrounding surface. Dust and sediment have " +
                "collected unevenly around its edges.";

            description =
                "A section of the floor in Chamber L-02 shows an uneven " +
                "distribution of dust and sediment compared with the " +
                "surrounding concrete. The difference may indicate " +
                "maintenance activity or disturbance after the chamber " +
                "was originally constructed, but its cause and date " +
                "cannot be established from visual inspection alone.";
        }

        else if (evidenceType == EvidenceType.MaintenanceCabinet)
        {
            evidenceID = "chamber_maintenance_cabinet";
            title = "Chamber Maintenance Cabinet";

            inspectionText =
                "The cabinet contains old maintenance supplies. " +
                "Several compartments are empty, and dust has accumulated " +
                "unevenly across the shelves.";

            description =
                "The maintenance cabinet in Chamber L-02 contains " +
                "old supplies and partially emptied compartments. " +
                "The uneven dust accumulation suggests the contents " +
                "may have been handled at different times, although " +
                "there is no reliable indication of when or by whom.";
        }
        else
        {
            evidenceID = "chamber_pump_cover_marks";
            title = "Marks on Pump Maintenance Cover";

            inspectionText =
                "The pump's maintenance cover is coated in dust, " +
                "but the metal around several fasteners is visibly " +
                "scratched. The cover does not sit entirely flush " +
                "against its housing.";

            description =
                "The maintenance cover on the old pump assembly " +
                "shows scratches around its fasteners and a slight " +
                "misalignment with the housing. These features are " +
                "consistent with the cover having been removed or " +
                "adjusted, although the timing and purpose of any " +
                "such work are unknown.";
        }


        bool alreadyRecorded = journal.HasInformation(evidenceID);

        var choices = new List<DialogueChoice>();

        if (!alreadyRecorded)
        {
            choices.Add(new DialogueChoice(
                "Examine carefully",
                inspectionText,
                () => journal.AddEntry(
                    evidenceID,
                    title,
                    description,
                    "Observation",
                    Source
                )
            ));
        }
        else
        {
            choices.Add(new DialogueChoice(
                "Inspect again",
                inspectionText
            ));
        }

        dialogue.ShowDialogue(
            evidenceType == EvidenceType.DisturbedFloor
                ? "Chamber Floor"
                : "Maintenance Cabinet",
            alreadyRecorded
                ? "You've already recorded your observations here."
                : "Something about this area warrants closer inspection.",
            choices
        );
    }
}
