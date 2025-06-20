using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class binds BlendShapeSO values to an ExpressionType.
/// </summary>
[Serializable]
public class BlendShapeData
{
    [SerializeField] private BlendShapeSO blendShapeSO;
    [SerializeField] private ExpressionType blendShapeType;

    public BlendShapeSO BlendShape => blendShapeSO;
    public ExpressionType BlendShapeType => blendShapeType;
}

/// <summary>
/// Collection of BlendShapeDatas to be applied to the head's model blend shapes.
/// </summary>
[CreateAssetMenu(fileName = "BlendShapesCollectionSO", menuName = "SO/BlendShapesCollectionSO")]
public class BlendShapesCollectionSO : ScriptableObject
{
    [SerializeField] private List<BlendShapeData> blendShapes = new List<BlendShapeData>();

    public bool TryGetValue(ExpressionType blendShapeType, out BlendShapeData targetData)
    {
        targetData = default;
        foreach (var blendShapeData in blendShapes)
        {
            if (blendShapeData.BlendShapeType == blendShapeType)
            {
                targetData = blendShapeData;
                return true;
            }
        }

        Debug.LogWarning($"[BlendShapesCollectionSO] Blend shape with type '{blendShapeType}' not found.");
        return false;
    }
}
