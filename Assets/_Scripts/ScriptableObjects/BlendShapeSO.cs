using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Atomic class holding desired blend shape weights that rapresents an intended expression.
/// </summary>
[CreateAssetMenu(fileName = "BlendShapeSO", menuName = "SO/BlendShapeSO")]
public class BlendShapeSO : ScriptableObject
{
    public float blendShapeDuration = 1f;
    public List<float> blendShapeDatas = new List<float>();

}