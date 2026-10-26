
using UnityEngine;
using System.Collections.Generic;

public class DamagedInspectionLamp : Interactable
{
    private const string RequiredID =
        "chamber_equipment_tag_marking";

    private const string DiscoveryID =
        "chamber_damaged_inspection_lamp";

    private const string IdentificationID =
        "dwyer_lamp_identified";

    public override void Interact()
    {
        if (DialogueManager.Instance == null ||
            JournalManager.Instance == null)
            return;

        JournalManager journal = JournalManager.Instance;
        List<DialogueChoice> choices =
            new List<DialogueChoice>();

        if (!journal.HasInformation(RequiredID))
        {
            DialogueManager.Instance.ShowDialogue(
                "Drainage Assembly",
                "The drainage channel is clogged with " +
                "old debris. Something metallic is " +
                "wedged farther inside, but you cannot " +
                "get a clear look at it.",
                new List<DialogueChoice>
                {
                    new DialogueChoice(
                        "Leave it alone",
                        "You leave the debris undisturbed.",
                        null)
                });

            return;
        }

        if (!journal.HasInformation(DiscoveryID))
        {
            choices.Add(new DialogueChoice(
                "Examine the object behind the drainage grate",
                "Behind the corroded tag is the damaged " +
                "body of a portable inspection lamp. " +
                "Its casing is dented and partly " +
                "collapsed. A stamped inventory number " +
                "is visible beneath the handle: GSD-2714.",
                () =>
                {
                    journal.AddEntry(
                        DiscoveryID,
                        "Damaged Inspection Lamp",
                        "A damaged portable inspection lamp " +
                        "was found lodged inside the drainage " +
                        "assembly of Chamber L-02. Its casing " +
                        "bears the inventory number GSD-2714.",
                        "Evidence",
                        "Waterworks Chamber L-02");
                }
            ));
        }
        else if (
            journal.HasInformation(
                "dwyer_equipment_assignment_register") &&
            !journal.HasInformation(IdentificationID))
        {
            choices.Add(new DialogueChoice(
                "Compare the lamp with the equipment register",
                "The inventory number matches the " +
                "inspection lamp assigned to Deputy " +
                "Michael Dwyer in October 1991. " +
                "The register lists that lamp as " +
                "unreturned.",
                () =>
                {
                    journal.AddEntry(
                        IdentificationID,
                        "Dwyer's Inspection Lamp Identified",
                        "The damaged inspection lamp found " +
                        "inside Chamber L-02 bears inventory " +
                        "number GSD-2714. The October 1991 " +
                        "sheriff's department equipment " +
                        "register identifies this lamp as " +
                        "assigned to Deputy Michael Dwyer " +
                        "and subsequently unreturned. " +
                        "The match establishes the identity " +
                        "of the equipment, but not who " +
                        "left it in the chamber.",
                        "Evidence",
                        "Chamber L-02 / Sheriff's Equipment Register");
                }
            ));
        }
        else
        {
            choices.Add(new DialogueChoice(
                "Inspect the damaged lamp",
                "The lamp is badly damaged. Its " +
                "inventory number, GSD-2714, " +
                "remains legible.",
                null));
        }

        DialogueManager.Instance.ShowDialogue(
            "Damaged Inspection Lamp",
            "An old inspection lamp is trapped " +
            "inside the drainage assembly.",
            choices);
    }
}
