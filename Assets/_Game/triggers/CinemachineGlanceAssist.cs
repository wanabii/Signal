using UnityEngine;

public class CinemachineGlanceAssist : MonoBehaviour
{
    [Header("Look Transforms")]
    [SerializeField] private Transform playerYawRoot;
    [SerializeField] private Transform cameraPitchPivot;
    [SerializeField] private Transform cameraSource;

    [Header("Assist Settings")]
    [SerializeField] private float yawSpeed = 220f;
    [SerializeField] private float pitchSpeed = 180f;
    [SerializeField] private float defaultDuration = 0.8f;
    [SerializeField] private float maxPitch = 80f;

    [Header("Safety")]
    [SerializeField] private float maxYawAssistAngle = 45f;
    [SerializeField] private float maxPitchAssistAngle = 25f;

    private Transform target;
    private float timeLeft;

    public void GlanceAt(Transform newTarget, float duration = -1f)
    {
        if (newTarget == null)
            return;

        target = newTarget;
        timeLeft = duration > 0f ? duration : defaultDuration;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        if (timeLeft <= 0f)
        {
            target = null;
            return;
        }

        timeLeft -= Time.deltaTime;

        if (playerYawRoot == null || cameraPitchPivot == null)
            return;

        Vector3 from = cameraSource != null ? cameraSource.position : cameraPitchPivot.position;
        Vector3 worldDir = target.position - from;

        if (worldDir.sqrMagnitude < 0.0001f)
            return;

        Vector3 localDir = playerYawRoot.InverseTransformDirection(worldDir.normalized);

        float yawDelta = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
        yawDelta = Mathf.Clamp(yawDelta, -maxYawAssistAngle, maxYawAssistAngle);

        float yawStep = Mathf.MoveTowards(0f, yawDelta, yawSpeed * Time.deltaTime);
        playerYawRoot.Rotate(0f, yawStep, 0f, Space.World);

        Vector3 localDirAfterYaw = playerYawRoot.InverseTransformDirection((target.position - from).normalized);
        float flatMagnitude = new Vector2(localDirAfterYaw.x, localDirAfterYaw.z).magnitude;

        float desiredPitch = -Mathf.Atan2(localDirAfterYaw.y, flatMagnitude) * Mathf.Rad2Deg;
        desiredPitch = Mathf.Clamp(desiredPitch, -maxPitchAssistAngle, maxPitchAssistAngle);

        Vector3 localEuler = cameraPitchPivot.localEulerAngles;
        float currentPitch = NormalizeAngle(localEuler.x);

        float nextPitch = Mathf.MoveTowards(currentPitch, desiredPitch, pitchSpeed * Time.deltaTime);
        nextPitch = Mathf.Clamp(nextPitch, -maxPitch, maxPitch);

        localEuler.x = nextPitch;
        localEuler.y = 0f;
        localEuler.z = 0f;
        cameraPitchPivot.localEulerAngles = localEuler;
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f)
            angle -= 360f;

        while (angle < -180f)
            angle += 360f;

        return angle;
    }
}
