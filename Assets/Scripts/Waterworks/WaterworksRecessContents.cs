
using System.Collections.Generic;
using UnityEngine;

public class WaterworksRecessContents : Interactable
{
    private const string RequiredInformationID =
        "chamber_concealed_floor_recess";

    private const string ClothingID =
        "chamber_recess_clothing";

    private const string PersonalEffectID =
        "chamber_recess_personal_effect";

    private const string TagHypothesisID =
        "possible_daniel_metal_tag";

    private const string MarkingID =
        "chamber_metal_tag_marking";

    private const string Source =
        "Waterworks Lower Chamber L-02";

    public override void Interact()
    {
        JournalManager journal = JournalManager.Instance;
        DialogueManager dialogue = DialogueManager.Instance;

        if (journal == null || dialogue == null)
            return;

        bool recessOpen =
            journal.HasInformation(RequiredInformationID);

        bool clothingFound =
            journal.HasInformation(ClothingID);

        bool personalEffectFound =
            journal.HasInformation(PersonalEffectID);

        bool hasTagHypothesis =
            journal.HasHypothesis(TagHypothesisID);

        bool markingFound =
            journal.HasInformation(MarkingID);

        var choices = new List<DialogueChoice>();

        if (!recessOpen)
        {
            choices.Add(new DialogueChoice(
                "Inspect the floor",
                "The inspection cover is still in place."
            ));
        }
        else if (!clothingFound)
        {
            choices.Add(new DialogueChoice(
                "Examine the deteriorated material",
                "The material appears to be woven fabric, " +
                "folded and compressed beneath sediment. " +
                "It may be part of an old garment.",
                () => journal.AddEntry(
                    ClothingID,
                    "Clothing Material in Floor Recess",
                    "Deteriorated woven fabric was found " +
                    "inside the floor recess of Chamber L-02. " +
                    "It may be part of a garment, but its " +
                    "original form and owner remain unknown.",
                    "Observation",
                    Source
                )
            ));
        }
        else if (!personalEffectFound)
        {
            choices.Add(new DialogueChoice(
                "Inspect beneath the fabric",
                "A small corroded metal object is partially " +
                "visible beneath the material. Its surface " +
                "bears an indistinct marking.",
                () => journal.AddEntry(
                    PersonalEffectID,
                    "Personal Effect in Floor Recess",
                    "A small corroded metal object was found " +
                    "beneath deteriorated clothing material " +
                    "in Chamber L-02. Its original purpose " +
                    "and ownership remain unknown.",
                    "Evidence",
                    Source
                )
            ));
        }
        else
        {
            if (hasTagHypothesis && !markingFound)
            {
                choices.Add(new DialogueChoice(
                    "Examine the object's engraved marking",
                    "With Evelyn's description in mind, " +
                    "you inspect the metal surface more " +
                    "closely. Beneath the corrosion, three " +
                    "characters appear to read '184'. " +
                    "The remaining markings are illegible.",
                    () => journal.AddEntry(
                        MarkingID,
                        "Partial Marking on Metal Tag",
                        "Closer examination of the corroded " +
                        "metal object revealed three apparently " +
                        "legible digits: 184. Other markings " +
                        "remain obscured. The object's origin " +
                        "and connection to Daniel Mercer " +
                        "have not been established.",
                        "Observation",
                        Source
                    )
                ));
            }

            choices.Add(new DialogueChoice(
                "Reexamine the recess contents",
                markingFound
                    ? "The metal object's partial marking " +
                      "appears to read 184. The deteriorated " +
                      "fabric remains unidentified."
                    : "The deteriorated clothing and " +
                      "corroded metal object remain in " +
                      "the recess."
            ));
        }

        dialogue.ShowDialogue(
            "Floor Recess Contents",
            !recessOpen
                ? "The recess remains concealed."
                : markingFound
                    ? "You've documented the object's visible marking."
                    : hasTagHypothesis && personalEffectFound
                        ? "Evelyn's recollection may help you examine the object."
                        : "The recess contains deteriorated material and debris.",
            choices
        );
    }
}
