
using System.Collections.Generic;
using UnityEngine;

public class WaterworksFloorRecess : Interactable
{
    private const string RequiredInformation =
    "chamber_disturbed_floor";

    private const string FloorEvidenceID =
        "chamber_disturbed_floor";

    private const string RecessEvidenceID =
        "chamber_concealed_floor_recess";

    private const string Source =
        "Waterworks Lower Chamber L-02";

    [Header("Visual State")]
    [SerializeField] private GameObject closedCover;
    [SerializeField] private GameObject openRecess;

    private static bool edgesInspected;
    private static bool retainingPointsInspected;

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

        bool hasFloorObservation =
            journal.HasInformation(FloorEvidenceID);

        bool canInvestigate =
    journal.HasInformation(RequiredInformation);

        bool recessOpened =
            journal.HasInformation(RecessEvidenceID);

        var choices = new List<DialogueChoice>();

        // Preserve the original floor observation.
        if (!hasFloorObservation)
        {
            choices.Add(new DialogueChoice(
                "Examine the disturbed floor",
                "Dust and sediment have accumulated unevenly " +
                "around a section of the concrete floor.",
                () => journal.AddEntry(
                    FloorEvidenceID,
                    "Disturbed Chamber Floor",
                   "Dust and sediment are unevenly distributed " +
"around part of Chamber L-02's floor.",
                    "Observation",
                    Source
                )
            ));
        }

        if (canInvestigate && !recessOpened)
        {
            if (!edgesInspected)
            {
                choices.Add(new DialogueChoice(
                    "Examine the edges of the disturbed area",
                    "Beneath the sediment, a narrow seam runs " +
                    "around a rectangular section of the floor. " +
                    "It appears to be a fitted cover rather " +
                    "than a patch of poured concrete.",
                    () => edgesInspected = true
                ));
            }
            else if (!retainingPointsInspected)
            {
                choices.Add(new DialogueChoice(
                    "Inspect the cover's retaining points",
                    "Two recessed lifting points are visible " +
                    "near the edge. The cover appears to be " +
                    "a removable inspection panel.",
                    () => retainingPointsInspected = true
                ));
            }
            else
            {
                choices.Add(new DialogueChoice(
                    "Lift the inspection cover",
                    "The heavy panel shifts aside, exposing " +
                    "a shallow utility recess. Old sediment " +
                    "has accumulated inside. Beneath it lies " +
                    "a deteriorated piece of dark material, " +
                    "partly concealed by debris.",
                    () =>
                    {
                        journal.AddEntry(
                            RecessEvidenceID,
                            "Concealed Floor Recess",
                            "A removable floor panel conceals a shallow recess " +
"in Chamber L-02. Debris and deteriorated material " +
"lie beneath the cover.",
                            "Observation",
                            Source
                        );

                        RefreshVisuals();
                    }
                ));
            }
        }

        if (recessOpened)
        {
            choices.Add(new DialogueChoice(
                "Look into the exposed recess",
                "The inspection cover has been moved aside. " +
                "The deteriorated material remains partially " +
                "buried beneath sediment."
            ));
        }

        if (choices.Count == 0)
        {
            choices.Add(new DialogueChoice(
                "Inspect the floor",
                "The floor shows an unusual accumulation " +
                "of dust and sediment."
            ));
        }

        dialogue.ShowDialogue(
            "Disturbed Floor Area",
            recessOpened
                ? "An old utility recess is exposed beneath the floor."
                : canInvestigate
                    ? "The disturbed area may deserve closer examination."
                    : "A section of the floor looks different from its surroundings.",
            choices
        );
    }

    private void RefreshVisuals()
    {
        JournalManager journal = JournalManager.Instance;

        bool opened = journal != null &&
            journal.HasInformation(RecessEvidenceID);

        if (closedCover != null)
            closedCover.SetActive(!opened);

        if (openRecess != null)
            openRecess.SetActive(opened);
    }
}
