
using UnityEngine;
using System.Collections.Generic;

public class Interactable : MonoBehaviour
{
    public enum InformationType
    {
        Observation,
        Claim,
        Evidence,
        Interpretation
    }

    public enum NPCProfile
    {
        GenericResident,
        EvelynShaw
    }

    [Header("General")]
    [SerializeField] private string interactionName = "Resident";
    [SerializeField] private bool isInformationSource = false;

    [Header("NPC Profile")]
    [SerializeField]
    private NPCProfile npcProfile =
        NPCProfile.GenericResident;

    [Header("Generic NPC Dialogue")]
    [TextArea(2, 5)]
    [SerializeField]
    private string openingLine =
        "It's been a quiet day around here.";

    [SerializeField] private string choice1 = "Ask about Daniel";

    [TextArea(2, 5)]
    [SerializeField]
    private string response1 =
        "Daniel Mercer disappeared seven years ago.";

    [SerializeField] private string choice2 = "Ask about the town";

    [TextArea(2, 5)]
    [SerializeField]
    private string response2 =
        "Nothing much changes around here.";

    [Header("Information Source")]
    [SerializeField]
    private string informationID =
        "anonymous_package";

    [SerializeField]
    private string informationTitle =
        "Anonymous package";

    [SerializeField]
    private string informationSource =
        "Anonymous package";

    [TextArea(3, 8)]
    [SerializeField]
    private string informationDescription =
        "An anonymous package contains a photocopy of a dated " +
        "maintenance record, a fragment of a work order, and a note: " +
        "\"They didn't look in the right place.\" The documents raise " +
        "questions about the accepted account of Daniel Mercer's " +
        "disappearance, but do not establish what happened to him.";

    [SerializeField]
    private InformationType informationType =
        InformationType.Evidence;

    // Fallback memory for NPCs without an NPCMemory component.
    private readonly HashSet<string> localQuestions =
        new HashSet<string>();

    private readonly HashSet<string> localKnownInformation =
        new HashSet<string>();

    private readonly HashSet<string> localEvents =
        new HashSet<string>();

    public string InteractionName => interactionName;
    public bool IsInformationSource => isInformationSource;

    public virtual void Interact()
    {
        if (isInformationSource)
        {
            InteractWithInformationSource();
            return;
        }

        if (npcProfile == NPCProfile.EvelynShaw)
        {
            InteractWithEvelyn();
            return;
        }

        InteractWithGenericNPC();
    }

    private void InteractWithInformationSource()
    {
        if (JournalManager.Instance == null)
        {
            Debug.LogError(
                "Interactable: JournalManager was not found."
            );
            return;
        }

        JournalManager.Instance.AddEntry(
            informationID,
            informationTitle,
            informationDescription,
            informationType.ToString(),
            informationSource
        );
    }

    // ---------------------------------------------------------
    // GENERIC NPC
    // ---------------------------------------------------------

    private void InteractWithGenericNPC()
    {
        if (!ValidateDialogueManager())
            return;

        List<DialogueChoice> choices =
            new List<DialogueChoice>();

        choices.Add(new DialogueChoice(
            choice1,
            response1
        ));

        choices.Add(new DialogueChoice(
            choice2,
            response2
        ));

        DialogueManager.Instance.ShowDialogue(
            interactionName,
            openingLine,
            choices
        );
    }

    // ---------------------------------------------------------
    // EVELYN SHAW — SHAW'S DINER
    // ---------------------------------------------------------

    private void InteractWithEvelyn()
    {
        if (!ValidateDialogueManager())
            return;

        List<DialogueChoice> choices =
            new List<DialogueChoice>();

        bool hasAskedAboutDaniel =
            HasAsked("evelyn_asked_about_daniel");

        bool hasAskedAboutLastDays =
            HasAsked("evelyn_asked_about_last_days");

        bool hasAskedAboutOriginalAccount =
            HasAsked("evelyn_asked_about_original_account");

        bool hasAskedAboutWaterworks =
            HasAsked("evelyn_asked_about_waterworks");

        bool knowsAboutPackage =
            KnowsInformation("anonymous_package");

        bool playerHasPackage =
            JournalManager.Instance != null &&
            JournalManager.Instance.HasInformation(
                "anonymous_package"
            );

        bool hasDisclosedPackage =
            KnowsInformation("anonymous_package");

        string currentOpeningLine;

        if (hasDisclosedPackage)
        {
            currentOpeningLine =
                "You mentioned that package you received. " +
                "I've been thinking about what you asked me " +
                "about Daniel.";
        }
        else if (hasAskedAboutDaniel)
        {
            currentOpeningLine =
                "Back again? I'll tell you what I remember, " +
                "but seven years is a long time.";
        }
        else
        {
            currentOpeningLine =
                "Afternoon. You don't look like you're passing " +
                "through. What brings you to Shaw's?";
        }

        // 1. Establish Evelyn's memory of Daniel.
        if (!hasAskedAboutDaniel)
        {
            choices.Add(new DialogueChoice(
                "Ask Evelyn about Daniel Mercer",
                "Daniel? He used to come in here fairly often. " +
                "Quiet fellow. Kept mostly to himself. When he " +
                "disappeared, people said he'd decided to leave. " +
                "After a while, that became the story everyone told.",
                () =>
                {
                    RecordQuestionOnce(
                        "evelyn_asked_about_daniel"
                    );

                    RecordClaimOnce(
                        "evelyn_daniel_memory",
                        "Evelyn — Daniel Mercer",
                        "Evelyn remembers Daniel as a fairly regular, " +
                        "quiet customer. She says the town came to " +
                        "accept the explanation that he had left.",
                        "Evelyn Shaw"
                    );
                }
            ));
        }

        // 2. Ask about the period shortly before the disappearance.
        if (hasAskedAboutDaniel && !hasAskedAboutLastDays)
        {
            choices.Add(new DialogueChoice(
                "Ask what Daniel was like shortly before he disappeared",
                "He seemed distracted toward the end. Not frightened, " +
                "exactly. More like he had something on his mind and " +
                "couldn't settle. I didn't press him. People come in " +
                "here with their own troubles.",
                () =>
                {
                    RecordQuestionOnce(
                        "evelyn_asked_about_last_days"
                    );

                    RecordClaimOnce(
                        "evelyn_daniel_last_days",
                        "Evelyn — Daniel's last days",
                        "Evelyn recalls that Daniel seemed distracted " +
                        "shortly before he disappeared, although she " +
                        "did not know what was troubling him.",
                        "Evelyn Shaw"
                    );
                }
            ));
        }

        // 3. Ask about the explanation accepted by the town.
        if (hasAskedAboutDaniel && !hasAskedAboutOriginalAccount)
        {
            choices.Add(new DialogueChoice(
                "Ask whether she believed Daniel left voluntarily",
                "I couldn't tell you what I believed. The authorities " +
                "treated it as a departure, and there wasn't much " +
                "for the rest of us to go on. People have a way of " +
                "accepting an explanation when there's nothing else " +
                "to hold on to.",
                () =>
                {
                    RecordQuestionOnce(
                        "evelyn_asked_about_original_account"
                    );

                    RecordClaimOnce(
                        "evelyn_accepted_account",
                        "Evelyn — the accepted explanation",
                        "Evelyn says the authorities treated Daniel's " +
                        "disappearance as a voluntary departure. She " +
                        "does not claim to know whether that account " +
                        "was true.",
                        "Evelyn Shaw"
                    );
                }
            ));
        }

        // 4. Disclose the package only after the player has found it.
        // Knowledge is recorded only when the player selects this option.
        if (playerHasPackage && !hasDisclosedPackage)
        {
            choices.Add(new DialogueChoice(
                "Tell Evelyn about the anonymous package",
                "A maintenance record? I can't tell you what that " +
                "proves. Daniel had been asking questions about " +
                "something at the old waterworks, though. I never " +
                "knew exactly what he was looking for. I didn't " +
                "connect it to his disappearance at the time.",
                () =>
                {
                    LearnInformationOnce("anonymous_package");

                    RememberEventOnce(
                        "anonymous_package_disclosed_to_evelyn"
                    );

                    RecordClaimOnce(
                        "evelyn_package_reaction",
                        "Evelyn — reaction to the package",
                        "After being told about the anonymous package, " +
                        "Evelyn recalls that Daniel had been asking " +
                        "questions about the old waterworks. She does " +
                        "not know what he was investigating or what " +
                        "happened to him.",
                        "Evelyn Shaw"
                    );
                }
            ));
        }

        // 5. The waterworks lead becomes available after disclosure.
        // This is a separate question, not an automatic conclusion.
        if (knowsAboutPackage && !hasAskedAboutWaterworks)
        {
            choices.Add(new DialogueChoice(
                "Ask what Daniel said about the waterworks",
                "I only remember him bringing it up. He asked whether " +
                "the place still kept its own records. I told him I " +
                "had no idea. I worked here, not for the town. That's " +
                "all I can honestly tell you.",
                () =>
                {
                    RecordQuestionOnce(
                        "evelyn_asked_about_waterworks"
                    );

                    RecordClaimOnce(
                        "evelyn_waterworks_statement",
                        "Evelyn — Daniel and the waterworks",
                        "Evelyn recalls Daniel asking whether the old " +
                        "waterworks kept its own records. She does not " +
                        "know why he was interested.",
                        "Evelyn Shaw"
                    );
                }
            ));
        }

        // 6. Always offer a low-stakes topic.
        choices.Add(new DialogueChoice(
            "Ask about the town",
            "Same people, same tables, same arguments over the " +
            "weather. Small towns don't change as quickly as " +
            "people imagine.",
            null
        ));

        DialogueManager.Instance.ShowDialogue(
            interactionName,
            currentOpeningLine,
            choices
        );
    }

    // ---------------------------------------------------------
    // MEMORY HELPERS
    // ---------------------------------------------------------

    private NPCMemory GetMemory()
    {
        return GetComponent<NPCMemory>();
    }

    private bool HasAsked(string questionID)
    {
        NPCMemory memory = GetMemory();

        if (memory != null)
            return memory.HasBeenAsked(questionID);

        return localQuestions.Contains(questionID);
    }

    private void RecordQuestionOnce(string questionID)
    {
        NPCMemory memory = GetMemory();

        if (memory != null)
        {
            memory.RecordQuestion(questionID);
        }
        else
        {
            localQuestions.Add(questionID);
        }
    }

    private bool KnowsInformation(string infoID)
    {
        NPCMemory memory = GetMemory();

        if (memory != null)
            return memory.KnowsInformation(infoID);

        return localKnownInformation.Contains(infoID);
    }

    private void LearnInformationOnce(string infoID)
    {
        NPCMemory memory = GetMemory();

        if (memory != null)
        {
            memory.LearnInformation(infoID);
        }
        else
        {
            localKnownInformation.Add(infoID);
        }
    }

    private void RememberEventOnce(string eventID)
    {
        NPCMemory memory = GetMemory();

        if (memory != null)
        {
            memory.RememberEvent(eventID);
        }
        else
        {
            localEvents.Add(eventID);
        }
    }

    // ---------------------------------------------------------
    // JOURNAL HELPERS
    // ---------------------------------------------------------

    private void RecordClaimOnce(
        string entryID,
        string title,
        string description,
        string source)
    {
        if (JournalManager.Instance == null)
        {
            Debug.LogError(
                "Interactable: JournalManager was not found " +
                "while recording claim '" + entryID + "'."
            );
            return;
        }

        // The JournalManager prevents duplicate IDs.
        JournalManager.Instance.RecordClaim(
            entryID,
            title,
            description,
            source
        );
    }

    private bool ValidateDialogueManager()
    {
        if (DialogueManager.Instance != null)
            return true;

        Debug.LogError(
            "Interactable: DialogueManager was not found."
        );

        return false;
    }
}
