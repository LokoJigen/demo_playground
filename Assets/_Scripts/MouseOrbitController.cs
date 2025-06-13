using UnityEngine;

public class MouseOrbitController : MonoBehaviour
{
    [Tooltip("Il target attorno a cui orbitare.")]
    public Transform lookTarget;

    [Header("Controlli Orbita")]
    public float orbitSpeed = 3.0f;
    public float distance = 5.0f;
    public float minPitch = -85f;
    public float maxPitch = 85f;

    private float yaw;
    private float pitch;
    private bool initialized = false;

    void OnEnable()
    {
        if (lookTarget != null && !initialized)
        {
            transform.LookAt(lookTarget.position);

            Vector3 offset = transform.position - lookTarget.position;
            distance = offset.magnitude;

            // Calcolo più robusto degli angoli iniziali
            Vector3 angles = GetLookAtAngles(lookTarget.position);

            yaw = angles.y;
            pitch = angles.x;

            initialized = true;

            Debug.Log($"[MouseOrbitController] Target initialized at position ({lookTarget.position.x}, {lookTarget.position.y}, {lookTarget.position.z})");
        }
    }

    private Vector3 GetLookAtAngles(Vector3 position)
    {
        Quaternion lookRotation = Quaternion.LookRotation(position - transform.position);
        Vector3 angles = lookRotation.eulerAngles;
        return angles;
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

            Vector2 currentMousePosition = wasLocked ? GetCurrentMousePosition() : Vector2.zero;

            float mouseX = currentMousePosition.x * orbitSpeed; ;
            float mouseY = currentMousePosition.y * orbitSpeed; ;

            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            UpdateCameraPosition();

            Debug.Log($"[MouseOrbitController] wasLocked: {wasLocked}, Mouse X: {mouseX}, Mouse Y: {mouseY}, yaw: {yaw}, pitch: {pitch}");
        }

        if (Input.GetMouseButtonUp(0))
        {
            SetCursorLockState(false);
        }
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

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);

        transform.position = lookTarget.position + offset;
        transform.LookAt(lookTarget.position);
    }
}
