using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Add to the always-active JournalUIRoot prefab.
// Keeps the existing JournalUIBinder unchanged.
public class JournalSelectionUIBinder : MonoBehaviour
{
    [SerializeField] private TMP_Text firstSelectionText;
    [SerializeField] private TMP_Text secondSelectionText;
    [SerializeField] private Button clearSelectionButton;

    private IEnumerator Start()
    {
        // Allow the existing JournalUIBinder to bind the scene UI first.
        yield return null;
        if (JournalManager.Instance != null)
            JournalManager.Instance.BindConnectionUI(
                firstSelectionText, secondSelectionText, clearSelectionButton);
        else
            Debug.LogWarning("JournalSelectionUIBinder: No JournalManager found.", this);
    }
}
