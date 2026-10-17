
using UnityEngine;
using System.Collections.Generic;

public class Interactable : MonoBehaviour
{
    public enum InformationType
    {
        Observation,
        Claim
    }

    [Header("General")]
    [SerializeField] private string interactionName = "Resident";
    [SerializeField] private bool isInformationSource = false;

    [Header("NPC Dialogue")]
    [TextArea(2, 5)]
    [SerializeField]
    private string openingLine =
        "It's been a quiet day around here.";

    [SerializeField] private string choice1 = "Ask about Daniel";

    [TextArea(2, 5)]
    [SerializeField]
    private string response1 =
        "Daniel disappeared years ago.";

    [SerializeField] private string choice2 = "Ask about the town";

    [TextArea(2, 5)]
    [SerializeField]
    private string response2 =
        "Nothing much changes around here.";

    [SerializeField]
    private string followUpChoice =
    "Ask why the original account was accepted";

    [TextArea(2, 5)]
    [SerializeField]
    private string followUpResponse =
        "At the time, people accepted the explanation. " +
        "There wasn't much reason to question it.";

    [Header("Information")]
    [SerializeField] private string informationID = "daniel_newspaper";

    [SerializeField]
    private string informationTitle =
        "Newspaper clipping";

    [SerializeField] private string informationSource = "Newspaper clipping";

    [TextArea(3, 8)]
    [SerializeField]
    private string informationDescription =
        "An old article reports that Daniel disappeared seven years ago.";

    private bool hasAskedAboutDaniel = false;

    [SerializeField]
    private InformationType informationType =
        InformationType.Observation;

    public string InteractionName => interactionName;

    public bool IsInformationSource => isInformationSource;

    private bool knowsAboutNewspaper = false;


    public virtual void Interact()
    {
        // Information sources still use the existing journal system.
        if (isInformationSource)
        {
            if (JournalManager.Instance == null)
            {
                Debug.LogError("JournalManager was not found.");
                return;
            }

            JournalManager.Instance.AddEntry(
                informationID,
                informationTitle,
                informationDescription,
                informationType.ToString(),
                informationSource
            );

            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager was not found.");
            return;
        }

        // Check the player's discoveries and formed hypotheses.
        bool playerHasNewspaper =
            JournalManager.Instance != null &&
            JournalManager.Instance.HasInformation("daniel_newspaper");

        bool hasConnectionHypothesis =
            JournalManager.Instance != null &&
            JournalManager.Instance.HasHypothesis(
                "newspaper_resident_connection"
            );

        bool knowsAboutDaniel = playerHasNewspaper;

        bool canTellResidentAboutNewspaper =
            playerHasNewspaper && !knowsAboutNewspaper;

        // Choose an opening line based on the resident's knowledge.
        string currentOpeningLine;

        if (knowsAboutNewspaper)
        {
            currentOpeningLine = knowsAboutNewspaper
                ? "You mentioned that old newspaper clipping. Is there something else?"
                : "We talked about Daniel already. What else do you want to know?";
        }
        else if (hasAskedAboutDaniel)
        {
            currentOpeningLine =
                "We talked about Daniel already. What else do you want to know?";
        }
        else
        {
            currentOpeningLine = openingLine;
        }

        // Create the list of available dialogue choices.
        List<DialogueChoice> choices = new List<DialogueChoice>();

        // 1. Ask about the town — always available.
        choices.Add(new DialogueChoice(
            choice2,
            response2
        ));

        // 2. Ask about Daniel — available after finding the newspaper.
        if (knowsAboutDaniel)
        {
            string danielResponse = hasAskedAboutDaniel
                ? "As I told you before, Daniel disappeared years ago. " +
                  "I haven't learned anything new since we last spoke."
                : response1;

            choices.Add(new DialogueChoice(
                choice1,
                danielResponse,
                () =>
                {
                    if (!hasAskedAboutDaniel)
                    {
                        hasAskedAboutDaniel = true;

                        if (JournalManager.Instance != null)
                        {
                            JournalManager.Instance.RecordClaim(
                                "resident_daniel_statement",
                                "Resident — Daniel's disappearance",
                                "The resident states: " + response1,
                                "Resident"
                            );
                        }
                    }
                }
            ));
        }

        // 3. Tell the resident about the newspaper.
        if (canTellResidentAboutNewspaper)
        {
            choices.Add(new DialogueChoice(
                "Tell the resident about the newspaper",
                "I see. I hadn't heard about that clipping. " +
                "I'll keep it in mind.",
                () =>
                {
                    knowsAboutNewspaper = true;
                }
            ));
        }

        // 4. Ask the hypothesis-dependent follow-up.
        if (hasConnectionHypothesis)
        {
            choices.Add(new DialogueChoice(
                followUpChoice,
                followUpResponse
            ));
        }

        DialogueManager.Instance.ShowDialogue(
            interactionName,
            currentOpeningLine,
            choices
        );
    }
}