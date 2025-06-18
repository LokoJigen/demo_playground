#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class PrefabData
{
    public string id;
    public GameObject prefab;
    public int prefabType;
}

[CreateAssetMenu(fileName = "PrefabsHolderSO", menuName = "SO/PrefabsHolderSO")]
public class PrefabsHolderSO : ScriptableObject
{
    [Header("Default Prefab")]
    public GameObject defaultPrefab;

    [Header("Source list of prefabs to register")]
    public List<GameObject> prefabs = new List<GameObject>();

    [Header("Cached generated PrefabData")]
    public List<PrefabData> prefabDataList = new List<PrefabData>();

    private Dictionary<string, PrefabData> _prefabDataDict;

#if UNITY_EDITOR
    [ContextMenu("Add Only New Prefabs")]
    private void AddOnlyNewPrefabsWithConfirmation()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Add New Prefabs",
            "This will add any new unique prefabs from the list. Existing entries will be preserved.\nDo you want to continue?",
            "Yes", "Cancel"
        );

        if (confirmed)
        {
            AddOnlyNewPrefabs();
            EditorUtility.SetDirty(this);
        }
    }

    [ContextMenu("Remove Unreferenced Prefabs")]
    private void RemoveUnreferencedPrefabs()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Remove Unreferenced Prefabs",
            "This will remove any PrefabData entries whose prefabs are no longer in the 'prefabs' list.\nContinue?",
            "Yes", "Cancel"
        );

        if (confirmed)
        {
            var referencedSet = new HashSet<GameObject>(prefabs);
            prefabDataList.RemoveAll(pd => pd.prefab == null || !referencedSet.Contains(pd.prefab));
            EditorUtility.SetDirty(this);
        }
    }
#endif

    private void OnValidate()
    {
        AddOnlyNewPrefabs();
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    private void AddOnlyNewPrefabs()
    {
        var existingPrefabs = new HashSet<GameObject>(prefabDataList.Select(pd => pd.prefab));
        int nextTypeIndex = prefabDataList.Count;

        foreach (var prefab in prefabs)
        {
            if (prefab == null || existingPrefabs.Contains(prefab))
                continue;

            prefabDataList.Add(new PrefabData
            {
                id = prefab.name,
                prefab = prefab,
                prefabType = nextTypeIndex++
            });
        }
    }

    // Runtime use
    public void InitializeRuntimeDictionary()
    {
        if (_prefabDataDict != null)
            return;

        _prefabDataDict = new Dictionary<string, PrefabData>();

        foreach (var data in prefabDataList)
        {
            if (data != null && !_prefabDataDict.ContainsKey(data.id))
            {
                _prefabDataDict.Add(data.id, data);
            }
        }
    }

    public PrefabData GetPrefabDataById(string id)
    {
        if (_prefabDataDict == null)
            InitializeRuntimeDictionary();

        if (_prefabDataDict.TryGetValue(id, out var data))
            return data;

        Debug.LogWarning($"[PrefabsHolderSO] Prefab with ID '{id}' not found.");
        return new PrefabData { id = id, prefab = defaultPrefab, prefabType = -1 };
    }

    public PrefabData GetPrefabDataByIndex(int index)
    {
        if (_prefabDataDict == null)
            InitializeRuntimeDictionary();

        if (index >= 0 && index < _prefabDataDict.Count)
        {
            // Access by index via prefabDataList
            return prefabDataList[index];
        }

        Debug.LogWarning($"[PrefabsHolderSO] Prefab index '{index}' is out of range.");
        return new PrefabData { id = "", prefab = defaultPrefab, prefabType = -1 };
    }

}
