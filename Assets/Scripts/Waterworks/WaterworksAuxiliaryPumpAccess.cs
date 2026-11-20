
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaterworksAuxiliaryPumpAccess : Interactable
{
    private const string ChamberHypothesisID =
        "sealed_inspection_chamber";

    private const string AccessHypothesisID =
        "possible_secondary_chamber_access";

    private const string ConnectionID =
        "waterworks_auxiliary_pump_connection";

    private const string ClearedID =
        "waterworks_auxiliary_passage_exposed";

    private const string RackInspectedID =
        "waterworks_auxiliary_rack_inspected";

    private const string LatchReleasedID =
        "waterworks_auxiliary_latch_released";

    private const string Source =
        "North County Waterworks";

    [Header("Auxiliary Hatch Destination")]
    [SerializeField]
    private string chamberSceneName =
        "WaterworksLowerChamber_Interior";

    [SerializeField] private string chamberSpawnID = "";

    [Header("Movable Equipment Rack")]
    [SerializeField] private Transform equipmentRack;

    [SerializeField]
    private Vector3 clearedLocalOffset =
        new Vector3(1.5f, 0f, 0f);

    private Vector3 blockedLocalPosition;

    private void Awake()
    {
        if (equipmentRack != null)
            blockedLocalPosition = equipmentRack.localPosition;
    }


    private void Start()
    {
        RefreshRackPosition();
    }

    private void Update()
    {
        RefreshRackPosition();
    }

    private void RefreshRackPosition()
    {
        if (equipmentRack == null)
            return;

        JournalManager journal = JournalManager.Instance;

        if (journal == null)
            return;

        bool passageExposed =
            journal.HasInformation(ClearedID);

        Vector3 targetPosition =
            blockedLocalPosition +
            (passageExposed
                ? clearedLocalOffset
                : Vector3.zero);

        if ((equipmentRack.localPosition - targetPosition)
            .sqrMagnitude > 0.0001f)
        {
            equipmentRack.localPosition = targetPosition;
        }
    }

    public override void Interact()
    {
        JournalManager journal = JournalManager.Instance;
        DialogueManager dialogue = DialogueManager.Instance;

        if (journal == null || dialogue == null)
        {
            Debug.LogWarning(
                "WaterworksAuxiliaryPumpAccess requires " +
                "JournalManager and DialogueManager.",
                this
            );
            return;
        }

        bool hasChamberHypothesis =
            journal.HasHypothesis(ChamberHypothesisID);

        bool canClearRack =
            journal.HasHypothesis(AccessHypothesisID);

        bool hasConnection =
            journal.HasInformation(ConnectionID);

        bool passageExposed =
            journal.HasInformation(ClearedID);

        bool rackInspected =
            journal.HasInformation(RackInspectedID);

        bool latchReleased =
            journal.HasInformation(LatchReleasedID);

        var choices = new List<DialogueChoice>();

        if (hasChamberHypothesis && !hasConnection)
        {
            choices.Add(new DialogueChoice(
                "Examine the auxiliary maintenance opening",
                "Behind the disused pump equipment, a narrow " +
                "maintenance opening continues toward the older " +
                "lower-service infrastructure. Its direction " +
                "appears consistent with the archived floor plan, " +
                "but the far end isn't visible.",
                () => journal.AddEntry(
                    ConnectionID,
                    "Auxiliary Pump Maintenance Connection",
                    "A narrow opening behind the auxiliary pump " +
                    "equipment extends toward the older " +
                    "lower-service area, possibly leading to " +
                    "Chamber L-02.",
                    "Observation",
                    Source
                )
            ));
        }

        if (canClearRack && !passageExposed)
        {
            if (!rackInspected)
            {
                choices.Add(new DialogueChoice(
                    "Inspect the equipment blocking the hatch",
                    "A disused pump-equipment rack sits across " +
                    "the opening. Its frame runs on short floor " +
                    "guides, but a retaining latch holds it in " +
                    "place. The rack looks movable once the " +
                    "latch is released.",
                    () => journal.AddEntry(
                        RackInspectedID,
                        "Auxiliary Equipment Rack Inspected",
                        "I found that the equipment rack sits " +
                        "on floor guides and is held by a latch.",
                        "Event",
                        Source
                    )
                ));
            }
            else if (!latchReleased)
            {
                choices.Add(new DialogueChoice(
                    "Release the rack's retaining latch",
                    "The retaining latch lifts free. The rack " +
                    "is no longer secured to the floor guides " +
                    "and can be shifted aside.",
                    () => journal.AddEntry(
                        LatchReleasedID,
                        "Auxiliary Rack Latch Released",
                        "I released the retaining latch on the " +
                        "auxiliary equipment rack.",
                        "Event",
                        Source
                    )
                ));
            }
            else
            {
                choices.Add(new DialogueChoice(
                    "Move the equipment rack aside",
                    "You slide the rack along its guides, " +
                    "exposing the maintenance opening. A narrow " +
                    "passage continues into darkness toward the " +
                    "older lower-service area. Its far end is " +
                    "still out of sight.",
                    () =>
                    {
                        journal.AddEntry(
                            ClearedID,
                            "Exposed Auxiliary Maintenance Passage",
                            "Moving the equipment rack exposed a " +
                            "narrow maintenance passage toward " +
                            "the older lower-service area.",
                            "Observation",
                            Source
                        );

                        RefreshRackPosition();
                    }
                ));
            }
        }

        if (passageExposed)
        {
            choices.Add(new DialogueChoice(
                "Enter the exposed maintenance passage",
                "You make your way through the narrow opening " +
                "toward the old inspection chamber.",
                EnterChamber
            ));
        }

        if (choices.Count == 0)
        {
            string response = canClearRack
                ? "The auxiliary opening is obstructed by a " +
                  "disused equipment rack."
                : hasChamberHypothesis
                    ? "The opening may connect to the older " +
                      "lower-service infrastructure, but " +
                      "equipment blocks the route."
                    : "An old auxiliary pump access hatch. " +
                      "Disused equipment is visible beyond it.";

            choices.Add(new DialogueChoice(
                "Inspect the access hatch",
                response
            ));
        }

        dialogue.ShowDialogue(
            "Auxiliary Pump Access",
            passageExposed
                ? "The equipment rack is clear of the opening. " +
                  "A narrow passage is accessible."
                : canClearRack
                    ? "The auxiliary opening may provide " +
                      "another route toward Chamber L-02."
                    : hasChamberHypothesis
                        ? "The archived plans suggest this area " +
                          "may connect to the lower inspection " +
                          "chamber."
                        : "A narrow maintenance hatch leads " +
                          "toward auxiliary pump equipment.",
            choices
        );
    }

    private void EnterChamber()
    {
        JournalManager journal = JournalManager.Instance;

        if (journal == null ||
            !journal.HasInformation(ClearedID))
        {
            Debug.LogWarning(
                "Cannot enter the chamber until the " +
                "auxiliary passage is exposed.",
                this
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(chamberSceneName))
        {
            Debug.LogError(
                "No chamber scene configured.",
                this
            );
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(
            chamberSceneName))
        {
            Debug.LogError(
                "Chamber scene not found in build list: " +
                chamberSceneName,
                this
            );
            return;
        }

        SceneSpawnManager.SetNextSpawn(chamberSpawnID);
        SceneManager.LoadScene(chamberSceneName);
    }
}
