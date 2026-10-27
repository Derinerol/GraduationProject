#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Read-only editor audit. Does not execute gameplay or modify assets.
public static class InvestigationDeepAuditExporter
{
    static readonly List<string[]> objects = new List<string[]>();
    static readonly List<string[]> fields = new List<string[]>();
    static readonly List<string[]> definitions = new List<string[]>();
    static readonly List<string[]> code = new List<string[]>();
    static readonly List<string[]> issues = new List<string[]>();
    static readonly List<string[]> readable = new List<string[]>();
    static readonly HashSet<string> sceneNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    static readonly string[] idFields = { "informationID", "evidenceID", "requiredInformationID", "requiredInformationID1", "requiredInformationID2", "hypothesisID", "informationEntryID" };
    static readonly string[] trackedCalls = { "HasInformation", "HasHypothesis", "HasAsked", "KnowsInformation", "RecordClaim", "RecordClaimOnce", "AddEntry", "LearnInformationOnce", "RecordQuestionOnce", "RememberEventOnce" };

    [MenuItem("Tools/Seven Years Quiet/Export Deep Investigation Audit")]
    public static void Export()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        string folder = EditorUtility.SaveFolderPanel("Choose deep audit output folder", Application.dataPath, "");
        if (string.IsNullOrEmpty(folder)) return;
        objects.Clear(); fields.Clear(); definitions.Clear(); code.Clear(); issues.Clear(); readable.Clear(); sceneNames.Clear();
        var setup = EditorSceneManager.GetSceneManagerSetup();
        int scenes = 0, prefabs = 0;
        try
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
            {
                string asset = AssetDatabase.GUIDToAssetPath(guid);
                sceneNames.Add(Path.GetFileNameWithoutExtension(asset));
                try
                {
                    Scene scene = EditorSceneManager.OpenScene(asset, OpenSceneMode.Single);
                    foreach (GameObject root in scene.GetRootGameObjects()) ScanTree(root, asset, "Scene");
                    scenes++;
                }
                catch (Exception ex) { Issue("SCAN_ERROR", "High", asset, "", ex.ToString()); }
            }
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
            {
                string asset = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = null;
                try
                {
                    root = PrefabUtility.LoadPrefabContents(asset);
                    ScanTree(root, asset, "Prefab");
                    prefabs++;
                }
                catch (Exception ex) { Issue("SCAN_ERROR", "High", asset, "", ex.ToString()); }
                finally { if (root != null) PrefabUtility.UnloadPrefabContents(root); }
            }
        }
        finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        ScanSourceCode();
        Analyze();
        Write(folder, "SceneObjects.csv", new[] { "Asset", "Kind", "HierarchyPath", "ActiveInHierarchy", "Component", "Enabled", "NPCProfile", "InformationSource", "SceneExit", "DestinationScene", "DestinationSpawnID", "HasNPCMemory" }, objects);
        Write(folder, "SerializedFields.csv", new[] { "Asset", "Kind", "HierarchyPath", "Component", "PropertyPath", "Value" }, fields);
        Write(folder, "HypothesisDefinitions.csv", new[] { "Asset", "Kind", "HierarchyPath", "HypothesisID", "JournalEntryID", "Title", "RequiredID1", "RequiredID2", "Description" }, definitions);
        Write(folder, "CodeReferences.csv", new[] { "Script", "Line", "Call", "LiteralFirstArgument", "Resolution", "Snippet" }, code);
        Write(folder, "ReadableEvidence.csv", new[] { "Asset", "Kind", "HierarchyPath", "Component", "Title", "EvidenceID", "RequiredInformationID", "JournalNote", "UnavailableMessage", "Body" }, readable);
        Write(folder, "AuditFindings.csv", new[] { "Finding", "Severity", "Location", "ID", "Explanation" }, issues);
        Write(folder, "Summary.csv", new[] { "Metric", "Value" }, new[] {
            new[] { "ScenesScanned", scenes.ToString() }, new[] { "PrefabsScanned", prefabs.ToString() },
            new[] { "SceneObjectComponents", objects.Count.ToString() }, new[] { "SerializedFields", fields.Count.ToString() },
            new[] { "HypothesisDefinitions", definitions.Count.ToString() }, new[] { "CodeReferences", code.Count.ToString() },
            new[] { "ReadableObjects", readable.Count.ToString() }, new[] { "Findings", issues.Count.ToString() },
            new[] { "AuditScope", "Static analysis only; runtime conditions, scene transitions, NPC state, and dynamically computed IDs require targeted tests." }
        });
        Debug.Log($"Deep investigation audit: {scenes} scenes, {prefabs} prefabs, {definitions.Count} hypothesis definitions, {code.Count} code references, {issues.Count} findings. Output: {folder}");
        EditorUtility.RevealInFinder(folder);
    }

    static void ScanTree(GameObject root, string asset, string kind)
    {
        foreach (MonoBehaviour mb in root.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb == null)
            {
                Issue("MISSING_SCRIPT", "High", asset + ":" + PathOf(root.transform), "", "Missing MonoBehaviour component found in scanned hierarchy.");
                continue;
            }
            string component = mb.GetType().Name;
            string path = PathOf(mb.transform);
            SerializedObject so = new SerializedObject(mb);
            bool relevant = mb is Interactable || mb is JournalManager || component.Contains("Waterworks") || component.Contains("Lamp") || component.Contains("Recess") || component.Contains("EquipmentTag") || component.Contains("NPCMemory") || component.Contains("Evidence") || component.Contains("Record");
            if (!relevant) continue;
            objects.Add(new[] { asset, kind, path, mb.gameObject.activeInHierarchy.ToString(), component, mb.enabled.ToString(), Val(so, "npcProfile"), Val(so, "isInformationSource"), Val(so, "isSceneExit"), Val(so, "destinationScene"), Val(so, "destinationSpawnID"), (mb.GetComponent<NPCMemory>() != null).ToString() });
            if (mb is Interactable && Val(so, "isSceneExit") == "True")
            {
                string destination = Val(so, "destinationScene");
                if (string.IsNullOrWhiteSpace(destination)) Issue("EMPTY_DESTINATION", "High", asset + ":" + path, "", "Scene exit has no destination scene.");
            }
            if (mb is Interactable && Val(so, "isInformationSource") == "True" && string.IsNullOrWhiteSpace(Val(so, "informationID")))
                Issue("EMPTY_INFORMATION_ID", "High", asset + ":" + path, "", "Information source has no informationID.");
            SerializedProperty p = so.GetIterator();
            bool enter = true;
            while (p.NextVisible(enter))
            {
                enter = false;
                if (p.propertyPath == "m_Script") continue;
                if (p.propertyType == SerializedPropertyType.String)
                    fields.Add(new[] { asset, kind, path, component, p.propertyPath, p.stringValue });
                else if (p.propertyType == SerializedPropertyType.Enum && p.depth <= 1)
                    fields.Add(new[] { asset, kind, path, component, p.propertyPath, p.enumValueIndex >= 0 && p.enumValueIndex < p.enumNames.Length ? p.enumNames[p.enumValueIndex] : "" });
            }
            if (mb is JournalManager)
            {
                var list = so.FindProperty("hypothesisDefinitions");
                if (list != null && list.isArray)
                {
                    for (int i = 0; i < list.arraySize; i++)
                    {
                        var d = list.GetArrayElementAtIndex(i);
                        definitions.Add(new[] { asset, kind, path, Child(d, "hypothesisID"), Child(d, "informationEntryID"), Child(d, "title"), Child(d, "requiredInformationID1"), Child(d, "requiredInformationID2"), Child(d, "description") });
                    }
                }
            }
            if (mb is MunicipalRecord)
                readable.Add(new[] { asset, kind, path, component, Val(so, "documentTitle"), Val(so, "evidenceID"), Val(so, "requiredInformationID"), Val(so, "journalNote"), Val(so, "unavailableMessage"), Val(so, "documentBody") });
            if (mb is NewspaperEvidence)
                readable.Add(new[] { asset, kind, path, component, Val(so, "headline"), Val(so, "evidenceID"), "", Val(so, "journalNote"), "", Val(so, "articleBody") });
        }
    }

    static void ScanSourceCode()
    {
        // Include all project scripts, not only Interactable.cs. This catches waterworks scripts and helper systems.
        foreach (string guid in AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) continue;
            try
            {
                string source = File.ReadAllText(Path.GetFullPath(path));
                string[] lines = source.Split('\n');
                // Literal first arguments can be on later lines. Comments/strings may create false positives: review snippets.
                var rx = new Regex(@"\b(?<call>HasInformation|HasHypothesis|HasAsked|KnowsInformation|RecordClaim|RecordClaimOnce|AddEntry|LearnInformationOnce|RecordQuestionOnce|RememberEventOnce)\s*\(\s*(?<arg>""(?:[^""\\]|\\.)*""|[A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Multiline);
                foreach (Match m in rx.Matches(source))
                {
                    int line = 1;
                    for (int j = 0; j < m.Index; j++) if (source[j] == '\n') line++;
                    string arg = m.Groups["arg"].Value;
                    bool literal = arg.Length >= 2 && arg[0] == '"';
                    string id = literal ? arg.Substring(1, arg.Length - 2) : arg;
                    string snippet = string.Join(" ", lines.Skip(Math.Max(0, line - 1)).Take(3).Select(x => x.Trim()).ToArray());
                    code.Add(new[] { path, line.ToString(), m.Groups["call"].Value, id, literal ? "Literal" : "Dynamic or variable; manual review", snippet });
                }
            }
            catch (Exception ex) { Issue("SOURCE_SCAN_ERROR", "Medium", path, "", ex.Message); }
        }
    }

    static void Analyze()
    {
        // All possible producers, including NPC and specialized script AddEntry/RecordClaim calls.
        var producers = new HashSet<string>(StringComparer.Ordinal);
        foreach (var f in fields)
            if ((f[4] == "informationID" && FieldValue(f, "isInformationSource") ) || f[4] == "evidenceID")
                if (!string.IsNullOrWhiteSpace(f[5])) producers.Add(f[5]);
        foreach (var d in definitions) if (!string.IsNullOrWhiteSpace(d[4])) producers.Add(d[4]);
        foreach (var c in code)
            if ((c[2] == "RecordClaim" || c[2] == "RecordClaimOnce" || c[2] == "AddEntry") && c[4] == "Literal") producers.Add(c[3]);
        // Note: many specialized scripts use variables/constants as first args; these are not resolved here.
        var hypothesisIDs = new HashSet<string>(definitions.Select(d => d[3]).Where(x => !string.IsNullOrWhiteSpace(x)), StringComparer.Ordinal);
        foreach (var d in definitions)
        {
            string location = d[0] + ":" + d[2];
            if (string.IsNullOrWhiteSpace(d[3]) || string.IsNullOrWhiteSpace(d[4])) Issue("INCOMPLETE_HYPOTHESIS", "High", location, d[3], "Hypothesis ID or journal entry ID is empty.");
            if (d[6] == d[7]) Issue("IDENTICAL_PREREQUISITES", "High", location, d[3], "Both selected information IDs are identical; the journal requires two distinct selections.");
            foreach (int i in new[] { 6, 7 })
            {
                string req = d[i];
                if (string.IsNullOrWhiteSpace(req)) Issue("EMPTY_HYPOTHESIS_PREREQUISITE", "High", location, d[3], "A hypothesis has an empty prerequisite.");
                else if (!producers.Contains(req)) Issue("UNRESOLVED_HYPOTHESIS_INPUT", "Review", location, req, "No literal/serialized producer found; may be generated using a constant or a variable.");
                if (req == d[4]) Issue("SELF_DEPENDENCY", "High", location, d[3], "Hypothesis requires its own journal entry ID.");
            }
        }
        foreach (var c in code)
        {
            if (c[4] != "Literal") continue;
            if (c[2] == "HasHypothesis" && !hypothesisIDs.Contains(c[3])) Issue("UNDEFINED_HYPOTHESIS_GATE", "High", c[0] + ":" + c[1], c[3], "No matching hypothesis definition found in scanned assets. Check the authoritative persistent JournalManager.");
            if (c[2] == "HasInformation" && !producers.Contains(c[3])) Issue("UNRESOLVED_INFORMATION_GATE", "Review", c[0] + ":" + c[1], c[3], "No literal/serialized producer found; inspect constants and dynamic entry creation.");
        }
        foreach (var r in readable)
        {
            string location = r[0] + ":" + r[2];
            if (string.IsNullOrWhiteSpace(r[5])) Issue("EMPTY_READABLE_EVIDENCE_ID", "High", location, "", "Readable object has no evidence ID.");
            if (!string.IsNullOrWhiteSpace(r[6]) && !producers.Contains(r[6])) Issue("UNRESOLVED_DOCUMENT_GATE", "Review", location, r[6], "Required information ID has no obvious producer.");
            if (!string.IsNullOrWhiteSpace(r[6]) && r[6] == r[5]) Issue("DOCUMENT_SELF_DEPENDENCY", "High", location, r[5], "Document requires its own evidence ID.");
        }
        foreach (var group in definitions.GroupBy(d => d[0]))
        {
            foreach (var dup in group.GroupBy(d => d[3]).Where(g => g.Count() > 1 && g.Key != ""))
                Issue("DUPLICATE_HYPOTHESIS_ID", "High", group.Key, dup.Key, "Same JournalManager asset contains repeated hypothesis IDs.");
            foreach (var dup in group.GroupBy(d => d[4]).Where(g => g.Count() > 1 && g.Key != ""))
                Issue("DUPLICATE_HYPOTHESIS_ENTRY_ID", "High", group.Key, dup.Key, "Same JournalManager asset contains repeated hypothesis journal IDs.");
        }
        foreach (var o in objects.Where(x => x[8] == "True"))
            if (!sceneNames.Contains(o[9])) Issue("MISSING_SCENE_DESTINATION", "High", o[0] + ":" + o[2], o[9], "Scene exit destination not found among scanned scenes.");
    }

    static bool FieldValue(string[] f, string property) => fields.Any(x => x[0] == f[0] && x[2] == f[2] && x[3] == f[3] && x[4] == property && x[5] == "True");
    static string Child(SerializedProperty p, string name) { var q = p.FindPropertyRelative(name); return q != null ? q.stringValue : ""; }
    static string Val(SerializedObject o, string name)
    {
        var p = o.FindProperty(name);
        if (p == null) return "";
        if (p.propertyType == SerializedPropertyType.String) return p.stringValue;
        if (p.propertyType == SerializedPropertyType.Boolean) return p.boolValue.ToString();
        if (p.propertyType == SerializedPropertyType.Enum) return p.enumValueIndex >= 0 && p.enumValueIndex < p.enumNames.Length ? p.enumNames[p.enumValueIndex] : "";
        return "";
    }
    static string PathOf(Transform t) { var parts = new List<string>(); for (; t != null; t = t.parent) parts.Add(t.name); parts.Reverse(); return string.Join("/", parts.ToArray()); }
    static void Issue(string type, string severity, string location, string id, string detail) => issues.Add(new[] { type, severity, location, id, detail });
    static void Write(string folder, string filename, string[] header, IEnumerable<string[]> rows)
    {
        using (var w = new StreamWriter(Path.Combine(folder, filename), false, new UTF8Encoding(true)))
        {
            w.WriteLine(string.Join(",", header.Select(Csv).ToArray()));
            foreach (var row in rows) w.WriteLine(string.Join(",", row.Select(Csv).ToArray()));
        }
    }
    static string Csv(string s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";
}
#endif
