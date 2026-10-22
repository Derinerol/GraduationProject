using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores an NPC's knowledge, remembered events and asked questions.
/// Runtime changes survive scene reloads within the current play session.
/// </summary>
public class NPCMemory : MonoBehaviour
{
    [Header("Persistent Identity")]
    [SerializeField] private string npcID = "";

    [Header("Information This NPC Knows")]
    [SerializeField] private List<string> knownInformationIDs = new();

    [Header("Events This NPC Remembers")]
    [SerializeField] private List<string> rememberedEventIDs = new();

    [Header("Questions Previously Asked")]
    [SerializeField] private List<string> askedQuestionIDs = new();

    private HashSet<string> knownInformation;
    private HashSet<string> rememberedEvents;
    private HashSet<string> askedQuestions;

    private class MemoryState
    {
        public readonly HashSet<string> KnownInformation = new();
        public readonly HashSet<string> RememberedEvents = new();
        public readonly HashSet<string> AskedQuestions = new();
    }

    private static readonly Dictionary<string, MemoryState> savedMemories = new();

    // Unity resets static fields at the beginning of a new Play session,
    // including when Enter Play Mode Options disables domain reload.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeMemories()
    {
        savedMemories.Clear();
    }

    private void Awake()
    {
        knownInformation = new HashSet<string>(knownInformationIDs);
        rememberedEvents = new HashSet<string>(rememberedEventIDs);
        askedQuestions = new HashSet<string>(askedQuestionIDs);

        if (string.IsNullOrWhiteSpace(npcID))
        {
            Debug.LogWarning($"NPCMemory on '{name}' has no NPC ID. Its memory won't persist between scenes.", this);
            return;
        }

        if (savedMemories.TryGetValue(npcID, out MemoryState saved))
        {
            knownInformation.UnionWith(saved.KnownInformation);
            rememberedEvents.UnionWith(saved.RememberedEvents);
            askedQuestions.UnionWith(saved.AskedQuestions);
            SyncInspectorLists();
        }
        else
        {
            SaveCurrentState();
        }
    }

    public bool KnowsInformation(string informationID) => HasID(knownInformation, informationID);
    public bool RemembersEvent(string eventID) => HasID(rememberedEvents, eventID);
    public bool HasBeenAsked(string questionID) => HasID(askedQuestions, questionID);

    public bool LearnInformation(string informationID)
    {
        if (!AddID(knownInformation, informationID)) return false;
        knownInformationIDs.Add(informationID);
        SaveCurrentState();
        return true;
    }

    public bool RememberEvent(string eventID)
    {
        if (!AddID(rememberedEvents, eventID)) return false;
        rememberedEventIDs.Add(eventID);
        SaveCurrentState();
        return true;
    }

    public bool RecordQuestion(string questionID)
    {
        if (!AddID(askedQuestions, questionID)) return false;
        askedQuestionIDs.Add(questionID);
        SaveCurrentState();
        return true;
    }

    private void SaveCurrentState()
    {
        if (string.IsNullOrWhiteSpace(npcID)) return;

        if (!savedMemories.TryGetValue(npcID, out MemoryState state))
        {
            state = new MemoryState();
            savedMemories.Add(npcID, state);
        }

        state.KnownInformation.UnionWith(knownInformation);
        state.RememberedEvents.UnionWith(rememberedEvents);
        state.AskedQuestions.UnionWith(askedQuestions);
    }

    private void SyncInspectorLists()
    {
        knownInformationIDs = new List<string>(knownInformation);
        rememberedEventIDs = new List<string>(rememberedEvents);
        askedQuestionIDs = new List<string>(askedQuestions);
    }

    private static bool HasID(HashSet<string> collection, string id)
    {
        return !string.IsNullOrWhiteSpace(id) && collection != null && collection.Contains(id);
    }

    private static bool AddID(HashSet<string> collection, string id)
    {
        return !string.IsNullOrWhiteSpace(id) && collection != null && collection.Add(id);
    }
}
