
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
        EvelynShaw,
        ArthurFinch,
        HelenWard,
        FrankBell,
        CalebTurner,
        MichaelDwyer,
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
    [SerializeField] private string destinationSpawnID = "";

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

        if (npcProfile == NPCProfile.ArthurFinch)
        {
            InteractWithArthur();
            return;
        }

        if (npcProfile == NPCProfile.HelenWard)
        {
            InteractWithHelen();
            return;
        }

        if (npcProfile == NPCProfile.FrankBell)
        {
            InteractWithFrank();
            return;
        }

        if (npcProfile == NPCProfile.CalebTurner)
        {
            InteractWithCaleb();
            return;
        }

        if (npcProfile == NPCProfile.MichaelDwyer)
        {
            InteractWithDwyer();
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
                        "Evelyn remembers Daniel as a quiet diner regular. She never " +
                        "knew whether he left town.",
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
                        "Evelyn noticed Daniel seemed distracted during the week " +
                        "before he disappeared.",
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
                        "Evelyn accepted the story that Daniel left town, but never " +
                        "witnessed his departure.",
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
                        "The anonymous package reminded Evelyn that Daniel had asked " +
                        "about the waterworks before disappearing.",
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
                        "Daniel asked Evelyn where the waterworks records were kept. " +
                        "She couldn't tell him.",
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

        if (JournalManager.Instance.HasInformation(
            "chamber_recess_personal_effect"))
        {
            choices.Add(new DialogueChoice(
                "Ask Evelyn about the metal object",
                "Daniel showed me something similar once. " +
                "A little metal tag with a number stamped into it. " +
                "He said it had something to do with the waterworks. " +
                "I don't know where he got it, and I couldn't " +
                "tell you what the number was.",
                () =>
                {
                    JournalManager.Instance.RecordClaim(
                        "evelyn_daniel_metal_tag",
                        "Daniel's Numbered Metal Tag",
                        "Evelyn remembers Daniel showing her a numbered metal tag " +
                        "linked to the waterworks. She cannot recall its number.",
                        "Evelyn Shaw"
                    );
                }
            ));
        }

        if (
            JournalManager.Instance.HasInformation(
                "daniel_original_police_report") &&
            !JournalManager.Instance.HasInformation(
                "evelyn_dwyer_investigation_recollection")
        )
        {
            choices.Add(new DialogueChoice(
                "Ask about Deputy Dwyer's investigation",
                "Dwyer came around after Daniel disappeared. " +
                "He asked about Daniel's habits and whether he'd " +
                "been planning to leave town. I told him Daniel " +
                "had been worried about something at the waterworks. " +
                "He wrote it down, or at least I thought he did. " +
                "I never heard anything more about it.",
                () =>
                {
                    JournalManager.Instance.RecordClaim(
                        "evelyn_dwyer_investigation_recollection",
                        "Evelyn's Account of the Original Investigation",
                        "Evelyn says she told Dwyer about Daniel's waterworks " +
                        "concerns during the original investigation. She believed he " +
                        "noted them.",
                        "Evelyn Shaw"
                    );
                }
            ));
        }


        DialogueManager.Instance.ShowDialogue(
            interactionName,
            currentOpeningLine,
            choices
        );
    }

    // ---------------------------------------------------------
    // ARTHUR FINCH — GRAYFORD GAZETTE
    // ---------------------------------------------------------

    private void InteractWithArthur()
    {
        if (!ValidateDialogueManager())
            return;

        List<DialogueChoice> choices = new List<DialogueChoice>();
        bool hasAskedAboutDaniel = HasAsked("arthur_asked_about_daniel");
        bool hasAskedAboutReporting = HasAsked("arthur_asked_about_reporting");

        bool hasUnverifiedDeparture = JournalManager.Instance != null &&
            JournalManager.Instance.HasHypothesis("unverified_departure");
        bool hasUnresolvedWork = JournalManager.Instance != null &&
            JournalManager.Instance.HasHypothesis("unresolved_waterworks_work");

        string greeting = hasAskedAboutDaniel || hasAskedAboutReporting
            ? "Back again? Well, if it's about the old Mercer case, " +
              "I'll tell you what I remember."
            : "You looking for someone? The paper's not much of a place " +
              "for visitors these days. I'm Arthur Finch. Used to run the Gazette.";

        if (!hasAskedAboutDaniel)
        {
            choices.Add(new DialogueChoice(
                "Ask about Daniel Mercer",
                "I knew who he was. Small town, you tend to know people's names. " +
                "He wasn't a reporter, if that's what you're wondering. " +
                "I remember his disappearance more than I remember the man himself. " +
                "His mother came by once, asking whether we'd heard anything " +
                "the sheriff hadn't told her.",
                () =>
                {
                    RecordQuestionOnce("arthur_asked_about_daniel");
                    RecordClaimOnce(
                        "arthur_daniel_recollection",
                        "Arthur's Recollection",
                        "Daniel's mother visited the Gazette seeking information " +
                        "beyond what the sheriff's office had provided.",
                        "Arthur Finch");
                }
            ));
        }

        if (!hasAskedAboutReporting)
        {
            choices.Add(new DialogueChoice(
                "Ask about the original Gazette reporting",
                "We reported what the sheriff's office gave us. They said " +
                "there was no sign of foul play and that Mercer might've left " +
                "on his own. We didn't have anyone who'd seen him leave. " +
                "I made sure we attributed that explanation to the department. " +
                "That was the most we could responsibly say.",
                () =>
                {
                    RecordQuestionOnce("arthur_asked_about_reporting");
                    RecordClaimOnce(
                        "arthur_reporting_practice",
                        "The Gazette's Sources",
                        "The Gazette relied on the sheriff's office and had no " +
                        "independent witness to Daniel leaving town.",
                        "Arthur Finch");
                }
            ));
        }

        if (hasAskedAboutReporting && !HasAsked("arthur_asked_why_coverage_stopped"))
        {
            choices.Add(new DialogueChoice(
                "Ask why the Gazette stopped covering the disappearance",
                "Because nothing new came in. We ran the first story, then a " +
                "follow-up. I asked around afterward, but nobody had anything " +
                "they were willing to put their name to. A weekly paper can't " +
                "keep printing the same story without new information. " +
                "I know that wasn't much comfort to his family.",
                () =>
                {
                    RecordQuestionOnce("arthur_asked_why_coverage_stopped");
                    RecordClaimOnce(
                        "arthur_coverage_stopped",
                        "End of the Gazette's Coverage",
                        "Arthur says the Gazette stopped covering the case because no " +
                        "verifiable new information emerged.",
                        "Arthur Finch");
                }
            ));
        }

        if (hasUnverifiedDeparture && !HasAsked("arthur_asked_about_departure_assessment"))
        {
            choices.Add(new DialogueChoice(
                "Point out that nobody confirmed Daniel left town",
                "That's true. We never established that he left. The sheriff's " +
                "office thought it was the likeliest explanation, and we " +
                "reported it as their assessment. Looking back, I can see " +
                "how people might've taken that as a conclusion. " +
                "It wasn't one we could prove.",
                () =>
                {
                    RecordQuestionOnce("arthur_asked_about_departure_assessment");
                    RecordClaimOnce(
                        "arthur_departure_assessment",
                        "Arthur on the Departure Account",
                        "Arthur admits the Gazette could not verify Daniel's " +
                        "departure. Its reporting may have made the sheriff's theory " +
                        "seem conclusive.",
                        "Arthur Finch");
                }
            ));
        }

        if (hasUnresolvedWork && !HasAsked("arthur_asked_about_waterworks_records"))
        {
            choices.Add(new DialogueChoice(
                "Ask about the 1991 waterworks records",
                "Town maintenance records? We wouldn't have kept those here. " +
                "If you're looking for the original paperwork, I'd try the " +
                "municipal records office. The Gazette sometimes requested " +
                "copies for stories, but I don't remember us covering that " +
                "particular job.",
                () =>
                {
                    RecordQuestionOnce("arthur_asked_about_waterworks_records");
                    RecordClaimOnce(
                        "arthur_records_lead",
                        "Municipal Records Lead",
                        "Arthur recommends the municipal records office for the " +
                        "original waterworks paperwork.",
                        "Arthur Finch");
                }
            ));
        }

        DialogueManager.Instance.ShowDialogue(
            interactionName,
            greeting,
            choices
        );
    }

    // ---------------------------------------------------------
    // HELEN WARD — MUNICIPAL RECORDS OFFICE
    // ---------------------------------------------------------

    private void InteractWithHelen()
    {
        if (!ValidateDialogueManager())
            return;

        List<DialogueChoice> choices = new List<DialogueChoice>();
        JournalManager journal = JournalManager.Instance;

        bool hasRegister = journal != null &&
            journal.HasInformation("municipal_maintenance_register");
        bool hasLedger = journal != null &&
            journal.HasInformation("municipal_status_ledger");
        bool hasIndex = journal != null &&
            journal.HasInformation("municipal_retention_index");
        bool hasHypothesis = journal != null &&
            journal.HasHypothesis("unresolved_waterworks_work");

        string greeting = HasAsked("helen_asked_about_procedure") ||
            HasAsked("helen_asked_about_work_order")
            ? "Back again? If you have a reference number, I can explain " +
              "what the records say."
            : "Good afternoon. I'm Helen Ward. The public registers are " +
              "available for inspection. Is there something specific you need?";

        if (!HasAsked("helen_asked_about_procedure"))
        {
            choices.Add(new DialogueChoice(
                "Ask how the records office organizes its files",
                "Public Works keeps the original registers and status ledgers. " +
                "Contractor paperwork is indexed separately. A reference " +
                "number is the best way to find the right file.",
                () =>
                {
                    RecordQuestionOnce("helen_asked_about_procedure");
                    RecordClaimOnce(
                        "helen_records_procedure",
                        "Municipal Filing Procedure",
                        "Public Works keeps registers and status ledgers; contractor " +
                        "paperwork is indexed separately.",
                        "Helen Ward");
                }
            ));
        }

        if (hasRegister && !HasAsked("helen_asked_about_work_order"))
        {
            choices.Add(new DialogueChoice(
                "Ask about work order WW-91-184",
                "That reference is in the Public Works register. It shows " +
                "an inspection was authorized. An authorization doesn't " +
                "tell you whether the work was completed.",
                () =>
                {
                    RecordQuestionOnce("helen_asked_about_work_order");
                    RecordClaimOnce(
                        "helen_work_order_context",
                        "Helen on Work-Order Authorization",
                        "Helen confirms WW-91-184 was authorized, but the register " +
                        "does not establish completion.",
                        "Helen Ward");
                }
            ));
        }

        if (hasLedger && !HasAsked("helen_asked_about_completion"))
        {
            choices.Add(new DialogueChoice(
                "Ask why the completion field is blank",
                "A blank field doesn't necessarily mean the work wasn't " +
                "done. It means this ledger doesn't confirm that it was. " +
                "I wouldn't draw a stronger conclusion from it.",
                () =>
                {
                    RecordQuestionOnce("helen_asked_about_completion");
                    RecordClaimOnce(
                        "helen_missing_completion",
                        "Helen on the Missing Completion Entry",
                        "Helen says the blank completion field does not establish " +
                        "whether the work was done.",
                        "Helen Ward");
                }
            ));
        }

        if (hasIndex && !HasAsked("helen_asked_about_contractor_files"))
        {
            choices.Add(new DialogueChoice(
                "Ask where contractor completion reports are kept",
                "The contractor's own records would be the next place to " +
                "check. Bell Municipal Services handled that order. I can't " +
                "tell you whether they still have a report for it.",
                () =>
                {
                    RecordQuestionOnce("helen_asked_about_contractor_files");
                    RecordClaimOnce(
                        "helen_contractor_records",
                        "Contractor Records Lead",
                        "Helen suggests asking Bell Municipal Services for supporting " +
                        "records on WW-91-184.",
                        "Helen Ward");
                }
            ));
        }

        if (hasRegister && hasLedger &&
    !HasAsked("helen_asked_about_discrepancy"))
        {
            choices.Add(new DialogueChoice(
                "Question the discrepancy between the work records",
                "You're comparing an authorization with an incomplete " +
                "status record. I can confirm the paperwork doesn't settle " +
                "the question. I can't tell you what happened at the site.",
                () =>
                {
                    RecordQuestionOnce("helen_asked_about_discrepancy");
                    RecordClaimOnce(
                        "helen_unresolved_work_assessment",
                        "Helen's Assessment of the Records",
                        "Helen confirms the available records cannot establish " +
                        "whether the waterworks job was completed.",
                        "Helen Ward");
                }
            ));
        }

        // The player can ask about the changed waterworks access only
        // after connecting Caleb's recollection to the physical barrier.
        bool hasChangedServiceAccess = journal != null &&
            journal.HasHypothesis("changed_service_access");

        if (hasChangedServiceAccess &&
            !HasAsked("helen_asked_about_facilities_alterations"))
        {
            choices.Add(new DialogueChoice(
                "Ask about records for changes to the waterworks passage",
                "Normally, yes. A permanent alteration to a municipal " +
                "facility should have some kind of authorization or " +
                "maintenance entry. But older site modifications weren't " +
                "always filed with the original work orders. You'd want " +
                "the facilities alteration register. I can tell you " +
                "where it's kept.",
                () =>
                {
                    RecordQuestionOnce("helen_asked_about_facilities_alterations");
                    RecordClaimOnce(
                        "helen_facilities_register_lead",
                        "Facilities Alteration Register",
                        "Helen recommends checking the facilities alteration register " +
                        "for records of changes to the lower passage.",
                        "Helen Ward");
                }
            ));
        }

        choices.Add(new DialogueChoice(
            "Ask about the office's public hours",
            "We keep regular weekday hours. The older files take a " +
            "little longer to find, but you're welcome to consult " +
            "the public registers.",
            null
        ));


        if (
            JournalManager.Instance.HasInformation(
                "chamber_possible_human_remains") &&
            !JournalManager.Instance.HasInformation(
                "helen_chamber_discovery_report")
        )

        {
            choices.Add(new DialogueChoice(
                "Tell Helen about the discovery in Chamber L-02",
                "You found what might be human remains? " +
                "Then this has gone beyond missing maintenance records. " +
                "The chamber needs to be secured and the discovery " +
                "reported to the authorities. " +
                "I can help you identify the appropriate municipal " +
                "contact, but I can't authorize an examination myself.",
                () =>
                {
                    JournalManager.Instance.AddEntry(
                        "helen_chamber_discovery_report",
                        "Chamber Discovery Reported to Helen Ward",
                        "I reported the suspected remains in Chamber L-02 to Helen. " +
                        "She advised securing the site and contacting the " +
                        "authorities.",
                        "Event",
                        "Conversation with Helen Ward"
                    );
                }
            ));
        }

        if (
            JournalManager.Instance.HasInformation(
                "helen_chamber_discovery_report") &&
            !JournalManager.Instance.HasInformation(
                "chamber_official_examination_requested")
        )
        {
            choices.Add(new DialogueChoice(
                "Ask Helen about an official examination",
                "I've identified the proper contact. " +
                "I'll pass along the chamber location and " +
                "the details you've documented. " +
                "The authorities will decide how to proceed. " +
                "Until then, you should leave the site undisturbed.",
                () =>
                {
                    JournalManager.Instance.AddEntry(
                        "chamber_official_examination_requested",
                        "Official Examination Requested",
                        "Helen agreed to forward the chamber discovery to the " +
                        "appropriate authorities for examination. Findings are " +
                        "pending.",
                        "Event",
                        "Conversation with Helen Ward"
                    );
                }
            ));
        }

        if (
            journal.HasInformation("daniel_remains_identification_report") &&
            !HasAsked("helen_asked_about_daniel_identification")
        )
        {
            choices.Add(new DialogueChoice(
                "Ask Helen about Daniel's identification",
                "I saw the supplemental report. " +
                "After seven years, at least his family " +
                "can finally be told what happened to him " +
                "in one respect.\n\n" +
                "But the report doesn't explain how he " +
                "died, or why his remains were in that " +
                "chamber. Those are questions the " +
                "investigation still needs to answer.\n\n" +
                "And the records concerning that barrier " +
                "deserve another look.",
                () =>
                {
                    RecordQuestionOnce(
                        "helen_asked_about_daniel_identification");

                    RecordClaimOnce(
                        "helen_daniel_identification_response",
                        "Helen on Daniel's Identification",
                        "Helen acknowledges Daniel's identification but says the " +
                        "report does not explain his death or the chamber's closure.",
                        "Helen Ward");
                }
            ));
        }

        DialogueManager.Instance.ShowDialogue(
            interactionName,
            greeting,
            choices
        );
    }

    // ---------------------------------------------------------
    // FRANK BELL — BELL MUNICIPAL SERVICES
    // ---------------------------------------------------------

    private void InteractWithFrank()
    {
        if (!ValidateDialogueManager())
            return;

        List<DialogueChoice> choices = new List<DialogueChoice>();
        JournalManager journal = JournalManager.Instance;

        bool hasJobLedger = journal != null && journal.HasInformation("bell_job_ledger");
        bool hasDispatch = journal != null && journal.HasInformation("bell_dispatch_sheet");
        bool hasBilling = journal != null && journal.HasInformation("bell_billing_copy");
        bool hasBillingHypothesis = journal != null && journal.HasHypothesis("billed_work_unverified");
        bool hasEvelynStatement = journal != null && journal.HasInformation("evelyn_waterworks_statement");

        string greeting = HasAsked("frank_asked_about_business") ||
            HasAsked("frank_asked_about_work_order")
            ? "You again. Still looking through old paperwork?"
            : "Can I help you? If you're looking for a job, we're not hiring. " +
              "If it's about town maintenance, you'll have to be more specific.";

        if (!HasAsked("frank_asked_about_business"))
        {
            choices.Add(new DialogueChoice(
                "Ask how long Bell has worked for Grayford",
                "Long enough to remember when half the town still had dirt roads. " +
                "We handle drainage, repairs, whatever the council doesn't have " +
                "the people for. Nothing glamorous, but somebody has to do it.",
                () =>
                {
                    RecordQuestionOnce("frank_asked_about_business");
                    RecordClaimOnce(
                        "frank_business_history",
                        "Bell Municipal Services",
                        "Frank says Bell Municipal Services has handled Grayford's " +
                        "maintenance contracts for years.",
                        "Frank Bell");
                }
            ));
        }

        if (hasJobLedger && !HasAsked("frank_asked_about_work_order"))
        {
            choices.Add(new DialogueChoice(
                "Ask about work order WW-91-184",
                "Ninety-one? You're asking me about a job from seven years ago? " +
                "We did plenty of work at the old waterworks. Inspections, " +
                "drainage problems, that sort of thing. I couldn't tell you the " +
                "details of one particular job without looking it up.",
                () =>
                {
                    RecordQuestionOnce("frank_asked_about_work_order");
                    RecordClaimOnce(
                        "frank_work_order_account",
                        "Frank on WW-91-184",
                        "Frank recalls waterworks maintenance jobs but claims not to " +
                        "remember the details of WW-91-184.",
                        "Frank Bell");
                }
            ));
        }

        if (hasDispatch && !HasAsked("frank_asked_about_dispatch"))
        {
            choices.Add(new DialogueChoice(
                "Ask about the equipment dispatched to the waterworks",
                "That's what a dispatch sheet is for. Means we assigned a crew. " +
                "Doesn't mean I can tell you what they found when they got there. " +
                "You'd need the field report for that.",
                () =>
                {
                    RecordQuestionOnce("frank_asked_about_dispatch");
                    RecordClaimOnce(
                        "frank_dispatch_explanation",
                        "Frank on the Dispatch Sheet",
                        "Frank says the dispatch sheet confirms a crew assignment, " +
                        "not what happened on site.",
                        "Frank Bell");
                }
            ));
        }

        if (hasBilling && !HasAsked("frank_asked_about_billing"))
        {
            choices.Add(new DialogueChoice(
                "Ask why Bell billed the town for WW-91-184",
                "Because we had a job on the books. We billed for the inspection " +
                "and the equipment allocation. That's standard. The invoice " +
                "isn't a diary of what happened out there.",
                () =>
                {
                    RecordQuestionOnce("frank_asked_about_billing");
                    RecordClaimOnce(
                        "frank_billing_explanation",
                        "Frank on the Invoice",
                        "Frank describes the invoice as routine billing for " +
                        "inspection and equipment, not proof of completed work.",
                        "Frank Bell");
                }
            ));
        }

        if (hasBillingHypothesis && !HasAsked("frank_asked_about_completion"))
        {
            choices.Add(new DialogueChoice(
                "Challenge the missing completion confirmation",
                "Then the town's file is incomplete. Wouldn't be the first time. " +
                "Look, I can't account for every piece of paper somebody " +
                "misplaced seven years ago. If you're suggesting we billed " +
                "for work we didn't do, you'd better have more than an empty " +
                "box in a ledger.",
                () =>
                {
                    RecordQuestionOnce("frank_asked_about_completion");
                    RecordClaimOnce(
                        "frank_completion_response",
                        "Frank on Missing Completion Records",
                        "Frank blames incomplete records for the missing completion " +
                        "entry and denies improper billing.",
                        "Frank Bell");
                }
            ));
        }

        if (hasEvelynStatement && !HasAsked("frank_asked_about_daniel"))
        {
            choices.Add(new DialogueChoice(
                "Ask whether Daniel Mercer visited about the waterworks",
                "Mercer? I remember the name. Everybody does. But I don't " +
                "remember him coming here. Why would he?",
                () =>
                {
                    RecordQuestionOnce("frank_asked_about_daniel");
                    RecordClaimOnce(
                        "frank_daniel_denial",
                        "Frank on Daniel Mercer",
                        "Frank claims he does not remember Daniel visiting Bell " +
                        "Municipal Services.",
                        "Frank Bell");
                }
            ));
        }

        // The personnel register identifies the assigned workers, not attendance.
        bool hasPersonnelRegister = journal != null &&
            journal.HasInformation("bell_personnel_register");

        if (hasPersonnelRegister && !HasAsked("frank_asked_about_former_workers"))
        {
            choices.Add(new DialogueChoice(
                "Ask where Caleb Turner and R. Hayes can be found",
                "Hayes left town years ago. Turner hasn't worked for me in a " +
                "long time either. Last I heard, he was doing maintenance " +
                "work on his own. I don't keep track of former employees.",
                () =>
                {
                    RecordQuestionOnce("frank_asked_about_former_workers");
                    RecordClaimOnce(
                        "frank_former_workers",
                        "Former Bell Employees",
                        "Frank says R. Hayes left town and Caleb Turner now works " +
                        "independently.",
                        "Frank Bell");
                }
            ));
        }

        // Follow-up unlocked by comparing the alteration register with
        // Caleb's recollection of the lower service gate.
        bool hasUnverifiedAccessAlteration = journal != null &&
            journal.HasHypothesis("unverified_access_alteration");

        if (hasUnverifiedAccessAlteration &&
            !HasAsked("frank_asked_about_barrier_responsibility"))
        {
            choices.Add(new DialogueChoice(
                "Question the incomplete lower-access alteration record",
                "That old register? Those entries were handled by Public Works. " +
                "We had a drainage job out there, sure, but that doesn't mean " +
                "every change to the building was ours. If somebody replaced " +
                "a gate or put up a barrier, you'd need to ask who authorized " +
                "it. I can't tell you that from a billing sheet.",
                () =>
                {
                    RecordQuestionOnce("frank_asked_about_barrier_responsibility");
                    RecordClaimOnce(
                        "frank_barrier_responsibility",
                        "Frank on the Lower-Access Alteration",
                        "Frank acknowledges Bell's drainage work but denies that the " +
                        "records establish who installed the lower-access barrier.",
                        "Frank Bell");
                }
            ));
        }

        // Follow-up: Frank is named in the supplementary
        // access log on the day Daniel disappeared.
        bool hasSupplementaryAccessLog = journal != null &&
            journal.HasInformation("bell_supplementary_access_log");

        if (hasSupplementaryAccessLog &&
            !HasAsked("frank_asked_about_oct17_visit"))
        {
            choices.Add(new DialogueChoice(
                "Ask about Frank's October 17 waterworks site visit",
                "I remember going out there around that time. There " +
                "were drainage complaints that hadn't been cleared up. " +
                "But those logs weren't always filled in on the day. " +
                "If my name's on it, I probably intended to check the " +
                "site. Doesn't mean I can tell you exactly when I went.",
                () =>
                {
                    RecordQuestionOnce("frank_asked_about_oct17_visit");
                    RecordClaimOnce(
                        "frank_oct17_site_visit",
                        "Frank's Account of the October 17 Visit",
                        "Frank recalls visiting the waterworks around October 17, " +
                        "1991, but will not confirm that date. He questions the " +
                        "access log's accuracy.",
                        "Frank Bell");
                }
            ));
        }

        // Follow-up: confront Frank with the gatekeeper's
        // October 17 visitor register.
        bool hasPossibleOct17Attendance = journal != null &&
            journal.HasHypothesis("possible_oct17_attendance");

        if (hasPossibleOct17Attendance &&
            !HasAsked("frank_asked_about_gatekeeper_register"))
        {
            choices.Add(new DialogueChoice(
                "Confront Frank with the 6:40 PM visitor entry",
                "Six forty? If that's what the book says, that's " +
                "what somebody wrote down. I told you, I was " +
                "dealing with drainage complaints around then. " +
                "Those registers weren't exactly reliable.\n\n" +
                "Maybe I stopped by. Maybe somebody expected me. " +
                "I can't tell you what a clerk was thinking seven " +
                "years ago. And if you're trying to make something " +
                "out of that lower service entrance, you're wasting " +
                "your time. There was nothing down there worth " +
                "looking at.",
                () =>
                {
                    RecordQuestionOnce(
                        "frank_asked_about_gatekeeper_register");

                    RecordClaimOnce(
                        "frank_lower_access_knowledge",
                        "Frank's Unprompted Reference to Lower Access",
                        "Confronted with the 6:40 PM entry, Frank questions the " +
                        "register and brings up the lower service entrance without " +
                        "prompting.",
                        "Frank Bell");
                }
            ));
        }

        if (
            journal.HasInformation("bell_original_barrier_work_order") &&
            !HasAsked("frank_confronted_about_original_barrier_order")
        )
        {
            choices.Add(new DialogueChoice(
                "Confront Bell about the original barrier work order",
                "Where did you get that?\n\n" +
                "That was an emergency closure. The lower " +
                "passage wasn't safe, and I wasn't going " +
                "to have my crew wandering into an area " +
                "that hadn't been inspected.\n\n" +
                "As for the paperwork, I told them to " +
                "record the work they actually performed. " +
                "They installed a barrier. They didn't " +
                "inspect the chamber.",
                () =>
                {
                    RecordQuestionOnce(
                        "frank_confronted_about_original_barrier_order");

                    RecordClaimOnce(
                        "frank_original_barrier_order_response",
                        "Bell's Explanation for the Barrier Order",
                        "Frank admits ordering the October 18 closure. He claims the " +
                        "chamber was unsafe and that the paperwork covered only work " +
                        "performed.",
                        "Frank Bell");
                }
            ));
        }

        if (
            journal.HasInformation("caleb_oct17_yard_witness_account") &&
            !HasAsked("frank_confronted_about_oct17_yard")
        )
        {
            choices.Add(new DialogueChoice(
                "Confront Bell about meeting Dwyer at the yard",
                "Caleb Turner told you that?\n\n" +
                "He worked for me. He knew deputies came " +
                "by the yard sometimes, especially when " +
                "there were problems at municipal sites.\n\n" +
                "And yes, I changed the crew assignment " +
                "for the eighteenth. There was a safety " +
                "issue at the waterworks. I wasn't sending " +
                "an ordinary maintenance crew into a " +
                "restricted area.\n\n" +
                "If Caleb thinks that means something " +
                "else happened, he should say what " +
                "he actually saw.",
                () =>
                {
                    RecordQuestionOnce(
                        "frank_confronted_about_oct17_yard");

                    RecordClaimOnce(
                        "frank_yard_meeting_response",
                        "Bell's Response to Caleb's Testimony",
                        "Frank admits changing the October 18 crew assignment but " +
                        "describes deputies visiting the yard as routine.",
                        "Frank Bell");
                }
            ));
        }

        if (
            journal.HasInformation("daniel_remains_identification_report") &&
            !HasAsked("frank_confronted_about_daniel_remains")
        )
        {
            choices.Add(new DialogueChoice(
                "Confront Bell about Daniel's remains",
                "Daniel Mercer?\n\n" +
                "You're telling me they found him in " +
                "that chamber?\n\n" +
                "I didn't know that. The passage was " +
                "closed because it wasn't safe. " +
                "That's what the work order says.\n\n" +
                "I can't explain how he ended up there. " +
                "And I won't pretend I can.",
                () =>
                {
                    RecordQuestionOnce(
                        "frank_confronted_about_daniel_remains");

                    RecordClaimOnce(
                        "frank_daniel_remains_response",
                        "Bell Denies Knowing About Daniel's Remains",
                        "Frank denies knowing Daniel's body was in Chamber L-02 when " +
                        "he ordered the passage sealed.",
                        "Frank Bell");
                }
            ));
        }

        if (
            journal.HasHypothesis("daniel_possible_platform_fall") &&
            !HasAsked("frank_confronted_about_platform_fall")
        )
        {
            choices.Add(new DialogueChoice(
                "Ask Bell whether Daniel fell from the platform",
                "That platform was dangerous. " +
                "The railing had been giving us trouble " +
                "for months.\n\n" +
                "If Mercer was down there, he had no " +
                "business climbing around on it.\n\n" +
                "But don't put words in my mouth. " +
                "I didn't say I saw him fall.",
                () =>
                {
                    RecordQuestionOnce(
                        "frank_confronted_about_platform_fall");

                    RecordClaimOnce(
                        "frank_platform_fall_response",
                        "Bell's Response to the Possible Fall",
                        "Frank says the platform railing was unsafe and denies " +
                        "witnessing Daniel fall.",
                        "Frank Bell");
                }
            ));
        }

        if (
            journal.HasInformation("bell_daniel_meeting_note") &&
            !HasAsked("frank_confronted_about_daniel_meeting")
        )
        {
            choices.Add(new DialogueChoice(
                "Ask Bell about his October 17 meeting with Daniel",
                "Mercer wanted to talk about the waterworks " +
                "contracts. He thought he'd found something " +
                "wrong with the figures.\n\n" +
                "I agreed to hear him out. That's all.\n\n" +
                "He was convinced somebody had been " +
                "signing off work that hadn't been done. " +
                "I told him the records were complicated " +
                "and he didn't understand how those " +
                "contracts operated.\n\n" +
                "We argued about it. That doesn't mean " +
                "anything happened to him.",
                () =>
                {
                    RecordQuestionOnce(
                        "frank_confronted_about_daniel_meeting");

                    RecordClaimOnce(
                        "frank_daniel_meeting_admission",
                        "Bell Admits Meeting Daniel",
                        "Frank admits meeting Daniel about disputed waterworks " +
                        "contracts. They argued, but Frank denies involvement in his " +
                        "disappearance.",
                        "Frank Bell");
                }
            ));
        }

        if (
            journal.HasInformation("frank_daniel_meeting_admission") &&
            journal.HasHypothesis("daniel_possible_platform_fall") &&
            !HasAsked("frank_confronted_about_argument_and_fall")
        )
        {
            choices.Add(new DialogueChoice(
                "Ask whether the argument ended at the platform",
                "We argued near the maintenance level. " +
                "Mercer kept insisting I was hiding " +
                "something in the records.\n\n" +
                "He wanted to see the lower inspection " +
                "area. I told him it wasn't safe.\n\n" +
                "He wouldn't listen.\n\n" +
                "And before you ask, no, I'm not saying " +
                "I pushed him. I didn't.",
                () =>
                {
                    RecordQuestionOnce(
                        "frank_confronted_about_argument_and_fall");

                    RecordClaimOnce(
                        "frank_platform_argument_admission",
                        "Bell Admits Arguing Near the Platform",
                        "Frank admits arguing with Daniel near the maintenance " +
                        "platform. He says Daniel wanted to inspect the lower area " +
                        "and denies pushing him.",
                        "Frank Bell");
                }
            ));
        }

        if (
            journal.HasInformation("frank_daniel_meeting_admission") &&
            journal.HasInformation("frank_platform_argument_admission") &&
            journal.HasHypothesis("daniel_possible_platform_fall") &&
            !HasAsked("frank_final_fall_confrontation")
        )
        {
            choices.Add(new DialogueChoice(
                "Confront Bell about what happened to Daniel",
                "You don't understand what that night was like.\n\n" +
                "Mercer came there convinced I'd been stealing " +
                "from the town. He had papers, figures, " +
                "all sorts of accusations.\n\n" +
                "I told him to leave. He wouldn't. " +
                "He kept pushing past me toward the " +
                "maintenance platform.\n\n" +
                "I grabbed his arm to stop him. " +
                "He pulled away. We struggled for a moment.\n\n" +
                "Then the railing gave way.\n\n" +
                "He went over before I could catch him.\n\n" +
                "I didn't push him off that platform. " +
                "I never meant for him to fall.\n\n" +
                "When I got down there, he wasn't moving.",
                () =>
                {
                    RecordQuestionOnce(
                        "frank_final_fall_confrontation");

                    RecordClaimOnce(
                        "frank_daniel_fall_confession",
                        "Bell Admits Witnessing Daniel's Fall",
                        "Frank admits grabbing Daniel during an argument near the " +
                        "platform. He claims Daniel pulled away, the railing failed, " +
                        "and the fall was accidental.",
                        "Frank Bell");
                }
            ));
        }

        if (
            journal.HasInformation("frank_daniel_fall_confession") &&
            !HasAsked("frank_asked_what_happened_after_fall")
        )
        {
            choices.Add(new DialogueChoice(
                "Ask Bell what he did after Daniel fell",
                "I panicked.\n\n" +
                "I thought he was dead. I didn't know " +
                "what to do.\n\n" +
                "There'd been an argument. I'd grabbed him. " +
                "Nobody was going to believe it was an accident.\n\n" +
                "I called for help.\n\n" +
                "That's all I'm going to say about that night.",
                () =>
                {
                    RecordQuestionOnce(
                        "frank_asked_what_happened_after_fall");

                    RecordClaimOnce(
                        "frank_called_for_help_after_fall",
                        "Bell Says He Called for Help",
                        "Frank says he panicked after Daniel fell and called for " +
                        "help, but refuses to say whom he contacted.",
                        "Frank Bell");
                }
            ));
        }

        if (
            journal.HasInformation("dwyer_concealment_confession") &&
            !HasAsked("frank_confronted_with_dwyer_confession")
        )
        {
            choices.Add(new DialogueChoice(
                "Confront Bell with Dwyer's confession",
                "He told you everything?\n\n" +
                "Then you know I never planned for " +
                "Mercer to die.\n\n" +
                "I was afraid. I thought one terrible " +
                "accident would destroy everything I'd built.\n\n" +
                "So I made it worse. Every decision " +
                "after that night made it worse.\n\n" +
                "I ordered the passage sealed. " +
                "I told the crew what to write down. " +
                "And I let Mercer's family spend seven " +
                "years wondering where he'd gone.\n\n" +
                "I can't defend that.",
                () =>
                {
                    RecordQuestionOnce(
                        "frank_confronted_with_dwyer_confession");

                    RecordClaimOnce(
                        "frank_concealment_admission",
                        "Bell Admits Ordering the Concealment",
                        "Frank admits ordering the passage sealed and controlling the " +
                        "maintenance report to conceal Daniel's death. He maintains " +
                        "the fall was accidental.",
                        "Frank Bell");
                }
            ));
        }


        choices.Add(new DialogueChoice(

            "Ask what kind of work the company handles now",
            "Drainage, repairs, road maintenance. Same work we've always done. " +
            "If you need a current job reference, the office has the files.",
            null
        ));

        DialogueManager.Instance.ShowDialogue(
            interactionName,
            greeting,
            choices
        );
    }

    // ---------------------------------------------------------
    // CALEB TURNER — TURNER'S REPAIR WORKSHOP
    // ---------------------------------------------------------

    private void InteractWithCaleb()
    {
        if (!ValidateDialogueManager())
            return;

        List<DialogueChoice> choices = new List<DialogueChoice>();
        JournalManager journal = JournalManager.Instance;

        bool hasFormerWorkers = journal != null &&
            journal.HasInformation("frank_former_workers");
        bool hasPersonnelRegister = journal != null &&
            journal.HasInformation("bell_personnel_register");
        bool hasDispatch = journal != null &&
            journal.HasInformation("bell_dispatch_sheet");
        bool hasBillingHypothesis = journal != null &&
            journal.HasHypothesis("billed_work_unverified");

        string greeting = HasAsked("caleb_asked_about_workshop") ||
            HasAsked("caleb_asked_about_bell")
            ? "Back again? I've still got work to finish, but go ahead."
            : "Something I can help you with? I've got a pump in pieces " +
              "here, so you'll have to excuse the mess.";

        if (!HasAsked("caleb_asked_about_workshop"))
        {
            choices.Add(new DialogueChoice(
                "Ask about his repair business",
                "Mostly pumps, motors, generators. Whatever people bring in. " +
                "It's not much of a business, but it's mine. I used to do " +
                "larger jobs when I worked for Bell.",
                () =>
                {
                    RecordQuestionOnce("caleb_asked_about_workshop");
                    RecordClaimOnce(
                        "caleb_workshop_history",
                        "Caleb's Repair Business",
                        "Caleb repairs pumps, motors, and generators independently " +
                        "after leaving Bell Municipal Services.",
                        "Caleb Turner");
                }
            ));
        }

        if (hasFormerWorkers && !HasAsked("caleb_asked_about_bell"))
        {
            choices.Add(new DialogueChoice(
                "Ask about his employment with Bell Municipal Services",
                "Worked there a good few years. Municipal contracts, " +
                "drainage repairs, that sort of thing. Left eventually. " +
                "Wanted to work for myself. That's about all there is to it.",
                () =>
                {
                    RecordQuestionOnce("caleb_asked_about_bell");
                    RecordClaimOnce(
                        "caleb_bell_employment",
                        "Caleb's Former Employment",
                        "Caleb confirms he worked on municipal drainage and " +
                        "maintenance jobs for Bell before leaving to work " +
                        "independently.",
                        "Caleb Turner");
                }
            ));
        }

        if (hasPersonnelRegister && !HasAsked("caleb_asked_about_work_order"))
        {
            choices.Add(new DialogueChoice(
                "Ask about waterworks job WW-91-184",
                "That number doesn't mean much to me, but I remember the old " +
                "waterworks. We were sent out to look at the drainage near " +
                "the lower access. I remember going there. Can't say I " +
                "remember us finishing everything on the order.",
                () =>
                {
                    RecordQuestionOnce("caleb_asked_about_work_order");
                    RecordClaimOnce(
                        "caleb_work_order_memory",
                        "Caleb's Waterworks Recollection",
                        "Caleb recalls working near the waterworks' lower access but " +
                        "cannot confirm that WW-91-184 was completed.",
                        "Caleb Turner");
                }
            ));
        }

        if (hasDispatch && !HasAsked("caleb_asked_about_equipment"))
        {
            choices.Add(new DialogueChoice(
                "Ask about the equipment dispatched to the waterworks",
                "A portable pump, lamps, the usual tools. The lower access " +
                "was damp, even when it hadn't rained. We took equipment " +
                "down there, but I couldn't tell you exactly how much " +
                "work we got through.",
                () =>
                {
                    RecordQuestionOnce("caleb_asked_about_equipment");
                    RecordClaimOnce(
                        "caleb_equipment_recollection",
                        "Caleb on the Dispatched Equipment",
                        "Caleb recalls taking a portable pump and lamps to the damp " +
                        "lower waterworks passage.",
                        "Caleb Turner");
                }
            ));
        }

        if (hasBillingHypothesis && !HasAsked("caleb_asked_about_completion"))
        {
            choices.Add(new DialogueChoice(
                "Ask about the missing completion confirmation",
                "I don't know what happened to the paperwork. Bell handled " +
                "the office side. We were there to do the work. If you're " +
                "asking whether I signed off on the job, I don't " +
                "remember doing that.",
                () =>
                {
                    RecordQuestionOnce("caleb_asked_about_completion");
                    RecordClaimOnce(
                        "caleb_completion_uncertainty",
                        "Caleb on the Completion Record",
                        "Caleb says Bell handled the paperwork. He does not remember " +
                        "signing off on the waterworks job.",
                        "Caleb Turner");
                }
            ));
        }

        // Follow-up: the personal equipment log does not record a pump return.
        bool hasEquipmentLog = journal != null &&
            journal.HasInformation("caleb_equipment_log");
        bool hasStoredPump = journal != null &&
            journal.HasInformation("caleb_stored_equipment");

        if (hasEquipmentLog && !HasAsked("caleb_asked_about_missing_pump"))
        {
            choices.Add(new DialogueChoice(
                "Ask why the pump's return was never recorded",
                "I remember that pump. Or at least I remember having trouble " +
                "getting the paperwork straight afterward. The lamp came " +
                "back damaged, and I wrote that down. The pump was another " +
                "matter. I asked about it, but nobody gave me a proper " +
                "answer. After a while, I stopped asking.",
                () =>
                {
                    RecordQuestionOnce("caleb_asked_about_missing_pump");
                    RecordClaimOnce(
                        "caleb_missing_pump_response",
                        "Caleb on the Missing Pump Return",
                        "Caleb recalls an unreturned pump and says he never received " +
                        "a clear explanation for the missing entry.",
                        "Caleb Turner");
                }
            ));
        }

        if (hasStoredPump && !HasAsked("caleb_asked_about_stored_pump"))
        {
            choices.Add(new DialogueChoice(
                "Ask why he still has equipment marked as Bell's",
                "That old thing? Bell was clearing out equipment when I " +
                "left. Some of it was scrap, some of it wasn't worth " +
                "hauling back. I took a few things for parts. That pump " +
                "hasn't run properly in years.",
                () =>
                {
                    RecordQuestionOnce("caleb_asked_about_stored_pump");
                    RecordClaimOnce(
                        "caleb_pump_ownership_account",
                        "Caleb on the Stored Pump",
                        "Caleb says he acquired used Bell equipment when he left, but " +
                        "cannot identify the workshop pump as the one from WW-91-184.",
                        "Caleb Turner");
                }
            ));
        }

        // The journal hypothesis links an unreturned pump with Bell-marked
        // equipment, but does not establish that they are the same unit.
        bool hasUnresolvedPumpHypothesis = journal != null &&
            journal.HasHypothesis("unresolved_pump_return");

        if (hasUnresolvedPumpHypothesis &&
            !HasAsked("caleb_asked_about_pump_connection"))
        {
            choices.Add(new DialogueChoice(
                "Ask whether the stored pump is the one missing from the waterworks job",
                "Could be the same model. Bell had more than one. I didn't " +
                "keep a list of everything I took when I left. Look, I " +
                "remember that job because of the lower access. The " +
                "equipment wasn't what stayed with me.",
                () =>
                {
                    RecordQuestionOnce("caleb_asked_about_pump_connection");
                    RecordClaimOnce(
                        "caleb_lower_access_recollection",
                        "Caleb on the Lower Access",
                        "Caleb remembers the waterworks' lower access more clearly " +
                        "than the equipment assigned there.",
                        "Caleb Turner");
                }
            ));
        }

        // Only available after Caleb himself brings up the lower access.
        if (HasAsked("caleb_asked_about_pump_connection") &&
            !HasAsked("caleb_asked_about_lower_access"))
        {
            choices.Add(new DialogueChoice(
                "Ask what he remembers about the lower access",
                "It was an old service passage below the main buildings. " +
                "Damp concrete, poor lighting. Parts of it hadn't been " +
                "used in years. We were supposed to check the drainage " +
                "down there. I remember the gate was difficult to open. " +
                "That's about all I can tell you.",
                () =>
                {
                    RecordQuestionOnce("caleb_asked_about_lower_access");
                    RecordClaimOnce(
                        "caleb_lower_access_description",
                        "Lower Waterworks Access",
                        "Caleb describes a poorly lit lower service passage with a " +
                        "heavy gate, used for drainage inspections.",
                        "Caleb Turner");
                }
            ));
        }

        // Follow-up unlocked by the waterworks barrier hypothesis.
        bool hasBarrierEvidence = journal != null &&
    journal.HasInformation("waterworks_service_barrier");
        bool hasBarrierRecollection = journal != null &&
            journal.HasInformation("caleb_barrier_recollection");

        if (hasBarrierEvidence && !hasBarrierRecollection)
        {
            choices.Add(new DialogueChoice(
                "Ask whether the lower service passage was sealed",
                "Sealed? No, I remember a gate down there. Heavy thing, " +
                "stuck half the time. I don't remember a solid panel. " +
                "But that was seven years ago. Could've been changed since.",
                () =>
                {
                    RecordQuestionOnce("caleb_asked_about_service_barrier");
                    RecordClaimOnce(
                        "caleb_barrier_recollection",
                        "Caleb's Recollection of the Service Gate",
                        "Caleb remembers a heavy gate where a solid barrier now " +
                        "blocks the lower passage. He cannot date the change.",
                        "Caleb Turner");
                }
            ));
        }

        if (
            journal.HasInformation("bell_original_barrier_work_order") &&
            !HasAsked("caleb_asked_about_oct17_night")
        )
        {
            choices.Add(new DialogueChoice(
                "Ask Caleb what happened before the barrier was sealed",
                "I wasn't supposed to be at the yard that night. " +
                "I'd left some tools behind and went back " +
                "to collect them.\n\n" +
                "Bell came in late. Dwyer was with him. " +
                "They didn't see me at first. I couldn't " +
                "hear everything they said, but Bell was " +
                "angry. Dwyer kept telling him to lower " +
                "his voice.\n\n" +
                "When Bell noticed me, he said the waterworks " +
                "job was changing. Told me not to report " +
                "there the next morning. Said he'd handle " +
                "the crew assignments himself.\n\n" +
                "The lower passage was sealed the next day. " +
                "I didn't think much of it then. " +
                "Later... I wasn't so sure.",
                () =>
                {
                    RecordQuestionOnce(
                        "caleb_asked_about_oct17_night");

                    RecordClaimOnce(
                        "caleb_oct17_yard_witness_account",
                        "Caleb's Account of October 17",
                        "Caleb says he saw Bell and Dwyer together at the yard late " +
                        "on October 17, 1991. Bell told him not to report for the " +
                        "next morning's waterworks job.",
                        "Caleb Turner");
                }
            ));
        }

        choices.Add(new DialogueChoice(
            "Ask what he repairs most often",
            "Pumps, mostly. People don't think about drainage until " +
            "something stops working.",
            null
        ));

        if (
            journal.HasInformation("dwyer_oct17_dispatch_slip") &&
            !HasAsked("caleb_asked_about_lower_access_directions")
        )
        {
            choices.Add(new DialogueChoice(
                "Ask how someone would find the lower service area",
                "Back then? You wouldn't just wander into it " +
                "from the front gate. The lower passage was " +
                "around the service side of the building. " +
                "There were maintenance routes, but they " +
                "weren't marked for visitors.\n\n" +
                "If you didn't know the place, you'd probably " +
                "need somebody to point you in the right direction.",
                () =>
                {
                    RecordQuestionOnce(
                        "caleb_asked_about_lower_access_directions");

                    RecordClaimOnce(
                        "caleb_lower_access_directions",
                        "Caleb on Access to the Lower Service Area",
                        "Caleb says the lower passage was reached by maintenance " +
                        "routes that visitors might struggle to find.",
                        "Caleb Turner");
                }
            ));
        }

        if (
            journal.HasInformation("daniel_remains_identification_report") &&
            journal.HasInformation("caleb_oct17_yard_witness_account") &&
            !HasAsked("caleb_confronted_about_silence")
        )
        {
            choices.Add(new DialogueChoice(
                "Ask Caleb why he stayed silent for seven years",
                "Because I was afraid.\n\n" +
                "Bell was my employer. Dwyer was a deputy. " +
                "I was just a maintenance worker who'd seen " +
                "something he wasn't supposed to see.\n\n" +
                "I kept telling myself there was an explanation. " +
                "That whatever happened at the waterworks " +
                "wasn't my business.\n\n" +
                "But I knew the lower passage had been sealed " +
                "right after Daniel disappeared. And I knew " +
                "Bell and Dwyer had been together that night.\n\n" +
                "Seven years of pretending not to know anything " +
                "does something to you.\n\n" +
                "That package you received... I sent it.",
                () =>
                {
                    RecordQuestionOnce(
                        "caleb_confronted_about_silence");

                    RecordClaimOnce(
                        "caleb_package_sender_confession",
                        "Caleb Admits Sending the Anonymous Package",
                        "Caleb admits sending the anonymous package. He suspected a " +
                        "connection between the sealed passage and Daniel's " +
                        "disappearance but feared speaking out.",
                        "Caleb Turner");
                }
            ));
        }

        if (
            journal.HasInformation("caleb_package_sender_confession") &&
            !HasAsked("caleb_asked_why_now")
        )
        {
            choices.Add(new DialogueChoice(
                "Ask Caleb why he sent the package now",
                "I kept thinking somebody else would come " +
                "forward. Somebody who knew more than I did.\n\n" +
                "Nobody ever did.\n\n" +
                "I started going through some old things " +
                "I'd kept from my time working for Bell. " +
                "It reminded me how much I'd tried to forget.\n\n" +
                "I didn't know what you'd find. I didn't " +
                "even know if Daniel was dead.\n\n" +
                "I just knew someone needed to start asking " +
                "questions again.",
                () =>
                {
                    RecordQuestionOnce("caleb_asked_why_now");

                    RecordClaimOnce(
                        "caleb_package_motivation",
                        "Caleb's Reason for Reopening the Case",
                        "Caleb revisited old Bell records and sent the package hoping " +
                        "an outside journalist would investigate. He did not know " +
                        "Daniel was dead.",
                        "Caleb Turner");
                }
            ));
        }

        if (
            journal.HasInformation("caleb_package_sender_confession") &&
            !HasAsked("caleb_asked_about_package_contents")
        )
        {
            choices.Add(new DialogueChoice(
                "Ask Caleb what he intended the package to reveal",
                "I wanted you to look at the waterworks. " +
                "At the records. At the dates.\n\n" +
                "I couldn't tell you what happened to Daniel, " +
                "because I didn't know. But I knew the official " +
                "story had left things out.\n\n" +
                "I thought if you started there, you'd find " +
                "something the rest of us had missed.",
                () =>
                {
                    RecordQuestionOnce(
                        "caleb_asked_about_package_contents");

                    RecordClaimOnce(
                        "caleb_package_intent",
                        "Caleb's Intended Lead",
                        "Caleb intended the package to draw attention to the " +
                        "waterworks and its records. He says he did not know how " +
                        "Daniel died.",
                        "Caleb Turner");
                }
            ));
        }


        DialogueManager.Instance.ShowDialogue(
            interactionName,
            greeting,
            choices
        );
    }

    // ---------------------------------------------------------
    // MICHAEL DWYER — GRAYFORD MAIN STREET
    // ---------------------------------------------------------

    private void InteractWithDwyer()
    {
        if (!ValidateDialogueManager())
            return;

        JournalManager journal = JournalManager.Instance;
        if (journal == null)
            return;

        List<DialogueChoice> choices =
            new List<DialogueChoice>();

        bool hasPoliceReport = journal.HasInformation(
            "daniel_original_police_report");

        bool hasEvelynLead = journal.HasHypothesis(
            "dwyer_unrecorded_waterworks_lead");

        bool hasBellAttendance = journal.HasHypothesis(
            "possible_oct17_attendance");

        bool hasInvestigationGap = journal.HasHypothesis(
            "original_investigation_gap");

        bool hasExaminationReport = journal.HasInformation(
            "waterworks_preliminary_examination_report");

        string greeting = HasAsked("dwyer_asked_about_case")
            ? "Still looking into the Mercer case? " +
              "I told you what I could."
            : "Deputy Michael Dwyer. " +
              "Can I help you with something?";

        // Basic introduction, available from the beginning.
        if (!HasAsked("dwyer_asked_about_case"))
        {
            choices.Add(new DialogueChoice(
                "Ask about Daniel Mercer's disappearance",
                "Mercer disappeared in October of '91. " +
                "I handled the initial inquiries. " +
                "We spoke to people who knew him, " +
                "checked what information we could. " +
                "Nobody could establish where he went. " +
                "The case was never closed.",
                () =>
                {
                    RecordQuestionOnce(
                        "dwyer_asked_about_case");

                    RecordClaimOnce(
                        "dwyer_original_case_account",
                        "Dwyer's Account of the Mercer Case",
                        "Dwyer says he investigated Daniel's disappearance. The " +
                        "inquiries were inconclusive and the case remained open.",
                        "Michael Dwyer");
                }
            ));
        }

        // The archived report establishes his official role.
        if (hasPoliceReport &&
            !HasAsked("dwyer_asked_about_departure"))
        {
            choices.Add(new DialogueChoice(
                "Ask why investigators thought Daniel left town",
                "We didn't establish that he left. " +
                "It was one possibility. " +
                "There was no confirmed sighting after " +
                "the seventeenth and nothing at the time " +
                "that gave us a definite alternative. " +
                "I wasn't going to call it a crime " +
                "without evidence.",
                () =>
                {
                    RecordQuestionOnce(
                        "dwyer_asked_about_departure");

                    RecordClaimOnce(
                        "dwyer_departure_assessment",
                        "Dwyer on Voluntary Departure",
                        "Dwyer says voluntary departure was only a possibility and " +
                        "that the original inquiry found no clear evidence of a " +
                        "crime.",
                        "Michael Dwyer");
                }
            ));
        }

        // Evelyn recalls telling Dwyer about the waterworks.
        if (hasEvelynLead &&
            !HasAsked("dwyer_asked_about_evelyn_lead"))
        {
            choices.Add(new DialogueChoice(
                "Ask why Evelyn's waterworks lead was not recorded",
                "Mrs. Shaw spoke to us, yes. " +
                "So did plenty of other people. " +
                "I don't remember every detail of every " +
                "conversation from seven years ago. " +
                "And you're looking at a summary report. " +
                "That doesn't mean every lead we received " +
                "appears in it.",
                () =>
                {
                    RecordQuestionOnce(
                        "dwyer_asked_about_evelyn_lead");

                    RecordClaimOnce(
                        "dwyer_evelyn_lead_response",
                        "Dwyer's Response to Evelyn's Recollection",
                        "Dwyer acknowledges interviewing Evelyn but claims the " +
                        "summary report would not include every lead.",
                        "Michael Dwyer");
                }
            ));
        }

        // Bell's attendance has documentary support.
        if (hasBellAttendance &&
            !HasAsked("dwyer_asked_about_bell"))
        {
            choices.Add(new DialogueChoice(
                "Ask whether Frank Bell was investigated",
                "Bell? He worked municipal contracts. " +
                "Of course he had reasons to visit " +
                "the waterworks. A name in a visitor " +
                "register doesn't tell you who someone " +
                "met or what they did there. " +
                "If you've got evidence connecting him " +
                "to Mercer, that's another matter.",
                () =>
                {
                    RecordQuestionOnce(
                        "dwyer_asked_about_bell");

                    RecordClaimOnce(
                        "dwyer_bell_attendance_response",
                        "Dwyer on Bell's Waterworks Attendance",
                        "Dwyer says Bell had legitimate reasons to visit the " +
                        "waterworks and does not confirm whether Bell was questioned.",
                        "Michael Dwyer");
                }
            ));
        }

        // Major confrontation after official findings.
        if (hasInvestigationGap &&
            hasExaminationReport &&
            !HasAsked("dwyer_asked_about_remains"))
        {
            choices.Add(new DialogueChoice(
                "Confront Dwyer about the remains in Chamber L-02",
                "I've heard about the examination. " +
                "If those remains have been there " +
                "since '91, then the case has changed. " +
                "But nobody's identified them yet. " +
                "And you can't judge an entire investigation " +
                "by what turns up seven years later. " +
                "I'll answer questions through the " +
                "proper channels.",
                () =>
                {
                    RecordQuestionOnce(
                        "dwyer_asked_about_remains");

                    RecordClaimOnce(
                        "dwyer_chamber_discovery_response",
                        "Dwyer's Response to the Chamber Discovery",
                        "Dwyer acknowledges that the unidentified remains in Chamber " +
                        "L-02 may change the investigation but declines further " +
                        "comment.",
                        "Michael Dwyer");
                }
            ));
        }

        choices.Add(new DialogueChoice(
            "Ask whether the case is still open",
            "It was never formally resolved. " +
            "That doesn't mean there was nothing " +
            "done about it. Sometimes you follow " +
            "every lead you've got and still " +
            "come up empty.",
            null
        ));

        if (
            journal.HasHypothesis("bell_dwyer_oct17_overlap") &&
            !HasAsked("dwyer_asked_about_oct17_dispatch")
        )
        {
            choices.Add(new DialogueChoice(
                "Question Dwyer about his October 17 waterworks dispatch",
                "That was seven years ago. A site security call " +
                "could've been anything — an open gate, a trespasser, " +
                "somebody reporting damage. I don't remember the " +
                "details of that particular call. And Bell worked " +
                "municipal contracts. His name being in a register " +
                "doesn't mean we had anything to do with each other.",
                () =>
                {
                    RecordQuestionOnce(
                        "dwyer_asked_about_oct17_dispatch");

                    RecordClaimOnce(
                        "dwyer_oct17_dispatch_response",
                        "Dwyer's Account of the October 17 Dispatch",
                        "Dwyer acknowledges the October 17 dispatch record but claims " +
                        "not to remember the call or meeting Bell.",
                        "Michael Dwyer");
                }
            ));
        }

        if (
            journal.HasHypothesis("dwyer_lower_access_dispatch") &&
            !HasAsked("dwyer_asked_about_lower_access_call")
        )
        {
            choices.Add(new DialogueChoice(
                "Question Dwyer about the lower-service-area call",
                "The lower service area? If that's what the " +
                "dispatch slip says, then that's what the " +
                "operator recorded. But a caller mentioning " +
                "a location doesn't mean I went into every " +
                "part of the building.\n\n" +
                "I don't remember who was there when I arrived. " +
                "Seven years is a long time to expect someone " +
                "to recall an ordinary security call.",
                () =>
                {
                    RecordQuestionOnce(
                        "dwyer_asked_about_lower_access_call");

                    RecordClaimOnce(
                        "dwyer_lower_access_call_response",
                        "Dwyer on the Lower-Access Dispatch",
                        "Dwyer admits the dispatch concerned the lower service area " +
                        "but claims not to remember whom he encountered.",
                        "Michael Dwyer");
                }
            ));
        }

        if (
            journal.HasHypothesis("dwyer_lower_access_dispatch") &&
            !HasAsked("dwyer_asked_about_access_route")
        )
        {
            choices.Add(new DialogueChoice(
                "Ask how Dwyer found the lower service area",
                "You're assuming I went down there. " +
                "I was dispatched to the waterworks, " +
                "not ordered to inspect every maintenance " +
                "passage in the building.\n\n" +
                "I couldn't tell you which entrance I used " +
                "seven years ago. If someone gave me directions, " +
                "I don't remember who it was.",
                () =>
                {
                    RecordQuestionOnce(
                        "dwyer_asked_about_access_route");

                    RecordClaimOnce(
                        "dwyer_access_route_response",
                        "Dwyer on His Route Through the Waterworks",
                        "Dwyer disputes having entered the lower passage and cannot " +
                        "recall which entrance he used.",
                        "Michael Dwyer");
                }
            ));
        }

        if (
            journal.HasHypothesis("dwyer_missing_equipment_connection") &&
            !HasAsked("dwyer_asked_about_missing_lamp")
        )
        {
            choices.Add(new DialogueChoice(
                "Confront Dwyer about the missing inspection lamp",
                "That lamp? I remember carrying one on patrol. " +
                "We used them for inspections, traffic calls, " +
                "anything that needed extra light.\n\n" +
                "If the register says it wasn't returned, " +
                "then it wasn't accounted for. Equipment " +
                "went missing sometimes. That doesn't mean " +
                "I left it at the waterworks.\n\n" +
                "And you've got part of a tag, not the lamp. " +
                "You can't even read the whole number.",
                () =>
                {
                    RecordQuestionOnce(
                        "dwyer_asked_about_missing_lamp");

                    RecordClaimOnce(
                        "dwyer_missing_lamp_response",
                        "Dwyer on His Missing Inspection Lamp",
                        "Dwyer acknowledges using an inspection lamp but says the " +
                        "partial GSD tag cannot identify his missing equipment.",
                        "Michael Dwyer");
                }
            ));
        }

        if (
            journal.HasInformation("dwyer_lamp_identified") &&
            !HasAsked("dwyer_confronted_with_identified_lamp")
        )
        {
            choices.Add(new DialogueChoice(
                "Confront Dwyer with the identified inspection lamp",
                "You found the whole lamp?\n\n" +
                "Look, I was sent to the waterworks that night. " +
                "I've never denied that. Maybe I brought the " +
                "lamp with me. Maybe I left it somewhere " +
                "during the call.\n\n" +
                "But I don't remember going into that chamber. " +
                "And I certainly don't know how the lamp " +
                "ended up inside it.",
                () =>
                {
                    RecordQuestionOnce(
                        "dwyer_confronted_with_identified_lamp");

                    RecordClaimOnce(
                        "dwyer_identified_lamp_response",
                        "Dwyer Confronted with His Inspection Lamp",
                        "Confronted with lamp GSD-2714 from Chamber L-02, Dwyer " +
                        "concedes he may have brought it to the waterworks but denies " +
                        "remembering the chamber.",
                        "Michael Dwyer");
                }
            ));
        }

        if (
            journal.HasInformation("caleb_oct17_yard_witness_account") &&
            !HasAsked("dwyer_confronted_about_yard_meeting")
        )
        {
            choices.Add(new DialogueChoice(
                "Confront Dwyer about meeting Bell that night",
                "Turner says he saw us together?\n\n" +
                "I might've spoken with Bell after the " +
                "waterworks call. He was responsible for " +
                "maintenance at the site. That wouldn't " +
                "have been unusual.\n\n" +
                "But I don't remember going to his yard. " +
                "And even if I did, it doesn't mean " +
                "there was anything improper about it.\n\n" +
                "You're building an accusation out of " +
                "a conversation nobody can properly recall.",
                () =>
                {
                    RecordQuestionOnce(
                        "dwyer_confronted_about_yard_meeting");

                    RecordClaimOnce(
                        "dwyer_yard_meeting_response",
                        "Dwyer's Response to Caleb's Testimony",
                        "Dwyer concedes he may have spoken with Bell after the " +
                        "dispatch but claims not to remember visiting the yard.",
                        "Michael Dwyer");
                }
            ));
        }

        if (
            journal.HasInformation("daniel_remains_identification_report") &&
            !HasAsked("dwyer_confronted_about_daniel_remains")
        )
        {
            choices.Add(new DialogueChoice(
                "Confront Dwyer about Daniel's identification",
                "Then the case is no longer just a " +
                "missing-person investigation.\n\n" +
                "But identification isn't a finding " +
                "of homicide. The medical examiner " +
                "hasn't established how Mercer died.\n\n" +
                "As for that chamber, I've told you " +
                "what I remember about the dispatch. " +
                "I can't add anything to it.",
                () =>
                {
                    RecordQuestionOnce(
                        "dwyer_confronted_about_daniel_remains");

                    RecordClaimOnce(
                        "dwyer_daniel_remains_response",
                        "Dwyer on the Identification of Daniel",
                        "Dwyer acknowledges Daniel's remains were identified in L-02 " +
                        "but maintains he cannot recall the October 17 dispatch " +
                        "details.",
                        "Michael Dwyer");
                }
            ));
        }

        if (
            journal.HasInformation("frank_called_for_help_after_fall") &&
            !HasAsked("dwyer_confronted_about_bell_help_call")
        )
        {
            choices.Add(new DialogueChoice(
                "Ask Dwyer whether Bell called him after Daniel fell",
                "Bell told you that?\n\n" +
                "He said he called for help, and you're " +
                "assuming that means me.\n\n" +
                "I was dispatched to the waterworks. " +
                "That's in the record. But you're asking " +
                "me to confirm a conversation I don't " +
                "remember having.\n\n" +
                "If Bell has something to say about " +
                "that night, he can say it himself.",
                () =>
                {
                    RecordQuestionOnce(
                        "dwyer_confronted_about_bell_help_call");

                    RecordClaimOnce(
                        "dwyer_bell_help_call_response",
                        "Dwyer Responds to Bell's Admission",
                        "Dwyer refuses to confirm Bell called him after the fall, " +
                        "despite his documented waterworks dispatch.",
                        "Michael Dwyer");
                }
            ));
        }

        if (
            journal.HasInformation("frank_daniel_fall_confession") &&
            journal.HasInformation("dwyer_lamp_identified") &&
            journal.HasInformation("caleb_oct17_yard_witness_account") &&
            !HasAsked("dwyer_final_confrontation_stage_one")
        )
        {
            choices.Add(new DialogueChoice(
                "Confront Dwyer with Bell's confession and the lamp",
                "Bell told you about the fall?\n\n" +
                "Then I suppose there's no point pretending " +
                "I don't remember that night.\n\n" +
                "I arrived at the waterworks after the call. " +
                "Bell was waiting near the service entrance. " +
                "He was shaken. Kept saying there'd been " +
                "an accident.\n\n" +
                "He took me down to the lower inspection area. " +
                "Mercer was lying there. He wasn't moving.\n\n" +
                "I checked him. I couldn't find a pulse.\n\n" +
                "Bell said they'd argued. That Mercer had " +
                "fallen through the railing.\n\n" +
                "I should've called it in properly. " +
                "I know that now.",
                () =>
                {
                    RecordQuestionOnce(
                        "dwyer_final_confrontation_stage_one");

                    RecordClaimOnce(
                        "dwyer_witnessed_daniel_after_fall",
                        "Dwyer Admits Finding Daniel After the Fall",
                        "Dwyer admits Bell led him to an unresponsive Daniel at the " +
                        "waterworks on October 17, 1991. He failed to report the " +
                        "incident.",
                        "Michael Dwyer");
                }
            ));
        }

        if (
            journal.HasInformation("dwyer_witnessed_daniel_after_fall") &&
            !HasAsked("dwyer_final_confrontation_stage_two")
        )
        {
            choices.Add(new DialogueChoice(
                "Ask Dwyer why Daniel was hidden in Chamber L-02",
                "Bell was terrified. He thought he'd be " +
                "arrested for killing Mercer.\n\n" +
                "He kept saying it was an accident. " +
                "That nobody would believe him.\n\n" +
                "And I believed him. Or I wanted to.\n\n" +
                "We moved Mercer into the old inspection " +
                "chamber. I helped him do it.\n\n" +
                "Bell said he'd have the passage closed. " +
                "That the maintenance records would make " +
                "it look routine.\n\n" +
                "When Mercer's disappearance was reported, " +
                "I handled the initial inquiries. " +
                "I knew where he was. I knew he hadn't " +
                "simply left town.\n\n" +
                "I kept the waterworks out of the report. " +
                "I let everyone believe there was no " +
                "reason to suspect anything else.\n\n" +
                "I told myself it was too late to undo " +
                "what we'd done.\n\n" +
                "Seven years. That's how long I kept " +
                "telling myself that.",
                () =>
                {
                    RecordQuestionOnce(
                        "dwyer_final_confrontation_stage_two");

                    RecordClaimOnce(
                        "dwyer_concealment_confession",
                        "Dwyer Confesses to Concealing Daniel's Death",
                        "Dwyer admits helping Bell move Daniel's body into Chamber " +
                        "L-02 and omitting the waterworks lead from his " +
                        "investigation. Bell arranged the closure.",
                        "Michael Dwyer");
                }
            ));
        }


        DialogueManager.Instance.ShowDialogue(
            interactionName,
            greeting,
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
        SceneSpawnManager.SetNextSpawn(destinationSpawnID);
        SceneManager.LoadScene(destinationScene);
    }
}
