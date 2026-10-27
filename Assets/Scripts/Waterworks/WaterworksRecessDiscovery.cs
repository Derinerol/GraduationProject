
using System.Collections.Generic;
using UnityEngine;

public class WaterworksRecessDiscovery : Interactable
{
    private const string RequiredInformationID =
    "chamber_recess_clothing";

    private const string RecessOpenID =
        "chamber_concealed_floor_recess";

    private const string DiscoveryID =
        "chamber_possible_human_remains";

    private const string Source =
        "Waterworks Lower Chamber L-02";

    [Header("Discovery Visuals")]
    [SerializeField] private GameObject concealedCavity;
    [SerializeField] private GameObject exposedCavity;

    private static bool boundaryInspected;
    private static bool cavityInspected;

    private void Start()
    {
        RefreshVisuals();
    }

    public override void Interact()
    {
        JournalManager journal = JournalManager.Instance;
        DialogueManager dialogue = DialogueManager.Instance;

        if (journal == null || dialogue == null)
            return;

        bool recessOpen =
            journal.HasInformation(RecessOpenID);

        bool canInvestigate =
    journal.HasInformation(RequiredInformationID);

        bool remainsDiscovered =
            journal.HasInformation(DiscoveryID);

        var choices = new List<DialogueChoice>();

        if (!recessOpen)
        {
            choices.Add(new DialogueChoice(
                "Inspect the floor",
                "The inspection cover is still in place."
            ));
        }
        else if (!canInvestigate && !remainsDiscovered)
        {
            choices.Add(new DialogueChoice(
                "Inspect the rear of the recess",
                "Sediment and debris obscure the rear boundary. " +
                "Nothing more can be determined from here."
            ));
        }
        else if (!remainsDiscovered)
        {
            if (!boundaryInspected)
            {
                choices.Add(new DialogueChoice(
                    "Examine the rear boundary",
                    "The concrete edge doesn't meet the floor " +
                    "beneath it. A narrow gap extends farther " +
                    "back into the old service infrastructure.",
                    () => boundaryInspected = true
                ));
            }
            else if (!cavityInspected)
            {
                choices.Add(new DialogueChoice(
                    "Look into the deeper cavity",
                    "Beyond the sediment is a narrow service " +
                    "void. Something pale and irregular is " +
                    "partially visible among the debris.",
                    () => cavityInspected = true
                ));
            }
            else
            {
                choices.Add(new DialogueChoice(
                    "Document the objects in the cavity",
                    "From the opening, you can distinguish " +
                    "several shapes resembling human bones. " +
                    "The surrounding material appears heavily " +
                    "degraded. Their identity and age cannot " +
                    "be established by visual inspection.",
                    () =>
                    {
                        journal.AddEntry(
                            DiscoveryID,
                            "Possible Human Remains in Chamber L-02",
                            "Objects resembling human bones were found in a " +
"concealed service void adjoining Chamber L-02's " +
"floor recess. Their identity is unknown.",
                            "Observation",
                            Source
                        );

                        RefreshVisuals();
                    }
                ));
            }
        }
        else
        {
            choices.Add(new DialogueChoice(
                "Reexamine the cavity",
                "The suspected remains are still visible " +
                "within the service void. A formal examination " +
                "will be necessary to establish their identity."
            ));
        }

        dialogue.ShowDialogue(
            "Deeper Service Cavity",
            remainsDiscovered
                ? "The discovery needs independent examination."
                : canInvestigate
                    ? "The rear of the recess may conceal more than debris."
                    : "The recess extends toward an older section of the chamber.",
            choices
        );
    }

    private void RefreshVisuals()
    {
        JournalManager journal = JournalManager.Instance;

        bool discovered = journal != null &&
            journal.HasInformation(DiscoveryID);

        if (concealedCavity != null)
            concealedCavity.SetActive(!discovered);

        if (exposedCavity != null)
            exposedCavity.SetActive(discovered);
    }
}
