/// <summary>
/// Controls Fresnel-Lit shader to give visual feedback on each hit.
/// </summary>
public class FresnelLitControllerHitFeedback : FresnelLitToonController
{
    protected override void EnableVisualFeedback()
    {
        _material.SetFloat(materialPulseProperty, 1f);
        _material.SetFloat(materialFresnelToggleProperty, 1f);
    }

    protected override void DisableVisualFeedback()
    {
        _material.SetFloat(materialFresnelToggleProperty, 0f);
        _material.SetFloat(materialPulseProperty, 0f);
    }
}
