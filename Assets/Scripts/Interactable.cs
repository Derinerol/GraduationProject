
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
        CalebTurner
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
                        "Arthur remembers Daniel's mother visiting the Gazette to ask " +
                        "whether the paper had information beyond the sheriff's office.",
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
                        "Arthur says the Gazette relied on the sheriff's office " +
                        "for its assessment and had no independent witness " +
                        "to Daniel's departure.",
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
                        "Arthur says the Gazette stopped reporting on Daniel " +
                        "because it had no new information it could substantiate.",
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
                        "Arthur acknowledges the Gazette could not verify Daniel's " +
                        "departure and that readers may have treated the sheriff's " +
                        "assessment as an established conclusion.",
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
                        "Arthur suggests checking the municipal records office " +
                        "for the original waterworks paperwork. He doesn't " +
                        "recall the Gazette covering that work order.",
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
                        "Helen says Public Works retains registers and status " +
                        "ledgers, while contractor paperwork is indexed separately.",
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
                        "Helen confirms that the register records authorization " +
                        "for WW-91-184, not completion of the work.",
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
                        "Helen says the blank completion field cannot establish " +
                        "whether the work was performed.",
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
                        "Helen identifies Bell Municipal Services as the next " +
                        "place to ask about supporting work documentation, " +
                        "without confirming that such a report exists.",
                        "Helen Ward");
                }
            ));
        }

        if (hasHypothesis && !HasAsked("helen_asked_about_discrepancy"))
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
                        "Helen agrees the available records do not resolve " +
                        "whether the waterworks job was completed. She cannot " +
                        "verify what happened at the site.",
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
                        "Frank says his company has handled municipal maintenance " +
                        "work for Grayford for many years.",
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
                        "Frank says his company handled waterworks maintenance " +
                        "but cannot recall the details of WW-91-184.",
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
                        "Frank acknowledges that the sheet records a crew assignment " +
                        "but says a field report would be needed to establish " +
                        "what happened on site.",
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
                        "Frank describes the invoice as routine billing for the " +
                        "waterworks job and distinguishes it from a field report.",
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
                        "Frank attributes the missing completion confirmation to " +
                        "incomplete records and rejects the suggestion of improper billing.",
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
                        "Frank says he does not remember Daniel Mercer approaching " +
                        "Bell Municipal Services about the waterworks.",
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
                        "Frank says R. Hayes left Grayford and Caleb Turner " +
                        "no longer works for Bell Municipal Services. He " +
                        "believes Caleb may still do independent maintenance " +
                        "work locally.",
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
                        "Caleb runs an independent repair workshop, working " +
                        "mainly on pumps, motors, and generators. He previously " +
                        "handled larger jobs while employed by Bell.",
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
                        "Caleb confirms that he previously worked for Bell " +
                        "Municipal Services on municipal maintenance and " +
                        "drainage jobs. He says he left to work independently.",
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
                        "Caleb recalls going to the old waterworks for a " +
                        "lower-access drainage assignment, but cannot confirm " +
                        "that all the assigned work was finished.",
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
                        "Caleb recalls taking a portable pump and lamps to " +
                        "the damp lower-access area at the waterworks. He " +
                        "does not specify how much work was completed.",
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
                        "Caleb says Bell handled the office paperwork and " +
                        "does not recall personally signing off on the " +
                        "waterworks assignment.",
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
                        "Caleb recalls following up on a pump that lacked a " +
                        "recorded return after the 1991 waterworks assignment. " +
                        "He says nobody gave him a clear answer about it.",
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
                        "Caleb says he acquired old Bell equipment when he " +
                        "left the company and kept some for parts. He does " +
                        "not establish whether this is the pump assigned " +
                        "to WW-91-184.",
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
                        "When questioned about the missing pump, Caleb says " +
                        "Bell owned several similar pumps. He recalls the " +
                        "lower waterworks access more clearly than the " +
                        "equipment but does not explain why.",
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
                        "Caleb describes an old service passage beneath the " +
                        "waterworks buildings, with poor lighting and a " +
                        "difficult gate. He says the crew was assigned to " +
                        "inspect drainage there.",
                        "Caleb Turner");
                }
            ));
        }

        // Follow-up unlocked by the waterworks barrier hypothesis.
        bool hasBarrierHypothesis = journal != null &&
            journal.HasHypothesis("unexplained_service_barrier");
        bool hasBarrierRecollection = journal != null &&
            journal.HasInformation("caleb_barrier_recollection");

        if (hasBarrierHypothesis && !hasBarrierRecollection)
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
                        "Caleb Turner recalls a heavy gate in the lower waterworks " +
                        "service passage during his employment with Bell Municipal " +
                        "Services. He does not remember the solid barrier currently " +
                        "present at the site, although he acknowledges that the " +
                        "passage could have been modified after he last visited. " +
                        "His recollection does not establish when the existing " +
                        "barrier was installed.",
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
