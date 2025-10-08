
namespace MGAlienLib.Utility
{
    public enum eEasingType
    {
        Linear,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseInQuart,
        EaseOutQuart,
        EaseInOutQuart,
        EaseInQuint,
        EaseOutQuint,
        EaseInOutQuint,
        EaseInSine,
        EaseOutSine,
        EaseInOutSine,
        EaseInExpo,
        EaseOutExpo,
        EaseInOutExpo,
        EaseInCirc,
        EaseOutCirc,
        EaseInOutCirc,
        EaseInBack,
        EaseOutBack,
        EaseInOutBack,
        EaseInElastic,
        EaseOutElastic,
        EaseInOutElastic,
        EaseInBounce,
        EaseOutBounce,
        EaseInOutBounce
    }

    public static class TweenerUtility
    {


        public static float TweenFloat(eEasingType easingType, float startValue, float endValue, float duration, float currentTime)
        {
            float progress = currentTime / duration;
            progress = Mathf.Clamp(progress, 0f, 1f);

            switch (easingType)
            {
                case eEasingType.Linear:
                    Logger.Log(progress);
                    return Mathf.Lerp(startValue, endValue, progress);
                case eEasingType.EaseInQuad:
                    return Mathf.Lerp(startValue, endValue, progress * progress);
                case eEasingType.EaseOutQuad:
                    return Mathf.Lerp(startValue, endValue, 1 - (1 - progress) * (1 - progress));
                case eEasingType.EaseInOutQuad:
                    if (progress < 0.5f)
                    {
                        return Mathf.Lerp(startValue, endValue, 2 * progress * progress);
                    }
                    else
                    {
                        return Mathf.Lerp(startValue, endValue, 1 - Mathf.Pow(-2 * progress + 2, 2) / 2);
                    }
                case eEasingType.EaseInCubic:
                    return Mathf.Lerp(startValue, endValue, progress * progress * progress);
                case eEasingType.EaseOutCubic:
                    return Mathf.Lerp(startValue, endValue, 1 - Mathf.Pow(1 - progress, 3));
                case eEasingType.EaseInOutCubic:
                    if (progress < 0.5f)
                    {
                        return Mathf.Lerp(startValue, endValue, 4 * progress * progress * progress);
                    }
                    else
                    {
                        return Mathf.Lerp(startValue, endValue, 1 - Mathf.Pow(-2 * progress + 2, 3) / 2);
                    }
                case eEasingType.EaseInQuart:
                    return Mathf.Lerp(startValue, endValue, progress * progress * progress * progress);
                case eEasingType.EaseOutQuart:
                    return Mathf.Lerp(startValue, endValue, 1 - Mathf.Pow(1 - progress, 4));
                case eEasingType.EaseInOutQuart:
                    if (progress < 0.5f)
                    {
                        return Mathf.Lerp(startValue, endValue, 8 * progress * progress * progress * progress);
                    }
                    else
                    {
                        return Mathf.Lerp(startValue, endValue, 1 - Mathf.Pow(-2 * progress + 2, 4) / 2);
                    }
                case eEasingType.EaseInQuint:
                    return Mathf.Lerp(startValue, endValue, progress * progress * progress * progress * progress);
                case eEasingType.EaseOutQuint:
                    return Mathf.Lerp(startValue, endValue, 1 - Mathf.Pow(1 - progress, 5));
                case eEasingType.EaseInOutQuint:
                    if (progress < 0.5f)
                    {
                        return Mathf.Lerp(startValue, endValue, 16 * progress * progress * progress * progress * progress);
                    }
                    else
                    {
                        return Mathf.Lerp(startValue, endValue, 1 - Mathf.Pow(-2 * progress + 2, 5) / 2);
                    }
                case eEasingType.EaseInSine:
                    return Mathf.Lerp(startValue, endValue, 1 - Mathf.Cos(progress * Mathf.PIOver2));
                case eEasingType.EaseOutSine:
                    return Mathf.Lerp(startValue, endValue, Mathf.Sin(progress * Mathf.PIOver2));
                case eEasingType.EaseInOutSine:
                    return Mathf.Lerp(startValue, endValue, -(Mathf.Cos(Mathf.PI * progress) - 1) / 2);
                case eEasingType.EaseInExpo:
                    return Mathf.Lerp(startValue, endValue, progress == 0 ? 0 : Mathf.Pow(2, 10 * progress - 10));
                case eEasingType.EaseOutExpo:
                    return Mathf.Lerp(startValue, endValue, progress == 1 ? 1 : 1 - Mathf.Pow(2, -10 * progress));
                case eEasingType.EaseInOutExpo:
                    if (progress == 0) return startValue;
                    if (progress == 1) return endValue;
                    if (progress < 0.5f)
                    {
                        return Mathf.Lerp(startValue, endValue, Mathf.Pow(2, 20 * progress - 10) / 2);
                    }
                    else
                    {
                        return Mathf.Lerp(startValue, endValue, (2 - Mathf.Pow(2, -20 * progress + 10)) / 2);
                    }
                case eEasingType.EaseInCirc:
                    return Mathf.Lerp(startValue, endValue, 1 - Mathf.Sqrt(1 - Mathf.Pow(progress, 2)));
                case eEasingType.EaseOutCirc:
                    return Mathf.Lerp(startValue, endValue, Mathf.Sqrt(1 - Mathf.Pow(progress - 1, 2)));
                case eEasingType.EaseInOutCirc:
                    if (progress < 0.5f)
                    {
                        return Mathf.Lerp(startValue, endValue, (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * progress, 2))) / 2);
                    }
                    else
                    {
                        return Mathf.Lerp(startValue, endValue, (Mathf.Sqrt(1 - Mathf.Pow(-2 * progress + 2, 2)) + 1) / 2);
                    }
                case eEasingType.EaseInBack:
                    float c1 = 1.70158f;
                    float c3 = c1 + 1;
                    return Mathf.Lerp(startValue, endValue, c3 * progress * progress * progress - c1 * progress * progress);
                case eEasingType.EaseOutBack:
                    float c1out = 1.70158f;
                    float c3out = c1out + 1;
                    return Mathf.Lerp(startValue, endValue, 1 + c3out * Mathf.Pow(progress - 1, 3) + c1out * Mathf.Pow(progress - 1, 2));
                case eEasingType.EaseInOutBack:
                    float c1inout = 1.70158f;
                    float c2inout = c1inout * 1.525f;
                    if (progress < 0.5f)
                    {
                        return Mathf.Lerp(startValue, endValue, (Mathf.Pow(2 * progress, 2) * ((c2inout + 1) * 2 * progress - c2inout)) / 2);
                    }
                    else
                    {
                        return Mathf.Lerp(startValue, endValue, (Mathf.Pow(2 * progress - 2, 2) * ((c2inout + 1) * (progress * 2 - 2) + c2inout) + 2) / 2);
                    }
                case eEasingType.EaseInElastic:
                    float c4 = (2 * Mathf.PI) / 3;
                    if (progress == 0) return startValue;
                    if (progress == 1) return endValue;
                    return Mathf.Lerp(startValue, endValue, -Mathf.Pow(2, 10 * progress - 10) * Mathf.Sin((progress * 10 - 10.75f) * c4));
                case eEasingType.EaseOutElastic:
                    float c5 = (2 * Mathf.PI) / 3;
                    if (progress == 0) return startValue;
                    if (progress == 1) return endValue;
                    return Mathf.Lerp(startValue, endValue, Mathf.Pow(2, -10 * progress) * Mathf.Sin((progress * 10 - 0.75f) * c5) + 1);
                case eEasingType.EaseInOutElastic:
                    float c6 = (2 * Mathf.PI) / 4.5f;
                    if (progress == 0) return startValue;
                    if (progress == 1) return endValue;
                    if (progress < 0.5f)
                    {
                        return Mathf.Lerp(startValue, endValue, -(Mathf.Pow(2, 20 * progress - 10) * Mathf.Sin((20 * progress - 11.125f) * c6)) / 2);
                    }
                    else
                    {
                        return Mathf.Lerp(startValue, endValue, (Mathf.Pow(2, -20 * progress + 10) * Mathf.Sin((20 * progress - 11.125f) * c6)) / 2 + 1);
                    }
                case eEasingType.EaseInBounce:
                    {
                        float bounceProgress = 1 - progress;
                        float bounceValue;
                        if (bounceProgress < (1 / 2.75f))
                        {
                            bounceValue = 7.5625f * bounceProgress * bounceProgress;
                        }
                        else if (bounceProgress < (2 / 2.75f))
                        {
                            bounceValue = 7.5625f * (bounceProgress -= (1.5f / 2.75f)) * bounceProgress + 0.75f;
                        }
                        else if (bounceProgress < (2.5f / 2.75f))
                        {
                            bounceValue = 7.5625f * (bounceProgress -= (2.25f / 2.75f)) * bounceProgress + 0.9375f;
                        }
                        else
                        {
                            bounceValue = 7.5625f * (bounceProgress -= (2.625f / 2.75f)) * bounceProgress + 0.984375f;
                        }
                        return Mathf.Lerp(startValue, endValue, 1 - bounceValue);
                    }
                case eEasingType.EaseOutBounce:
                    if (progress < (1 / 2.75f))
                    {
                        return Mathf.Lerp(startValue, endValue, 7.5625f * progress * progress);
                    }
                    else if (progress < (2 / 2.75f))
                    {
                        return Mathf.Lerp(startValue, endValue, 7.5625f * (progress -= (1.5f / 2.75f)) * progress + 0.75f);
                    }
                    else if (progress < (2.5f / 2.75f))
                    {
                        return Mathf.Lerp(startValue, endValue, 7.5625f * (progress -= (2.25f / 2.75f)) * progress + 0.9375f);
                    }
                    else
                    {
                        return Mathf.Lerp(startValue, endValue, 7.5625f * (progress -= (2.625f / 2.75f)) * progress + 0.984375f);
                    }
                case eEasingType.EaseInOutBounce:
                    {
                        if (progress < 0.5f)
                        {
                            float bounceProgress = 1 - 2 * progress;
                            float bounceValue;
                            if (bounceProgress < (1 / 2.75f))
                            {
                                bounceValue = 7.5625f * bounceProgress * bounceProgress;
                            }
                            else if (bounceProgress < (2 / 2.75f))
                            {
                                bounceValue = 7.5625f * (bounceProgress -= (1.5f / 2.75f)) * bounceProgress + 0.75f;
                            }
                            else if (bounceProgress < (2.5f / 2.75f))
                            {
                                bounceValue = 7.5625f * (bounceProgress -= (2.25f / 2.75f)) * bounceProgress + 0.9375f;
                            }
                            else
                            {
                                bounceValue = 7.5625f * (bounceProgress -= (2.625f / 2.75f)) * bounceProgress + 0.984375f;
                            }
                            return Mathf.Lerp(startValue, endValue, (1 - bounceValue) / 2);
                        }
                        else
                        {
                            float bounceProgress = 2 * progress - 1;
                            float bounceValue;
                            if (bounceProgress < (1 / 2.75f))
                            {
                                bounceValue = 7.5625f * bounceProgress * bounceProgress;
                            }
                            else if (bounceProgress < (2 / 2.75f))
                            {
                                bounceValue = 7.5625f * (bounceProgress -= (1.5f / 2.75f)) * bounceProgress + 0.75f;
                            }
                            else if (bounceProgress < (2.5f / 2.75f))
                            {
                                bounceValue = 7.5625f * (bounceProgress -= (2.25f / 2.75f)) * bounceProgress + 0.9375f;
                            }
                            else
                            {
                                bounceValue = 7.5625f * (bounceProgress -= (2.625f / 2.75f)) * bounceProgress + 0.984375f;
                            }
                            return Mathf.Lerp(startValue, endValue, (1 + bounceValue) / 2);
                        }
                    }
                default:
                    return Mathf.Lerp(startValue, endValue, progress);
            }
        }
    }
}
