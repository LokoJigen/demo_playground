using System.Collections;
using System.Collections.Generic;
using com.trashpandaboy.events;
using EasyButtons;
using UnityEngine;

/// <summary>
/// This class is used as a payload for the Vfx event in the game. It contains the position of the VFX and its ID.
/// </summary>
public class VfxEventData
{
    public Vector3 worldPosition;
    public int vfxId;

    public VfxEventData(int vfxId, Vector3 worldPosition)
    {
        this.vfxId = vfxId;
        this.worldPosition = worldPosition;
    }
}


/// <summary>
/// This class manages the instantiation and recycling of VFX objects.
/// </summary>
public class VfxManager : MonoBehaviour
{

    private class PooledVfxInstance
    {
        public GameObject gameObject;
        public bool isInUse;
        public string id;

        public PooledVfxInstance(GameObject go, string id)
        {
            gameObject = go;
            isInUse = false;
            this.id = id;
        }
    }

    [Button]
    public void TriggerVfxTarget()
    {
        if (!Application.isPlaying) return;

        Debug.Log($"Triggering VFX target: {vfxTarget}");
        EventDispatcher.TriggerEvent(EventType.Vfx, new VfxEventData(vfxTarget, transform.position));
    }

    #region VARIABLES

    [SerializeField] private PrefabsHolderSO _vfxHolderSO;
    public static VfxManager instance;
    public int vfxTarget = 1;
    private Dictionary<string, List<PooledVfxInstance>> _pooledVfx = new Dictionary<string, List<PooledVfxInstance>>();
    private const float USAGE_LOCK_DURATION = 1.0f;
    private const float REACTIVATION_DELAY = 0.01f;

    #endregion

    #region UNITY CALLBACKS

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        EventDispatcher.StartListening(EventType.Vfx, OnVfxEvent);

        if (_pooledVfx.Count == 0)
        {
            _vfxHolderSO.InitializeRuntimeDictionary();

            foreach (var data in _vfxHolderSO.prefabDataList)
            {
                if (data == null || data.prefab == null)
                    continue;

                if (!_pooledVfx.ContainsKey(data.id))
                    _pooledVfx[data.id] = new List<PooledVfxInstance>();

                var pool = _pooledVfx[data.id];

                for (int i = 0; i < 10; i++) // Pool di 10 istanze per ogni prefab
                {
                    GameObject instance = Instantiate(data.prefab);
                    instance.SetActive(false); // Importante: parte disattivato
                    pool.Add(new PooledVfxInstance(instance, data.id));
                }
            }

        }
    }


    private void OnDisable()
    {
        EventDispatcher.StopListening(EventType.Vfx, OnVfxEvent);
    }

    #endregion

    #region PRIVATE METHODS

    private void OnVfxEvent(object data)
    {
        if (data is VfxEventData vfxEventData)
        {
            PlayVfx(vfxEventData);
        }
    }

    private void PlayVfx(VfxEventData vfxEventData)
    {
        var prefabData = _vfxHolderSO.GetRandomPrefabData();

        GameObject instance = GetPooledVfxInstance(prefabData.id, vfxEventData.worldPosition);

        instance.transform.position = vfxEventData.worldPosition;
        instance.transform.rotation = Quaternion.identity;
    }



    private GameObject GetPooledVfxInstance(string id, Vector3 position)
    {
        if (!_pooledVfx.ContainsKey(id))
        {
            _pooledVfx[id] = new List<PooledVfxInstance>();
        }

        var pool = _pooledVfx[id];

        pool.RemoveAll(p => p.gameObject == null);

        // Cerca un'istanza disponibile
        foreach (var pooled in pool)
        {
            if (!pooled.isInUse && !pooled.gameObject.activeInHierarchy)
            {
                StartCoroutine(MarkInUseTemporarily(pooled));
                return pooled.gameObject;
            }
        }

        var prefabData = _vfxHolderSO.GetRandomPrefabData();
        var instance = Instantiate(prefabData.prefab, position, Quaternion.identity);
        var pooledInstance = new PooledVfxInstance(instance, prefabData.id);
        _pooledVfx[id].Add(pooledInstance);
        StartCoroutine(MarkInUseTemporarily(pooledInstance));
        return instance;
    }

    private IEnumerator MarkInUseTemporarily(PooledVfxInstance pooled)
    {
        if (pooled.gameObject == null)
            yield break;

        pooled.isInUse = true;
        pooled.gameObject.SetActive(true);

        yield return new WaitForSeconds(USAGE_LOCK_DURATION);

        if (pooled.gameObject != null)  // 🔒
            pooled.gameObject.SetActive(false);

        pooled.isInUse = false;
    }


    #endregion

}
