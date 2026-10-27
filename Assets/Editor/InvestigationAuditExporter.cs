#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class InvestigationAuditExporter
{
    private class SourceRow
    {
        public string asset, path, kind, id, title, source, description, type, profile, notes;
    }

    private static readonly List<SourceRow> sources = new List<SourceRow>();
    private static readonly List<string[]> hypotheses = new List<string[]>();
    private static readonly List<string[]> warnings = new List<string[]>();

    [MenuItem("Tools/Seven Years Quiet/Export Investigation Audit")]
    public static void Export()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var setup = EditorSceneManager.GetSceneManagerSetup();
        sources.Clear(); hypotheses.Clear(); warnings.Clear();
        int sceneCount = 0, prefabCount = 0;
        try
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                try
                {
                    Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    foreach (GameObject root in scene.GetRootGameObjects()) ScanTree(root, path, "Scene");
                    sceneCount++;
                }
                catch (Exception ex) { warnings.Add(new[] { "SCAN_ERROR", path, "", ex.Message }); }
            }
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = null;
                try
                {
                    root = PrefabUtility.LoadPrefabContents(path);
                    ScanTree(root, path, "Prefab");
                    prefabCount++;
                }
                catch (Exception ex) { warnings.Add(new[] { "SCAN_ERROR", path, "", ex.Message }); }
                finally { if (root != null) PrefabUtility.UnloadPrefabContents(root); }
            }
        }
        finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }

        foreach (var group in sources.Where(s => !string.IsNullOrWhiteSpace(s.id)).GroupBy(s => s.id))
        {
            // Prefab/scene occurrences can legitimately share an ID; review them rather than auto-changing.
            if (group.Count() > 1)
                warnings.Add(new[] { "DUPLICATE_ID", group.Key, group.Count().ToString(),
                    string.Join(" | ", group.Select(s => s.asset + ":" + s.path).ToArray()) });
            if (group.Select(s => s.description).Distinct().Count() > 1)
                warnings.Add(new[] { "CONFLICTING_DESCRIPTION", group.Key, "", "Same ID has different descriptions" });
        }
        foreach (var h in hypotheses)
        {
            if (!sources.Any(s => s.id == h[4]))
                warnings.Add(new[] { "UNFOUND_PREREQUISITE", h[1], h[4], "First required ID not found among generic information objects (may come from NPC dialogue or another script)" });
            if (!sources.Any(s => s.id == h[5]))
                warnings.Add(new[] { "UNFOUND_PREREQUISITE", h[1], h[5], "Second required ID not found among generic information objects (may come from NPC dialogue or another script)" });
        }

        string folder = EditorUtility.SaveFolderPanel("Choose audit output folder", Application.dataPath, "");
        if (string.IsNullOrEmpty(folder)) return;
        WriteCsv(Path.Combine(folder, "InformationSources.csv"),
            new[] { "Asset", "HierarchyPath", "Kind", "InformationID", "Title", "Source", "Description", "Type", "NPCProfile", "Notes" },
            sources.Select(s => new[] { s.asset, s.path, s.kind, s.id, s.title, s.source, s.description, s.type, s.profile, s.notes }));
        WriteCsv(Path.Combine(folder, "Hypotheses.csv"),
            new[] { "Asset", "HypothesisID", "JournalEntryID", "Title", "RequiredID1", "RequiredID2", "Description" }, hypotheses);
        WriteCsv(Path.Combine(folder, "AuditWarnings.csv"),
            new[] { "Warning", "ID_or_Asset", "Count_or_RequiredID", "Details" }, warnings);
        Debug.Log($"Investigation audit complete: {sceneCount} scenes, {prefabCount} prefabs, {sources.Count} information sources, {hypotheses.Count} hypothesis definitions, {warnings.Count} warnings. Output: {folder}");
        EditorUtility.RevealInFinder(Path.Combine(folder, "InformationSources.csv"));
    }

    private static void ScanTree(GameObject root, string asset, string kind)
    {
        foreach (var component in root.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (component == null) continue;
            var so = new SerializedObject(component);
            string path = GetPath(component.transform);
            if (component is Interactable)
            {
                bool isInfo = GetBool(so, "isInformationSource");
                if (isInfo)
                {
                    sources.Add(new SourceRow {
                        asset = asset, path = path, kind = kind,
                        id = GetString(so, "informationID"),
                        title = GetString(so, "informationTitle"),
                        source = GetString(so, "informationSource"),
                        description = GetString(so, "informationDescription"),
                        type = GetEnum(so, "informationType"),
                        profile = GetEnum(so, "npcProfile"),
                        notes = GetBool(so, "isSceneExit") ? "Also marked scene exit; scene exit takes precedence" : ""
                    });
                }
            }
            if (component is JournalManager)
            {
                var list = so.FindProperty("hypothesisDefinitions");
                if (list == null || !list.isArray) continue;
                for (int i = 0; i < list.arraySize; i++)
                {
                    var item = list.GetArrayElementAtIndex(i);
                    hypotheses.Add(new[] {
                        asset + ":" + path,
                        GetChild(item, "hypothesisID"), GetChild(item, "informationEntryID"),
                        GetChild(item, "title"), GetChild(item, "requiredInformationID1"),
                        GetChild(item, "requiredInformationID2"), GetChild(item, "description")
                    });
                }
            }
        }
    }

    private static string GetPath(Transform t)
    {
        var parts = new List<string>();
        while (t != null) { parts.Add(t.name); t = t.parent; }
        parts.Reverse(); return string.Join("/", parts.ToArray());
    }
    private static string GetString(SerializedObject so, string name) => so.FindProperty(name)?.stringValue ?? "";
    private static bool GetBool(SerializedObject so, string name) => so.FindProperty(name)?.boolValue ?? false;
    private static string GetEnum(SerializedObject so, string name)
    {
        var p = so.FindProperty(name);
        return p != null && p.propertyType == SerializedPropertyType.Enum && p.enumValueIndex >= 0 && p.enumValueIndex < p.enumDisplayNames.Length
            ? p.enumDisplayNames[p.enumValueIndex] : "";
    }
    private static string GetChild(SerializedProperty p, string name) => p.FindPropertyRelative(name)?.stringValue ?? "";
    private static void WriteCsv(string path, string[] header, IEnumerable<string[]> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", header.Select(Escape).ToArray()));
        foreach (var row in rows) sb.AppendLine(string.Join(",", row.Select(Escape).ToArray()));
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
    }
    private static string Escape(string value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
}
#endif
