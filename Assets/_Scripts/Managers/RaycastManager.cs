using System.Text;
using com.trashpandaboy.events;
using UnityEngine;

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


public class RaycastManager : MonoBehaviour
{
    public LayerMask hitLayers; // Filter
    public float sensitivity = 0.1f; // Soglia di movimento minimo
    public float directionChangeThreshold = 5f; // Gradi di differenza per considerare un cambio

    private Vector2 m_lastDirection = Vector2.zero;
    private float m_currentMouseSpeed = 0f;
    private bool m_isHit = false;

    void Update()
    {
        CacheMouseInfo();

        if (Input.GetMouseButton(1)) // Click sinistro
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000f, hitLayers) && !m_isHit)
            {
                Debug.Log($"Hit: {hitInfo.collider.name} a {hitInfo.point}");
                
                m_isHit = true;
                // Trigger on collision event
                EventDispatcher.TriggerEvent(EventType.RaycastHit.ToString(), new RaycastEventData(hitInfo, m_currentMouseSpeed, m_lastDirection));
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            m_isHit = false;
        }
    }

    private bool CacheMouseInfo()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        m_currentMouseSpeed = mouseDelta.magnitude / Time.deltaTime;

        if (mouseDelta.magnitude < sensitivity)
            return false;

        Vector2 currentDirection = mouseDelta.normalized;

        if (m_lastDirection != Vector2.zero)
        {
            float angle = Vector2.Angle(m_lastDirection, currentDirection);
            if (angle > directionChangeThreshold)
            {
                // Debug.Log($"[MouseDirectionDebugger] new direction: {currentDirection}, angle: {angle}°");
            }
        }

        m_lastDirection = currentDirection;
        return true;
    }

}
