using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BlendShapeSO", menuName = "SO/BlendShapeSO")]
public class BlendShapeSO : ScriptableObject
{
    public List<float> blendShapeDatas = new List<float>();

}