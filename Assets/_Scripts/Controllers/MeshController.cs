using System.Collections;
using com.trashpandaboy.events;
using UnityEngine;

/// <summary>
/// This class controls the mesh of an object.
/// Applies a displacement and extrusion effect to the mesh vertices based on the given parameters.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class MeshController : MonoBehaviour
{
    #region VARIABLES

    [Header("Vertex manipulation settings")]

    [Tooltip("Extrusion amount along the line center-vertex positions.")]
    [Range(-1f, 1f)] public float extrusionAmount = 0f;

    [Tooltip("Displacement amount from the collision point along mouse direction if mouse speed > 0. Vertex-center of the mesh is used if mouse speed == 0.")]
    [Range(0f, 1.5f)] public float displacementAmount = 0.1f;

    [Tooltip("Cutoff for the dot product")]
    [Range(-1f, 1f)] public float dotCutoff = 0.1f;

    [Tooltip("Falloff radius from impact point")]
    public float impactFalloffRadius = 1.0f;

    [Space]

    [Header("Animation settings")]
    public float meshAnimationDuration = 0.5f;
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


    private MeshFilter m_meshFilter;
    private Mesh m_originalMesh;
    private Vector3[] m_originalVertices;

    private float m_lastExtrusion;
    private Vector3 m_meshCenter;
    private MeshCollider _meshCollider;

    #endregion

    #region UNITY CALLBACKS

    void OnEnable()
    {
        m_meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();

        if (m_meshFilter != null && m_meshFilter.sharedMesh != null)
        {
            m_originalMesh = m_meshFilter.sharedMesh;
            Mesh clonedMesh = Instantiate(m_originalMesh);
            m_meshFilter.sharedMesh = clonedMesh;

            m_originalVertices = clonedMesh.vertices;

            m_lastExtrusion = extrusionAmount;
            m_meshCenter = CalculateMeshCenter(m_originalVertices);
            ApplyExtrusion();
        }

        UpdateMeshCollider();

        EventDispatcher.StartListening(EventType.RaycastHit, HandleOnCollision);
    }

    void OnDisable()
    {
        EventDispatcher.StopListening(EventType.RaycastHit, HandleOnCollision);
    }

    void Update()
    {
        if (Mathf.Abs(m_lastExtrusion - extrusionAmount) > 0.0001f)
        {
            ApplyExtrusion();
            m_lastExtrusion = extrusionAmount;

            UpdateMeshCollider();
        }
    }

    #endregion

    private void HandleOnCollision(object data)
    {
        if (data is RaycastEventData collisionEventData)
        {
            TriggerImpactAnimated(collisionEventData.raycastHit.point, collisionEventData.mouseSpeed, collisionEventData.mouseDirection);
        }
    }

    #region VERTEX MANIPULATION METHODS

    private void ApplyExtrusion()
    {
        if (m_meshFilter == null || m_meshFilter.sharedMesh == null) return;

        Vector3[] extrudedVertices = GetExtrudedVertices();

        Mesh mesh = m_meshFilter.sharedMesh;
        mesh.vertices = extrudedVertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private Vector3[] GetExtrudedVertices()
    {
        Vector3[] extruded = new Vector3[m_originalVertices.Length];
        for (int i = 0; i < extruded.Length; i++)
        {
            Vector3 dir = (m_originalVertices[i] - m_meshCenter).normalized;
            extruded[i] = m_originalVertices[i] + dir * extrusionAmount;
        }
        return extruded;
    }

    private Vector3 CalculateMeshCenter(Vector3[] verts)
    {
        Vector3 sum = Vector3.zero;
        foreach (var v in verts)
            sum += v;
        return sum / verts.Length;
    }

    private void TriggerImpactAnimated(Vector3 localImpactPoint, float mouseSpeed, Vector3 mouseDirection)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateImpact(localImpactPoint, mouseSpeed, mouseDirection));
    }

    private IEnumerator AnimateImpact(Vector3 localImpactPoint, float mouseSpeed, Vector3 mouseDirection)
    {
        if (m_meshFilter == null || m_meshFilter.sharedMesh == null) yield break;

        SetDisplacementeAmountClamped(mouseSpeed);

        Vector3 impactDirection = (localImpactPoint - m_meshCenter).normalized;
        if (mouseSpeed > 0)
        {
            Vector3 mouseDirectionFlippedX = new Vector3(-mouseDirection.x, mouseDirection.y, mouseDirection.z);
            Vector3 impactProjectionPoint = localImpactPoint + mouseDirectionFlippedX;
            impactDirection = (localImpactPoint - impactProjectionPoint).normalized;
        }

        Vector3[] baseVertices = GetExtrudedVertices(); // use extruded mesh as a base for displacement

        Vector3[] deformedVertices = new Vector3[baseVertices.Length];
        Vector3[] currentVertices = new Vector3[baseVertices.Length];

        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector3 fromCenter = (baseVertices[i] - m_meshCenter).normalized;
            float dot = Vector3.Dot(fromCenter, impactDirection);
            float dotDisplacement = impactFalloffRadius * dot;

            if (dot > dotCutoff)
                deformedVertices[i] = baseVertices[i] - fromCenter * displacementAmount * dotDisplacement * dot;
            else if (dot < -dotCutoff)
                deformedVertices[i] = baseVertices[i] + fromCenter * displacementAmount * (-dotDisplacement) * (-dot);
            else
                deformedVertices[i] = baseVertices[i];
        }

        Mesh mesh = m_meshFilter.sharedMesh;
        float elapsed = 0f;
        while (elapsed < meshAnimationDuration)
        {
            float t = elapsed / meshAnimationDuration;
            float eased = easingCurve.Evaluate(t <= 0.5f ? t * 2f : (1f - t) * 2f);

            for (int i = 0; i < currentVertices.Length; i++)
            {
                currentVertices[i] = Vector3.Lerp(baseVertices[i], deformedVertices[i], eased);
            }

            mesh.vertices = currentVertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Restore to base extrusion
        mesh.vertices = baseVertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private void UpdateMeshCollider()
    {
        if (_meshCollider == null) return;

        _meshCollider.sharedMesh = null;
        _meshCollider.sharedMesh = m_meshFilter.sharedMesh;
    }

    private void SetDisplacementeAmountClamped(float mouseSpeed)
    {
        // Debug.Log($"[MeshController] MouseSpeed {mouseSpeed}");
        float mouseSpeedClamped = Mathf.Clamp(mouseSpeed, 20f, 150f);

        displacementAmount = Mathf.Clamp(mouseSpeedClamped / 100f, 0f, 1.5f);
    }

    #endregion
}
