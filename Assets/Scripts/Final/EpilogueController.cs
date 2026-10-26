
using UnityEngine;

public class EpilogueController : MonoBehaviour
{
    private void OnEnable()
    {
        FinalReportUI.OnReportPublished += StartEpilogue;
    }

    private void OnDisable()
    {
        FinalReportUI.OnReportPublished -= StartEpilogue;
    }

    private void StartEpilogue()
    {
        Debug.Log("Epilogue triggered successfully.");

        // Next: display the first epilogue panel.
    }
}
