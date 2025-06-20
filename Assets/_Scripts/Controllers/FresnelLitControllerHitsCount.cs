using UnityEngine;

/// <summary>
/// Controls Fresnel shader to give visual feedback based on the number of hits for next epic collision.
/// </summary>
public class FresnelLitControllerHitsCount : FresnelLitToonController
{
    protected override void EnableVisualFeedback()
    {
        if (_lastCollisionEventData == null) return;

        float t = Mathf.Clamp01((float)_lastCollisionEventData.hitCount / _lastCollisionEventData.hitForEpic);
        float thresholdValue = Mathf.Lerp(0.8f, 0.15f, t);
        float speedValue = Mathf.Lerp(1f, 20f, t);

        _material.SetFloat(materialPulseProperty, 1f);
        _material.SetFloat(materialPulseSpeed, speedValue);
        _material.SetFloat(materialFresnelThresholdProperty, thresholdValue);

    }

    protected override void DisableVisualFeedback()
    {
        if (_lastCollisionEventData == null) return;

        if (_lastCollisionEventData.isEpicCollision)
        {
            _material.SetFloat(materialPulseProperty, 0f);
            _material.SetFloat(materialPulseSpeed, 0f);
            _material.SetFloat(materialFresnelThresholdProperty, 1f);
        }
    }
}