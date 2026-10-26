
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
public class InvestigationDebug : MonoBehaviour
{
    private bool visible;
    private string informationID = "";
    private string hypothesisID = "";
    private string status = "Ready";
    private Rect window = new Rect(20, 20, 430, 420);

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.f8Key.wasPressedThisFrame)
        {
            visible = !visible;
        }
    }

    private void OnGUI()
    {
        if (!visible) return;

        window = GUI.Window(
            84721, window, DrawWindow,
            "Investigation Debug (F8)");
    }

    private void DrawWindow(int id)
    {
        JournalManager journal = JournalManager.Instance;

        GUILayout.Label(journal == null
            ? "JournalManager not found"
            : "JournalManager ready");

        GUILayout.Space(8);
        GUILayout.Label("Information ID:");
        informationID = GUILayout.TextField(informationID);

        if (GUILayout.Button("Grant information") &&
            journal != null)
        {
            status = journal.DebugGrantInformation(informationID)
                ? "Information granted"
                : "Invalid information ID";
        }

        GUILayout.Space(10);
        GUILayout.Label("Hypothesis ID:");
        hypothesisID = GUILayout.TextField(hypothesisID);

        if (GUILayout.Button("Grant hypothesis + prerequisites") &&
            journal != null)
        {
            status = journal.DebugGrantHypothesis(hypothesisID)
                ? "Hypothesis granted"
                : "Hypothesis not found";
        }

        GUILayout.Space(10);
        GUILayout.Label("Quick presets:");

        if (GUILayout.Button("Barrier investigation"))
        {
            GrantPresets(journal, new string[]
            {
                "unverified_lower_access_maintenance",
                "unexplained_service_barrier",
                "changed_service_access",
                "unverified_access_alteration"
            });
        }

        if (GUILayout.Button("Frank follow-up"))
        {
            GrantPresets(journal, new string[]
            {
                "unverified_access_alteration",
                "unresolved_barrier_responsibility"
            });
        }

        if (GUILayout.Button("Supplementary access log"))
        {
            GrantPresets(journal, new string[]
            {
                "unresolved_barrier_responsibility"
            });
        }

        GUILayout.Space(8);
        GUILayout.Label(status);

        GUI.DragWindow(new Rect(0, 0, 10000, 22));
    }

    private void GrantPresets(
        JournalManager journal, string[] ids)
    {
        if (journal == null)
        {
            status = "JournalManager not found";
            return;
        }

        int count = 0;

        foreach (string id in ids)
        {
            if (journal.DebugGrantHypothesis(id))
                count++;
        }

        status = "Granted " + count + "/" +
                 ids.Length + " hypotheses";
    }
}
#else
public class InvestigationDebug : MonoBehaviour { }
#endif
