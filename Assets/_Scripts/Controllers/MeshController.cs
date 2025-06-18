using System.Collections;
using com.trashpandaboy.events;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class MeshController : MonoBehaviour
{
    [Tooltip("Valore di estrusione lungo la normale dei vertici")]
    [Range(-1f, 1f)] public float extrusionAmount = 0f;
    [Range(0f, 1f)] public float displacementAmount = 0.1f;

    [Tooltip("Cutoff per il dot product: valori sotto questo non sono influenzati.")]
    [Range(-1f, 1f)] public float dotCutoff = 0.1f;

    [Tooltip("Distanza massima dal punto d’impatto entro cui l’effetto è applicato.")]
    public float impactFalloffRadius = 1.0f;



    [Header("Animazione")]
    public float meshAnimationDuration = 0.5f;
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


    private MeshFilter meshFilter;
    private Mesh originalMesh;
    private Vector3[] originalVertices;

    private float lastExtrusion;
    private Vector3 meshCenter;
    private MeshCollider _meshCollider;

    void OnEnable()
    {
        meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();

        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            originalMesh = meshFilter.sharedMesh;
            Mesh clonedMesh = Instantiate(originalMesh);
            meshFilter.sharedMesh = clonedMesh;

            originalVertices = clonedMesh.vertices;

            lastExtrusion = extrusionAmount;
            meshCenter = CalculateMeshCenter(originalVertices);
            ApplyExtrusion();
        }

        UpdateMeshCollider();

        EventDispatcher.StartListening(EventType.RaycastHit.ToString(), HandleOnCollision);
    }

    void OnDisable()
    {
        EventDispatcher.StopListening(EventType.RaycastHit.ToString(), HandleOnCollision);
    }

    void Update()
    {
        if (Mathf.Abs(lastExtrusion - extrusionAmount) > 0.0001f)
        {
            ApplyExtrusion();
            lastExtrusion = extrusionAmount;

            UpdateMeshCollider();
        }
    }

    private void HandleOnCollision(object data)
    {
        if (data is RaycastEventData collisionEventData)
        {
            TriggerImpactAnimated(collisionEventData.raycastHit.point);
        }
    }

    private void ApplyExtrusion()
    {
        if (meshFilter == null || meshFilter.sharedMesh == null) return;

        Vector3[] extrudedVertices = GetExtrudedVertices();

        Mesh mesh = meshFilter.sharedMesh;
        mesh.vertices = extrudedVertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private Vector3[] GetExtrudedVertices()
    {
        Vector3[] extruded = new Vector3[originalVertices.Length];
        for (int i = 0; i < extruded.Length; i++)
        {
            Vector3 dir = (originalVertices[i] - meshCenter).normalized;
            extruded[i] = originalVertices[i] + dir * extrusionAmount;
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

    public void TriggerImpactAnimated(Vector3 localImpactPoint)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateImpact(localImpactPoint));
        // EventDispatcher.TriggerEvent(EventType.Vfx.ToString(), new VfxEventData(1, localImpactPoint));
    }

    private IEnumerator AnimateImpact(Vector3 localImpactPoint)
    {
        if (meshFilter == null || meshFilter.sharedMesh == null) yield break;

        Vector3 impactDirection = (localImpactPoint - meshCenter).normalized;
        Vector3[] baseVertices = GetExtrudedVertices(); // usa mesh estrusa come base

        Vector3[] deformedVertices = new Vector3[baseVertices.Length];
        Vector3[] currentVertices = new Vector3[baseVertices.Length];

        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector3 fromCenter = (baseVertices[i] - meshCenter).normalized;
            float dot = Vector3.Dot(fromCenter, impactDirection);
            float dotDisplacement = impactFalloffRadius * dot;

            if (dot > dotCutoff)
                deformedVertices[i] = baseVertices[i] - fromCenter * displacementAmount * dotDisplacement * dot;
            else if (dot < -dotCutoff)
                deformedVertices[i] = baseVertices[i] + fromCenter * displacementAmount * (-dotDisplacement) * (-dot);
            else
                deformedVertices[i] = baseVertices[i];
        }

        Mesh mesh = meshFilter.sharedMesh;
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
        _meshCollider.sharedMesh = meshFilter.sharedMesh;
    }
}
