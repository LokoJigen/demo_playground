using UnityEngine;

public enum TargetAxis
{
    None,
    X,
    Y,
    Z,
}

public class MouseOrbitController : MonoBehaviour
{
    #region VARIABLES

    [Header("Orbit settings")]

    [Tooltip("Target Transform to look at. Set to null to disable orbiting.")]
    public Transform lookTarget;
    public float orbitSpeed = 3.0f;
    public float distance = 5.0f;

    [Space]


    [Header("Starting Settings")]

    [Tooltip("Face target OnEnable")]
    public bool isFaceTarget = true;

    [Tooltip("Target axis to face OnEnable")]
    public TargetAxis targetAxisToFace = TargetAxis.Z;

    [Tooltip("Is negative Axis")]
    public bool isNegativeAxisAtStart = false;

    [Tooltip("Is invert mouse scroll Axis")]
    public bool isInvertScrollAxis = false;

    [Space]


    [Header("Orbit limits")]

    [Tooltip("MinMax distance limits for the distance between the camera and the target")]
    public Vector2 minMaxDistance = new Vector2(1.0f, 10.0f);
    [Tooltip("Max angle offset from the target axis OnEnable (degrees)")]
    public float maxOrbitAngle = 90f;

    private float minPitch = -85f;
    private float maxPitch = 85f;
    private float yaw;
    private float pitch;
    private float initialYaw;
    private float initialPitch;
    private float maxYawOffset;
    private float maxPitchOffset;

    #endregion

    #region UNITY METHODS
    void OnEnable()
    {
        if (lookTarget != null)
        {
            if (isFaceTarget)
            {
                Vector3 targetPosition = lookTarget.position + GetTargetAxisVector(targetAxisToFace) * distance;
                transform.position = targetPosition;
            }

            UpdateRotationCached();

            // Debug.Log($"[MouseOrbitController] Target at position ({lookTarget.position.x}, {lookTarget.position.y}, {lookTarget.position.z})");
        }
    }


    void Update()
    {
        if (lookTarget == null) return;

        if (Input.GetMouseButton(0))
        {
            bool wasLocked = true;


            if (Cursor.lockState != CursorLockMode.Locked)
            {
                wasLocked = false;
                SetCursorLockState();
            }

            // Ignore mouse position when not locked to avoid input lag
            Vector2 currentMousePosition = wasLocked ? GetCurrentMousePosition() : Vector2.zero;

            AddToRotationCache(currentMousePosition);

            UpdateCameraOrbitPosition();

            // Debug.Log($"[MouseOrbitController] wasLocked: {wasLocked}, Mouse X: {currentMousePosition.x}, Mouse Y: {currentMousePosition.y}, yaw: {yaw}, pitch: {pitch}");
        }

        if (Input.GetMouseButtonUp(0))
        {
            SetCursorLockState(false);
        }


        UpdateDistanceClamped(distance, Input.mouseScrollDelta.y);
    }

    #endregion

    #region PRIVATE METHODS

    private Vector3 GetLookAtAngles(Vector3 position)
    {
        Quaternion lookRotation = Quaternion.LookRotation(position - transform.position);
        Vector3 angles = lookRotation.eulerAngles;
        return angles;
    }

    private void SetCursorLockState(bool isLocked = true)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
    }

    private Vector2 GetCurrentMousePosition()
    {
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    void UpdateCameraOrbitPosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);

        transform.position = lookTarget.position + offset;
        transform.LookAt(lookTarget.position);

        // Debug.Log($"[MouseOrbitController] Updated camera position to {transform.position}");
    }

    private void UpdateDistanceClamped(float currentDistance, float delta)
    {
        if (delta == 0) return; // GUARD CASE

        float sign = isInvertScrollAxis ? -1 : 1;

        distance = Mathf.Clamp(currentDistance + delta * sign, minMaxDistance.x, minMaxDistance.y);

        // Debug.Log($"[MouseOrbitController] Updated distance: delta {delta}, final distance: {distance}");

        UpdateCameraOrbitPosition();
    }

    private void UpdateRotationCached()
    {
        transform.LookAt(lookTarget.position);

        Vector3 angles = GetLookAtAngles(lookTarget.position);
        yaw = angles.y;
        pitch = angles.x;

        initialYaw = yaw;
        initialPitch = pitch;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Set Yaw & Pitch limits
        maxYawOffset = maxOrbitAngle;
        maxPitchOffset = maxOrbitAngle;

        // Debug.Log($"[MouseOrbitController] UpdateRotationCached - Orbit limits: Yaw {initialYaw - maxYawOffset} to {initialYaw + maxYawOffset}, Pitch {initialPitch - maxPitchOffset} to {initialPitch + maxPitchOffset}");

    }


    private void AddToRotationCache(Vector2 toAdd)
    {
        float newYaw = yaw + toAdd.x * orbitSpeed;
        float newPitch = pitch - toAdd.y * orbitSpeed;

        // Clamp rispetto all'angolo iniziale
        newYaw = Mathf.Clamp(newYaw, initialYaw - maxYawOffset, initialYaw + maxYawOffset);
        newPitch = Mathf.Clamp(newPitch, initialPitch - maxPitchOffset, initialPitch + maxPitchOffset);

        // Clamp anche contro i limiti assoluti
        yaw = Mathf.Clamp(newYaw, -360f, 360f);
        pitch = Mathf.Clamp(newPitch, minPitch, maxPitch);
    }


    private Vector3 GetTargetAxisVector(TargetAxis targetAxis)
    {
        Vector3 targetVector;
        float sign = isNegativeAxisAtStart ? -1 : 1;

        switch (targetAxis)
        {
            case TargetAxis.X:
                targetVector = Vector3.right * sign;
                break;
            case TargetAxis.Y:
                targetVector = Vector3.up * sign;
                break;
            case TargetAxis.Z:
                targetVector = Vector3.forward * sign;
                break;
            default:
                targetVector = Vector3.one * sign;
                break;
        }

        return targetVector;
    }

    #endregion

}
