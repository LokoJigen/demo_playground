using System.Collections;
using com.trashpandaboy.events;
using UnityEngine;

public class CollisionEventData
{
    public ExpressionType targetExpression;

    public CollisionEventData(ExpressionType targetExpression)
    {
        this.targetExpression = targetExpression;
    }
}

public class CollisionManager : MonoBehaviour
{
    [SerializeField] private float _collisionInterval = 1f;

    private float _collisionCooldownTimer = 1f;

    private Coroutine _collisionIntervalCoroutine;
    private float _intervalStartTime = 0f;


    private void OnEnable()
    {
        _collisionCooldownTimer = _collisionInterval;

        EventDispatcher.StartListening(EventType.RaycastHit.ToString(), HandleRaycastHit);
    }

    private void OnDisable()
    {
        StopCollisionInterval();
        EventDispatcher.StopListening(EventType.RaycastHit.ToString(), HandleRaycastHit);
    }

    private void HandleRaycastHit(object data)
    {
        if (data is RaycastEventData raycastEventData)
        {
            EventDispatcher.TriggerEvent(EventType.Vfx.ToString(), new VfxEventData(1, raycastEventData.raycastHit.point));
            EventDispatcher.TriggerEvent(EventType.Collision.ToString(), new CollisionEventData(ExpressionType.Hit));
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

    private bool IsCollisionIntervalActive() => _collisionIntervalCoroutine != null;

    private IEnumerator CollisionIntervalCoroutine()
    {
        while (Time.time - _intervalStartTime < _collisionCooldownTimer)
        {
            Debug.Log($"[CollisionManager] Collision interval triggered at time {Time.time}, remaining time: {(_collisionCooldownTimer - (Time.time - _intervalStartTime))}");
            yield return new WaitForSeconds(_collisionCooldownTimer);
        }

        _collisionIntervalCoroutine = null;
    }

}
