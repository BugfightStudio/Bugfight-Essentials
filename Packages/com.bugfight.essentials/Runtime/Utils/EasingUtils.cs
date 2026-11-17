using System;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace Bugfight.Utils
{
    public enum Ease
    {
        Linear,
        Custom,

        InSine, OutSine, InOutSine,
        InQuad, OutQuad, InOutQuad,
        InCubic, OutCubic, InOutCubic,
        InQuart, OutQuart, InOutQuart,
        InQuint, OutQuint, InOutQuint,
        InExpo, OutExpo, InOutExpo,
        InCirc, OutCirc, InOutCirc,
        InBack, OutBack, InOutBack,
        InElastic, OutElastic, InOutElastic,
        InBounce, OutBounce, InOutBounce
    }

    public static class EasingUtils
    {
        public static float GetEase(float lerp, Ease easing)
        {
            return easing switch
            {
                Ease.Linear => lerp,

                Ease.InSine => Easing.InSine(lerp),
                Ease.OutSine => Easing.OutSine(lerp),
                Ease.InOutSine => Easing.InOutSine(lerp),

                Ease.InQuad => Easing.InQuad(lerp),
                Ease.OutQuad => Easing.OutQuad(lerp),
                Ease.InOutQuad => Easing.InOutQuad(lerp),

                Ease.InCubic => Easing.InCubic(lerp),
                Ease.OutCubic => Easing.OutCubic(lerp),
                Ease.InOutCubic => Easing.InOutCubic(lerp),

                Ease.InCirc => Easing.InCirc(lerp),
                Ease.OutCirc => Easing.OutCirc(lerp),
                Ease.InOutCirc => Easing.InOutCirc(lerp),

                Ease.InBack => Easing.InBack(lerp),
                Ease.OutBack => Easing.OutBack(lerp),
                Ease.InOutBack => Easing.InOutBack(lerp),

                Ease.InElastic => Easing.InElastic(lerp),
                Ease.OutElastic => Easing.OutElastic(lerp),
                Ease.InOutElastic => Easing.InOutElastic(lerp),

                Ease.InBounce => Easing.InBounce(lerp),
                Ease.OutBounce => Easing.OutBounce(lerp),
                Ease.InOutBounce => Easing.InOutBounce(lerp),

                Ease.InQuart => lerp * lerp * lerp * lerp,
                Ease.OutQuart => 1 - Mathf.Pow(1 - lerp, 4),
                Ease.InOutQuart => lerp < 0.5 ? 8 * lerp * lerp * lerp * lerp : 1 - Mathf.Pow(-2 * lerp + 2, 4) / 2,

                Ease.InQuint => lerp * lerp * lerp * lerp * lerp,
                Ease.OutQuint => 1 - Mathf.Pow(1 - lerp, 5),
                Ease.InOutQuint => lerp < 0.5 ? 16 * lerp * lerp * lerp * lerp * lerp : 1 - Mathf.Pow(-2 * lerp + 2, 5) / 2,

                Ease.InExpo => lerp == 0 ? 0 : Mathf.Pow(2, 10 * lerp - 10),
                Ease.OutExpo => Math.Abs(lerp - 1) < Mathf.Epsilon ? 1 : 1 - Mathf.Pow(2, -10 * lerp),
                Ease.InOutExpo => lerp == 0 ? 0 : Math.Abs(lerp - 1) < Mathf.Epsilon ? 1 : lerp < 0.5 ? Mathf.Pow(2, 20 * lerp - 10) / 2 : (2 - Mathf.Pow(2, -20 * lerp + 10)) / 2,

                _ => lerp
            };
        }
    }
}
