using System.Text;
using com.trashpandaboy.events;
using UnityEngine;

/// <summary>
/// This class contains data related to raycast events.
/// </summary>
public class RaycastEventData
{
    public RaycastHit raycastHit;
    public float mouseSpeed;
    public Vector2 mouseDirection;


    public RaycastEventData(RaycastHit raycastHit, float mouseSpeed, Vector2 mouseDirection)
    {
        this.raycastHit = raycastHit;
        this.mouseSpeed = mouseSpeed;
        this.mouseDirection = mouseDirection;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"RaycastHit: {raycastHit.collider.name} at {raycastHit.point}\n");
        sb.AppendLine($"Mouse speed: {mouseSpeed}\n");
        sb.AppendLine($"Mouse direction: {mouseDirection}\n");

        return sb.ToString();
    }
}

/// <summary>
/// This class handles raycast events and triggers appropriate events based on raycast results.
/// </summary>
public class RaycastManager : MonoBehaviour
{

    #region VARIABLES

    public LayerMask hitLayers; // Filter
    public float sensitivity = 0.1f; // Soglia di movimento minimo
    public float directionChangeThreshold = 5f; // Gradi di differenza per considerare un cambio

    private Vector2 m_lastDirection = Vector2.zero;
    private float m_currentMouseSpeed = 0f;
    private bool m_isHit = false;

    #endregion

    #region UNITY CALLBACKS

    void Update()
    {
        CacheMouseInfo();

        if (Input.GetMouseButton(1)) // Right mouse button
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000f, hitLayers) && !m_isHit)
            {
                // Debug.Log($"Hit: {hitInfo.collider.name} a {hitInfo.point}");

                m_isHit = true;
                EventDispatcher.TriggerEvent(EventType.RaycastHit, new RaycastEventData(hitInfo, m_currentMouseSpeed, m_lastDirection));
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            // Reset hit status for next raycast
            m_isHit = false;
        }
    }

    #endregion

    #region HELPER METHODS

    private bool CacheMouseInfo()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        m_currentMouseSpeed = mouseDelta.magnitude / Time.deltaTime;

        if (mouseDelta.magnitude < sensitivity)
            return false;

        Vector2 currentDirection = mouseDelta.normalized;

        m_lastDirection = currentDirection;
        return true;
    }

    #endregion

}
