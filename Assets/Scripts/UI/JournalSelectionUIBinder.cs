using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JournalSelectionUIBinder : MonoBehaviour
{
    [SerializeField] private TMP_Text firstSelectionText;
    [SerializeField] private TMP_Text secondSelectionText;
    [SerializeField] private Button clearSelectionButton;

    private IEnumerator Start()
    {
        yield return null;
        if (JournalManager.Instance != null)
            JournalManager.Instance.BindConnectionUI(
                firstSelectionText, secondSelectionText, clearSelectionButton);
        else
            Debug.LogWarning("JournalSelectionUIBinder: No JournalManager found.", this);
    }
}
