
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
        "An anonymous sender sent me two waterworks records and a " +
        "note claiming the original search missed something.";

    [SerializeField]
    private InformationType informationType =
        InformationType.Evidence;

    [Header("Scene Exit")]
    [SerializeField] private bool isSceneExit = false;
    [SerializeField] private string destinationScene = "Diner_Investigation";

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
        // Scene exits take priority over other interaction types.
        if (isSceneExit)
        {
            ExitToScene();
            return;
        }

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

        List<DialogueChoice> choices = new List<DialogueChoice>();

        bool hasAskedAboutDaniel = HasAsked("evelyn_asked_about_daniel");
        bool hasAskedAboutLastDays = HasAsked("evelyn_asked_about_last_days");
        bool hasAskedAboutOriginalAccount = HasAsked("evelyn_asked_about_original_account");
        bool hasAskedAboutWaterworks = HasAsked("evelyn_asked_about_waterworks");
        bool knowsAboutPackage = KnowsInformation("anonymous_package");
        bool playerHasPackage = JournalManager.Instance != null &&
            JournalManager.Instance.HasInformation("anonymous_package");

        string currentOpeningLine;
        if (knowsAboutPackage)
        {
            currentOpeningLine =
                "I've been thinking about those papers you showed me. " +
                "I wish I remembered more.";
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
                "Afternoon. You don't look like you're passing through. " +
                "What brings you to Shaw's?";
        }

        // First establish Evelyn's familiarity with Daniel.
        if (!hasAskedAboutDaniel)
        {
            choices.Add(new DialogueChoice(
                "Ask Evelyn about Daniel Mercer",
                "Daniel? Sure, I remember him. Came in a few times a week, " +
                "usually by himself. Quiet fellow. Always polite. When he " +
                "disappeared, people said he'd left town. I never heard anything definite.",
                () =>
                {
                    RecordQuestionOnce("evelyn_asked_about_daniel");
                    RecordClaimOnce(
                        "evelyn_daniel_memory",
                        "Daniel at the Diner",
                        "Evelyn remembers Daniel as a quiet regular. " +
                        "She heard he left town but never knew for certain.",
                        "Evelyn Shaw");
                }
            ));
        }

        if (hasAskedAboutDaniel && !hasAskedAboutLastDays)
        {
            choices.Add(new DialogueChoice(
                "Ask what Daniel was like shortly before he disappeared",
                "He seemed distracted that last week. I remember bringing " +
                "him coffee and having to ask twice whether he wanted " +
                "anything to eat. That's not how he usually was. I figured " +
                "something was bothering him, but I didn't ask.",
                () =>
                {
                    RecordQuestionOnce("evelyn_asked_about_last_days");
                    RecordClaimOnce(
                        "evelyn_daniel_last_days",
                        "Daniel's Last Days",
                        "Evelyn noticed Daniel seemed unusually distracted " +
                        "during the week before he disappeared. She doesn't know why.",
                        "Evelyn Shaw");
                }
            ));
        }

        if (hasAskedAboutDaniel && !hasAskedAboutOriginalAccount)
        {
            choices.Add(new DialogueChoice(
                "Ask whether she believed Daniel left voluntarily",
                "At the time? I suppose I did. That's what everyone was " +
                "saying, and the sheriff's office didn't seem to think " +
                "anything else had happened. But I never saw him leave. " +
                "I don't know anyone who did.",
                () =>
                {
                    RecordQuestionOnce("evelyn_asked_about_original_account");
                    RecordClaimOnce(
                        "evelyn_accepted_account",
                        "Voluntary Departure",
                        "Evelyn believed Daniel left because everyone said so. " +
                        "She never saw him leave.",
                        "Evelyn Shaw");
                }
            ));
        }

        // The player must actively disclose the package.
        if (playerHasPackage && !knowsAboutPackage)
        {
            choices.Add(new DialogueChoice(
                "Tell Evelyn about the anonymous package",
                "Someone sent you waterworks records? That's strange. " +
                "Daniel asked me something about the old waterworks not " +
                "long before he disappeared. I didn't think much of it then. " +
                "I couldn't tell you what he was looking for.",
                () =>
                {
                    LearnInformationOnce("anonymous_package");
                    RememberEventOnce("anonymous_package_disclosed_to_evelyn");
                    RecordClaimOnce(
                        "evelyn_package_reaction",
                        "Evelyn's Recollection",
                        "Mentioning the package reminded Evelyn that Daniel " +
                        "had asked about the old waterworks shortly before disappearing.",
                        "Evelyn Shaw");
                }
            ));
        }

        // Follow-up only after Evelyn learns about the package.
        if (knowsAboutPackage && !hasAskedAboutWaterworks)
        {
            choices.Add(new DialogueChoice(
                "Ask what Daniel said about the waterworks",
                "He asked whether the old waterworks kept its own records " +
                "or if everything went through the town office. I told him " +
                "I wouldn't know. He didn't say what he needed them for, " +
                "and I didn't think to ask.",
                () =>
                {
                    RecordQuestionOnce("evelyn_asked_about_waterworks");
                    RecordClaimOnce(
                        "evelyn_waterworks_statement",
                        "Waterworks Records",
                        "Daniel asked Evelyn where the waterworks records " +
                        "were kept. She didn't know.",
                        "Evelyn Shaw");
                }
            ));
        }

        choices.Add(new DialogueChoice(
            "Ask about the town",
            "Grayford doesn't change much. Same faces, same tables, " +
            "same complaints about the weather. People remember things, " +
            "though. Even when they'd rather not talk about them.",
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
    private void ExitToScene()
    {
        if (string.IsNullOrWhiteSpace(destinationScene))
        {
            Debug.LogError(
                "Interactable: No destination scene configured.",
                this
            );
            return;
        }

        Debug.Log("Leaving for scene: " + destinationScene);

        SceneManager.LoadScene(destinationScene);
    }
}
