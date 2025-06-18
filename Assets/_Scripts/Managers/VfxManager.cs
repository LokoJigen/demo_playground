using com.trashpandaboy.events;
using EasyButtons;
using UnityEngine;

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

public class VfxManager : MonoBehaviour
{
    public static VfxManager instance;

    public int vfxTarget = 1;
    [Button]
    public void TriggerVfxTarget()
    {
        if (!Application.isPlaying) return;

        Debug.Log($"Triggering VFX target: {vfxTarget}");
        EventDispatcher.TriggerEvent(EventType.Vfx.ToString(), new VfxEventData(vfxTarget, transform.position));
    }

    [SerializeField] private PrefabsHolderSO _vfxHolderSO;

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
        EventDispatcher.StartListening(EventType.Vfx.ToString(), OnVfxEvent);
        // EventDispatcher.TriggerEvent(EventType.Vfx.ToString(), new VfxEventData(1, Vector3.one));
    }

    private void OnDisable()
    {
        EventDispatcher.StopListening(EventType.Vfx.ToString(), OnVfxEvent);
    }

    private void OnVfxEvent(object data)
    {
        if (data is VfxEventData vfxEventData)
        {
            PlayVfx(vfxEventData);
        }
    }

    private void PlayVfx(VfxEventData vfxEventData)
    {
        Debug.Log($"Playing VFX with ID: {vfxEventData}");
        GameObject go = Instantiate(_vfxHolderSO.GetPrefabDataByIndex(vfxEventData.vfxId).prefab, position: vfxEventData.worldPosition, rotation: Quaternion.identity);
    }



}
