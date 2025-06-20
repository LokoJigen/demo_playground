using System;
using System.Linq;

public static class ProjectUtils
{

    public static T GetRandomEnumValue<T>(params T[] excluded) where T : Enum
    {
        var values = Enum.GetValues(typeof(T)).Cast<T>();
        var filtered = values.Except(excluded).ToList();

        if (filtered.Count == 0)
            return default(T);

        return filtered[UnityEngine.Random.Range(0, filtered.Count)];
    }


}
