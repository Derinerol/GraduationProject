
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
                        "A damaged inspection lamp was found in Chamber L-02's " +
                        "drainage assembly. Its inventory number is GSD-2714.",
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
                        "The lamp's inventory number, GSD-2714, matches one " +
                        "assigned to Deputy Dwyer in October 1991. " +
                        "It was never recorded as returned.",
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
