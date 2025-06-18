using System;
using System.Collections.Generic;
using UnityEngine;

public enum BlendShapeType
{
    None,
    Hit,
    Weird,
    DistantFace,
}

[Serializable]
public class BlendShapeData
{
    [SerializeField] private BlendShapeSO blendShapeSO;
    [SerializeField] private BlendShapeType blendShapeType;

    public BlendShapeSO BlendShape => blendShapeSO;
    public BlendShapeType BlendShapeType => blendShapeType;
}

[CreateAssetMenu(fileName = "BlendShapesCollectionSO", menuName = "SO/BlendShapesCollectionSO")]
public class BlendShapesCollectionSO : ScriptableObject
{
    [SerializeField] private List<BlendShapeData> blendShapes = new List<BlendShapeData>();

    public bool TryGetValue(BlendShapeType blendShapeType, out BlendShapeData targetData)
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
        return false;
    }
}
