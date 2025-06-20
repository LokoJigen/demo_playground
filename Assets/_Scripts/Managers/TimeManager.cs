using com.trashpandaboy.events;
using UnityEngine;

/// <summary>
/// This class manages the timescale during collisions.
/// </summary>
public class TimeManager : MonoBehaviour
{

    public static bool IsSlowMo = false;

    #region UNITY CALLBACKS

    private void OnEnable()
    {
        EventDispatcher.StartListening(EventType.Collision, HandleOnCollision);
    }

    private void OnDisable()
    {
        EventDispatcher.StopListening(EventType.Collision, HandleOnCollision);
    }

    #endregion

    #region PRIVATE METHODS

    private void HandleOnCollision(object data)
    {
        if (data is CollisionEventData collisionEventData)
        {
            if (!collisionEventData.isEpicCollision) return;

            if (!IsSlowMo)
            {
                // Debug.Log($"[TimeManager] SlowMo TIME");
                SetSlowMo(true);

                CancelInvoke(nameof(ResetTimeScale));
                Invoke(nameof(ResetTimeScale), 2f);
            }
        }
    }

    private void SetSlowMo(bool isSlowMo = false)
    {
        Time.timeScale = isSlowMo ? 0.5f : 1f;
        IsSlowMo = isSlowMo;
    }

    private void ResetTimeScale()
    {
        Time.timeScale = 1f;
        IsSlowMo = false;
    }


    #endregion

}