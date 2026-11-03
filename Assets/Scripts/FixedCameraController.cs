
using UnityEngine;

public class FixedCameraController : MonoBehaviour
{
    [System.Serializable]
    public class CameraView
    {
        public Transform cameraPoint;
        public float orthographicSize = 6f;
    }

    [Header("References")]
    [SerializeField] private Camera gameplayCamera;

    [Header("Views")]
    [SerializeField] private CameraView[] views;

    [Header("Transition")]
    [SerializeField] private float transitionSpeed = 4f;

    private int activeView = 0;

    private void Start()
    {
        if (gameplayCamera == null)
            gameplayCamera = Camera.main;

        if (gameplayCamera == null || views.Length == 0)
            return;

        SetViewImmediate(0);
    }

    private void LateUpdate()
    {
        if (gameplayCamera == null || views.Length == 0)
            return;

        CameraView target = views[activeView];

        if (target.cameraPoint == null)
            return;

        float t = 1f - Mathf.Exp(
            -transitionSpeed * Time.deltaTime
        );

        Transform cam = gameplayCamera.transform;

        cam.position = Vector3.Lerp(
            cam.position,
            target.cameraPoint.position,
            t
        );

        cam.rotation = Quaternion.Slerp(
            cam.rotation,
            target.cameraPoint.rotation,
            t
        );

        gameplayCamera.orthographicSize = Mathf.Lerp(
            gameplayCamera.orthographicSize,
            target.orthographicSize,
            t
        );
    }

    public void SetView(int index)
    {
        if (index < 0 || index >= views.Length)
            return;

        if (views[index].cameraPoint == null)
            return;

        activeView = index;
    }

    public void SetViewImmediate(int index)
    {
        if (index < 0 || index >= views.Length)
            return;

        if (views[index].cameraPoint == null)
            return;

        activeView = index;

        CameraView target = views[index];

        gameplayCamera.transform.SetPositionAndRotation(
            target.cameraPoint.position,
            target.cameraPoint.rotation
        );

        gameplayCamera.orthographicSize =
            target.orthographicSize;
    }
}
