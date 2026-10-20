
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores significant knowledge and interactions for one NPC.
/// Add this component to each NPC that needs persistent
/// memory during the current play session.
/// </summary>
public class NPCMemory : MonoBehaviour
{
    [Header("Information This NPC Knows")]
    [SerializeField] private List<string> knownInformationIDs = new();

    [Header("Events This NPC Remembers")]
    [SerializeField] private List<string> rememberedEventIDs = new();

    [Header("Questions Previously Asked")]
    [SerializeField] private List<string> askedQuestionIDs = new();

    private HashSet<string> knownInformation;
    private HashSet<string> rememberedEvents;
    private HashSet<string> askedQuestions;

    private void Awake()
    {
        // Build fast lookup collections from the Inspector lists.
        knownInformation = new HashSet<string>(knownInformationIDs);
        rememberedEvents = new HashSet<string>(rememberedEventIDs);
        askedQuestions = new HashSet<string>(askedQuestionIDs);
    }

    public bool KnowsInformation(string informationID)
    {
        return HasID(knownInformation, informationID);
    }

    public bool RemembersEvent(string eventID)
    {
        return HasID(rememberedEvents, eventID);
    }

    public bool HasBeenAsked(string questionID)
    {
        return HasID(askedQuestions, questionID);
    }

    /// <summary>
    /// Records information disclosed to this NPC.
    /// Returns true only if this is new information for them.
    /// </summary>
    public bool LearnInformation(string informationID)
    {
        if (!AddID(knownInformation, informationID))
            return false;

        knownInformationIDs.Add(informationID);
        return true;
    }

    /// <summary>
    /// Records a significant event experienced by this NPC.
    /// Returns true only the first time the event is recorded.
    /// </summary>
    public bool RememberEvent(string eventID)
    {
        if (!AddID(rememberedEvents, eventID))
            return false;

        rememberedEventIDs.Add(eventID);
        return true;
    }

    /// <summary>
    /// Records a question so it can be asked only once
    /// if the dialogue design requires that behavior.
    /// </summary>
    public bool RecordQuestion(string questionID)
    {
        if (!AddID(askedQuestions, questionID))
            return false;

        askedQuestionIDs.Add(questionID);
        return true;
    }

    private static bool HasID(HashSet<string> collection, string id)
    {
        return !string.IsNullOrWhiteSpace(id)
            && collection != null
            && collection.Contains(id);
    }

    private static bool AddID(HashSet<string> collection, string id)
    {
        return !string.IsNullOrWhiteSpace(id)
            && collection != null
            && collection.Add(id);
    }
}
