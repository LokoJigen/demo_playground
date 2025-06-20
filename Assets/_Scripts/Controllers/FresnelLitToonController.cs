using com.trashpandaboy.events;
using UnityEngine;

/// <summary>
/// Abstract class to control Fresnel shader
/// </summary>
public abstract class FresnelLitToonController : MonoBehaviour
{
    #region VARIABLES

    [SerializeField] protected Renderer targetRenderer;
    [SerializeField] protected string materialFresnelToggleProperty = "_FresnelToggle";
    [SerializeField] protected string materialFresnelThresholdProperty = "_FresnelThreshold";
    [SerializeField] protected string materialPulseProperty = "_UseFresnelPulse";
    [SerializeField] protected string materialPulseSpeed = "_FresnelPulseSpeed";
    [SerializeField] protected string materialPulseAmplitude = "_FresnelPulseAmplitude";
    [SerializeField] protected float feedbackDuration = 1.0f;

    protected Material _material;
    protected CollisionEventData _lastCollisionEventData;

    #endregion

    #region UNITY CALLBACKS

    protected void Awake()
    {
        if (targetRenderer != null)
            _material = targetRenderer.material;

        Debug.Assert(_material != null, "Target renderer should have a material assigned.", this);
    }

    protected void OnEnable()
    {
        EventDispatcher.StartListening(EventType.Collision, OnCollisionEvent);
    }

    protected void OnDisable()
    {
        EventDispatcher.StopListening(EventType.Collision, OnCollisionEvent);
    }

    #endregion

    #region ABSTRACT METHODS

    protected abstract void EnableVisualFeedback();

    protected abstract void DisableVisualFeedback();

    #endregion

    protected void OnCollisionEvent(object data)
    {
        if (_material == null) return;

        if (data is CollisionEventData collisionEventData)
        {
            // Debug.Log($"[FresnelLitToonController] {collisionEventData}");
            _lastCollisionEventData = collisionEventData;

            EnableVisualFeedback();

            CancelInvoke(nameof(DisableVisualFeedback));
            Invoke(nameof(DisableVisualFeedback), feedbackDuration);
        }
    }

}
