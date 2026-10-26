
using System;
using UnityEngine;

public class CaseResolutionManager : MonoBehaviour
{
    public static CaseResolutionManager Instance { get; private set; }

    public event Action OnCaseResolved;

    private const string DwyerConfessionID =
        "dwyer_concealment_confession";

    private const string BellAdmissionID =
        "frank_concealment_admission";

    private bool resolutionAnnounced;

    public bool IsCaseResolved
    {
        get
        {
            if (JournalManager.Instance == null)
                return false;

            return
                JournalManager.Instance.HasInformation(DwyerConfessionID) &&
                JournalManager.Instance.HasInformation(BellAdmissionID);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (resolutionAnnounced || !IsCaseResolved)
            return;

        resolutionAnnounced = true;
        OnCaseResolved?.Invoke();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
