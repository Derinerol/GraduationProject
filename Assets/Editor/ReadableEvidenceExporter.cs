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

public static class ReadableEvidenceExporter
{
    private static readonly string[] DocumentHeaders = {
        "Asset", "HierarchyPath", "Kind", "Department", "Document Title", "Reference",
        "Document Date", "Document Body", "Evidence ID", "Evidence Title", "Journal Note",
        "Evidence Source", "Required Information ID", "Unavailable Message"
    };
    private static readonly string[] NewspaperHeaders = {
        "Asset", "HierarchyPath", "Kind", "Publication", "Publication Date", "Headline",
        "Article Body", "Evidence ID", "Evidence Title", "Journal Note"
    };

    [MenuItem("Tools/Seven Years Quiet/Export Readable Evidence")]
    public static void Export()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        string folder = EditorUtility.SaveFolderPanel("Choose readable evidence export folder", Application.dataPath, "");
        if (string.IsNullOrEmpty(folder)) return;

        var sceneSetup = EditorSceneManager.GetSceneManagerSetup();
        var documents = new List<string[]>();
        var newspapers = new List<string[]>();
        var errors = new List<string[]>();
        int sceneCount = 0, prefabCount = 0;

        try
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
            {
                string asset = AssetDatabase.GUIDToAssetPath(guid);
                try
                {
                    Scene scene = EditorSceneManager.OpenScene(asset, OpenSceneMode.Single);
                    foreach (GameObject root in scene.GetRootGameObjects())
                        Scan(root, asset, "Scene", documents, newspapers);
                    sceneCount++;
                }
                catch (Exception ex)
                {
                    errors.Add(new[] { "Scene", asset, ex.ToString() });
                    Debug.LogWarning("Readable evidence export failed for " + asset + ": " + ex.Message);
                }
            }

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
            {
                string asset = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = null;
                try
                {
                    root = PrefabUtility.LoadPrefabContents(asset);
                    Scan(root, asset, "Prefab", documents, newspapers);
                    prefabCount++;
                }
                catch (Exception ex)
                {
                    errors.Add(new[] { "Prefab", asset, ex.ToString() });
                    Debug.LogWarning("Readable evidence export failed for " + asset + ": " + ex.Message);
                }
                finally
                {
                    if (root != null) PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }
        finally
        {
            EditorSceneManager.RestoreSceneManagerSetup(sceneSetup);
        }

        WriteCsv(Path.Combine(folder, "MunicipalDocuments.csv"), DocumentHeaders, documents);
        WriteCsv(Path.Combine(folder, "Newspapers.csv"), NewspaperHeaders, newspapers);
        WriteCsv(Path.Combine(folder, "ReadableEvidenceExportErrors.csv"),
            new[] { "Kind", "Asset", "Error" }, errors);
        Debug.Log($"Readable evidence export complete. Scanned {sceneCount} scenes and {prefabCount} prefabs; " +
                  $"exported {documents.Count} municipal documents and {newspapers.Count} newspapers. " +
                  $"Errors: {errors.Count}. Folder: {folder}");
        EditorUtility.RevealInFinder(folder);
    }

    private static void Scan(GameObject root, string asset, string kind,
        List<string[]> documents, List<string[]> newspapers)
    {
        foreach (MunicipalRecord record in root.GetComponentsInChildren<MunicipalRecord>(true))
        {
            var so = new SerializedObject(record);
            documents.Add(new[] {
                asset, HierarchyPath(record.transform), kind,
                Value(so, "department"), Value(so, "documentTitle"), Value(so, "reference"),
                Value(so, "documentDate"), Value(so, "documentBody"), Value(so, "evidenceID"),
                Value(so, "evidenceTitle"), Value(so, "journalNote"), Value(so, "evidenceSource"),
                Value(so, "requiredInformationID"), Value(so, "unavailableMessage")
            });
        }
        foreach (NewspaperEvidence paper in root.GetComponentsInChildren<NewspaperEvidence>(true))
        {
            var so = new SerializedObject(paper);
            newspapers.Add(new[] {
                asset, HierarchyPath(paper.transform), kind,
                Value(so, "publication"), Value(so, "publicationDate"), Value(so, "headline"),
                Value(so, "articleBody"), Value(so, "evidenceID"), Value(so, "evidenceTitle"),
                Value(so, "journalNote")
            });
        }
    }

    private static string Value(SerializedObject obj, string name)
    {
        SerializedProperty property = obj.FindProperty(name);
        return property != null && property.propertyType == SerializedPropertyType.String
            ? property.stringValue : "";
    }

    private static string HierarchyPath(Transform transform)
    {
        var parts = new List<string>();
        for (Transform t = transform; t != null; t = t.parent) parts.Add(t.name);
        parts.Reverse();
        return string.Join("/", parts);
    }

    private static void WriteCsv(string path, string[] headers, IEnumerable<string[]> rows)
    {
        using (var writer = new StreamWriter(path, false, new UTF8Encoding(true)))
        {
            writer.WriteLine(string.Join(",", headers.Select(Escape)));
            foreach (string[] row in rows)
                writer.WriteLine(string.Join(",", row.Select(Escape)));
        }
    }

    private static string Escape(string value)
    {
        value = value ?? "";
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
#endif
