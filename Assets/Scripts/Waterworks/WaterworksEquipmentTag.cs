
using UnityEngine;
using System.Collections.Generic;

public class WaterworksEquipmentTag : Interactable
{
    private const string DiscoveryID =
        "chamber_corroded_equipment_tag";

    private const string MarkingID =
        "chamber_equipment_tag_marking";

    public override void Interact()
    {
        if (DialogueManager.Instance == null ||
            JournalManager.Instance == null)
            return;

        JournalManager journal = JournalManager.Instance;

        List<DialogueChoice> choices =
            new List<DialogueChoice>();

        if (!journal.HasInformation(DiscoveryID))
        {
            choices.Add(new DialogueChoice(
                "Examine the corroded metal object",
                "A small metal tag is caught between " +
                "the drainage grate and the concrete. " +
                "Its surface is badly corroded, " +
                "but it appears to have been stamped " +
                "with an identification number.",
                () =>
                {
                    journal.AddEntry(
                        DiscoveryID,
                        "Corroded Equipment Tag",
                        "A small, corroded metal tag was " +
                        "found lodged beside the drainage " +
                        "channel in Chamber L-02. It bears " +
                        "a partially obscured stamped " +
                        "identification number. Its origin " +
                        "and purpose remain unknown.",
                        "Observation",
                        "Waterworks Chamber L-02"
                    );
                }
            ));
        }
        else if (!journal.HasInformation(MarkingID))
        {
            choices.Add(new DialogueChoice(
                "Inspect the stamped marking",
                "Beneath the corrosion, several " +
                "characters are still visible: " +
                "'GSD' followed by a partially " +
                "legible number. The tag resembles " +
                "an equipment inventory label.",
                () =>
                {
                    journal.AddEntry(
                        MarkingID,
                        "Marking on Corroded Equipment Tag",
                        "Closer examination of the tag " +
                        "reveals the letters GSD and " +
                        "a partially legible inventory " +
                        "number. The marking may indicate " +
                        "a connection to the Grayford " +
                        "Sheriff's Department, but the " +
                        "tag's original assignment " +
                        "has not been established.",
                        "Evidence",
                        "Waterworks Chamber L-02"
                    );
                }
            ));
        }
        else
        {
            choices.Add(new DialogueChoice(
                "Reexamine the equipment tag",
                "The corroded tag bears the letters " +
                "'GSD' and an incomplete inventory " +
                "number. There is nothing further " +
                "you can identify here.",
                null
            ));
        }

        DialogueManager.Instance.ShowDialogue(
            "Corroded Equipment Tag",
            "A small metal object is lodged " +
            "beside the drainage channel.",
            choices
        );
    }
}
