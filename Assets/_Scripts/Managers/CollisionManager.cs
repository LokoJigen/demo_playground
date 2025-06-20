using System.Collections;
using System.Text;
using com.trashpandaboy.events;
using UnityEngine;

/// <summary>
/// Used to pass collision event data to other scripts.
/// </summary>
public class CollisionEventData
{
    public ExpressionType targetExpression;
    public bool isEpicCollision;
    public int hitCount;
    public int hitForEpic;

    public CollisionEventData(ExpressionType targetExpression, bool isEpicCollision = false, int hitCount = 0, int hitForEpic = 3)
    {
        this.targetExpression = targetExpression;
        this.isEpicCollision = isEpicCollision;
        this.hitCount = hitCount;
        this.hitForEpic = hitForEpic;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append($"[Collision Event Data]");
        sb.AppendLine($"Target Expression: {targetExpression}\n");
        sb.AppendLine($"Is Epic Collision: {isEpicCollision}\n");
        sb.AppendLine($"Collision Count: {hitCount}\n");
        sb.AppendLine($"Hit for Epic: {hitForEpic}\n");

        return sb.ToString();
    }
}

/// <summary>
/// This class Handles collision events.
/// Keeps track of the collisions and triggers the corresponding events.
/// </summary>
public class CollisionManager : MonoBehaviour
{

    #region VARIABLES

    [SerializeField] private float _collisionInterval = 1f;

    private float _collisionCooldownTimer = 1f;

    private Coroutine _collisionIntervalCoroutine;
    private float _intervalStartTime = 0f;
    private int m_hitCounter = 0;
    private int m_hitForEpic = 3;

    #endregion

    #region UNITY CALLBACKS

    private void OnEnable()
    {
        _collisionCooldownTimer = _collisionInterval;

        EventDispatcher.StartListening(EventType.RaycastHit, HandleRaycastHit);
    }

    private void OnDisable()
    {
        StopCollisionInterval();
        EventDispatcher.StopListening(EventType.RaycastHit, HandleRaycastHit);
    }

    #endregion

    #region PRIVATE METHODS

    private void HandleRaycastHit(object data)
    {
        if (data is RaycastEventData raycastEventData)
        {
            m_hitCounter++;

            bool isEpicCollision = m_hitCounter % m_hitForEpic == 0;

            if (isEpicCollision)
            {
                m_hitCounter = 0;
                m_hitForEpic += Random.Range(1, 5);
            }
            // Debug.Log($"[CollisionManager] Hit counter: {m_hitCounter}, % {m_hitCounter % HITS_FOR_EPIC}, isEpicCollision: {isEpicCollision}");

            EventDispatcher.TriggerEvent(EventType.Vfx, new VfxEventData(1, raycastEventData.raycastHit.point));
            EventDispatcher.TriggerEvent(EventType.Collision, new CollisionEventData(ExpressionController.GetRandomExpression(), isEpicCollision, m_hitCounter, m_hitForEpic));
            TriggerCollisionInterval();
        }
    }

    private void TriggerCollisionInterval()
    {
        if (_collisionIntervalCoroutine != null)
            StopCoroutine(_collisionIntervalCoroutine);

        _intervalStartTime = Time.time;
        _collisionIntervalCoroutine = StartCoroutine(CollisionIntervalCoroutine());
    }

    private void StopCollisionInterval()
    {
        if (_collisionIntervalCoroutine != null)
            StopCoroutine(_collisionIntervalCoroutine);

        _collisionIntervalCoroutine = null;
    }

    private IEnumerator CollisionIntervalCoroutine()
    {
        while (Time.time - _intervalStartTime < _collisionCooldownTimer)
        {
            // Debug.Log($"[CollisionManager] Collision interval triggered at time {Time.time}, remaining time: {(_collisionCooldownTimer - (Time.time - _intervalStartTime))}");
            yield return new WaitForSeconds(_collisionCooldownTimer);
        }

        _collisionIntervalCoroutine = null;
    }

    #endregion

}