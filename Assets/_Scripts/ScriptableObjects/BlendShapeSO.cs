using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BlendShapeSO", menuName = "SO/BlendShapeSO")]
public class BlendShapeSO : ScriptableObject
{
    public float blendShapeDuration = 1f;
    public List<float> blendShapeDatas = new List<float>();

}