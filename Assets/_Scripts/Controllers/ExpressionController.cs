using System;
using System.Collections;
using System.Collections.Generic;
using com.trashpandaboy.events;
using UnityEngine;

public enum ExpressionType { None, Idle, DistantFace, Hit, Weird }

public enum EasingType
{
    Linear, EaseIn, EaseOut, EaseInOut, QuadIn, QuadOut, QuadInOut,
    CubicIn, CubicOut, CubicInOut, QuartIn, QuartOut, QuartInOut,
    QuintIn, QuintOut, QuintInOut, SinIn, SinOut, SinInOut,
    ExpoIn, ExpoOut, ExpoInOut, ElasticIn, ElasticOut, ElasticInOut,
    BounceIn, BounceOut, BounceInOut, BackIn, BackOut, BackInOut
}

[Serializable]
public class ExpressionsData
{
    public ExpressionType expressionType;
    public BlendShapeSO blendShapeData;
    // public float expressionDuration;
}

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class ExpressionController : MonoBehaviour
{
    public List<ExpressionsData> blendShapeSOs = new List<ExpressionsData>();
    public ExpressionType currentExpression = ExpressionType.Idle;
    public EasingType easingType = EasingType.EaseInOut;

    private Dictionary<ExpressionType, BlendShapeSO> m_blendShapesDict = new();
    private SkinnedMeshRenderer _skinnedMeshRenderer;
    private Coroutine _currentCoroutine;

    private void OnEnable()
    {
        _skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

        m_blendShapesDict.Clear();

        foreach (var data in blendShapeSOs)
        {
            m_blendShapesDict[data.expressionType] = data.blendShapeData;
        }

        EventDispatcher.StartListening(EventType.Collision.ToString(), HandleOnCollision);
    }

    private void OnDisable()
    {
        EventDispatcher.StopListening(EventType.Collision.ToString(), HandleOnCollision);
    }

    private void Update()
    {
        // Debug input
        if (Input.GetKeyDown(KeyCode.Q)) SwitchExpression(ExpressionType.DistantFace);
        if (Input.GetKeyDown(KeyCode.W)) SwitchExpression(ExpressionType.Hit);
        if (Input.GetKeyDown(KeyCode.E)) SwitchExpression(ExpressionType.Weird);
        if (Input.GetKeyDown(KeyCode.R)) SwitchExpression(ExpressionType.Idle);
    }

    public void SwitchExpression(ExpressionType newExpression)
    {
        if (!_skinnedMeshRenderer || !m_blendShapesDict.ContainsKey(newExpression)) return;

        if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);

        Debug.Log($"[ExpressionController] Switching to expression: {newExpression}, duration {m_blendShapesDict[newExpression].blendShapeDuration}", this);
        _currentCoroutine = StartCoroutine(AnimateToExpression(newExpression, m_blendShapesDict[newExpression].blendShapeDuration));
    }

    private IEnumerator AnimateToExpression(ExpressionType newExpression, float holdDuration)
    {
        ExpressionType from = currentExpression;
        ExpressionType to = newExpression;
        currentExpression = to;

        float time = 0f;
        float holdDurationOneWay = holdDuration * 0.5f;

        while (time < holdDurationOneWay)
        {
            float t = ApplyEasing(time / holdDurationOneWay, easingType);
            ApplyBlendShapeLerp(from, to, t);
            time += Time.deltaTime;
            yield return null;
        }

        ApplyBlendShapeLerp(from, to, 1f);

        // Attendi la durata dell’espressione
        yield return new WaitForSeconds(holdDurationOneWay);

        // Ritorna all’Idle
        ExpressionType backToIdle = ExpressionType.Idle;
        from = to;
        to = backToIdle;
        time = 0f;

        while (time < holdDurationOneWay)
        {
            float t = ApplyEasing(time / holdDurationOneWay, easingType);
            ApplyBlendShapeLerp(from, to, t);
            time += Time.deltaTime;
            yield return null;
        }

        ApplyBlendShapeLerp(from, to, 1f);
        currentExpression = backToIdle;
        _currentCoroutine = null;
    }

    private void ApplyBlendShapeLerp(ExpressionType from, ExpressionType to, float t)
    {
        // Debug.Log($"[ExpressionController] Applying blend shape lerp from {from} to {to} at time {t}", this);

        var fromData = m_blendShapesDict[from].blendShapeDatas;
        var toData = m_blendShapesDict[to].blendShapeDatas;

        for (int i = 0; i < toData.Count; i++)
        {
            float fromValue = i < fromData.Count ? fromData[i] : 0f;
            float toValue = toData[i];
            float lerped = Mathf.Lerp(fromValue, toValue, t);
            _skinnedMeshRenderer.SetBlendShapeWeight(i, lerped);

            if(i == 0)
                Debug.Log($"Blend shape {i} from {fromValue} to {toValue} at time {t}, lerped {lerped}", this);
        }
    }

    protected float ApplyEasing(float t, EasingType easingType)
    {
        switch (easingType)
        {
            case EasingType.Linear: return t;
            case EasingType.EaseIn: return t * t;
            case EasingType.EaseOut: return 1 - Mathf.Pow(1 - t, 2);
            case EasingType.EaseInOut: return t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
            case EasingType.QuadIn: return t * t;
            case EasingType.QuadOut: return 1 - (1 - t) * (1 - t);
            case EasingType.QuadInOut: return t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
            case EasingType.CubicIn: return t * t * t;
            case EasingType.CubicOut: return 1 - Mathf.Pow(1 - t, 3);
            case EasingType.CubicInOut: return t < 0.5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
            case EasingType.QuartIn: return t * t * t * t;
            case EasingType.QuartOut: return 1 - Mathf.Pow(1 - t, 4);
            case EasingType.QuartInOut: return t < 0.5f ? 8 * t * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 4) / 2;
            case EasingType.QuintIn: return t * t * t * t * t;
            case EasingType.QuintOut: return 1 - Mathf.Pow(1 - t, 5);
            case EasingType.QuintInOut: return t < 0.5f ? 16 * t * t * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 5) / 2;
            case EasingType.SinIn: return 1 - Mathf.Cos((t * Mathf.PI) / 2);
            case EasingType.SinOut: return Mathf.Sin((t * Mathf.PI) / 2);
            case EasingType.SinInOut: return -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
            case EasingType.ExpoIn: return t == 0 ? 0 : Mathf.Pow(2, 10 * t - 10);
            case EasingType.ExpoOut: return t == 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
            case EasingType.ExpoInOut:
                return t == 0 ? 0 : t == 1 ? 1 : t < 0.5f
                    ? Mathf.Pow(2, 20 * t - 10) / 2
                    : (2 - Mathf.Pow(2, -20 * t + 10)) / 2;
            case EasingType.ElasticIn:
                return Mathf.Sin(13 * Mathf.PI / 2 * t) * Mathf.Pow(2, 10 * (t - 1));
            case EasingType.ElasticOut:
                return Mathf.Sin(-13 * Mathf.PI / 2 * (t + 1)) * Mathf.Pow(2, -10 * t) + 1;
            case EasingType.ElasticInOut:
                return t < 0.5f
                    ? 0.5f * Mathf.Sin(13 * Mathf.PI / 2 * (2 * t)) * Mathf.Pow(2, 10 * (2 * t - 1))
                    : 0.5f * (Mathf.Sin(-13 * Mathf.PI / 2 * (2 * t - 1 + 1)) * Mathf.Pow(2, -10 * (2 * t - 1)) + 2);
            case EasingType.BounceIn:
                return 1 - ApplyEasing(1 - t, EasingType.BounceOut);
            case EasingType.BounceOut:
                if (t < 1 / 2.75f) return 7.5625f * t * t;
                else if (t < 2 / 2.75f) return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
                else if (t < 2.5 / 2.75) return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
                else return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
            case EasingType.BounceInOut:
                return t < 0.5f
                    ? (1 - ApplyEasing(1 - 2 * t, EasingType.BounceOut)) / 2
                    : (1 + ApplyEasing(2 * t - 1, EasingType.BounceOut)) / 2;
            case EasingType.BackIn:
                float c1 = 1.70158f;
                float c3 = c1 + 1;
                return c3 * t * t * t - c1 * t * t;
            case EasingType.BackOut:
                c1 = 1.70158f;
                c3 = c1 + 1;
                return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
            case EasingType.BackInOut:
                c1 = 1.70158f * 1.525f;
                return t < 0.5f
                    ? (Mathf.Pow(2 * t, 2) * ((c1 + 1) * 2 * t - c1)) / 2
                    : (Mathf.Pow(2 * t - 2, 2) * ((c1 + 1) * (t * 2 - 2) + c1) + 2) / 2;
            default: return t;
        }
    }
    private void HandleOnCollision(object data)
    {
        if (data is CollisionEventData collisionEventData)
        {
            SwitchExpression(collisionEventData.targetExpression);
        }
    }
}
