//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Graphics.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Framework.MathUtils
{
    public static class Interpolation
    {
        public static float Lerp(float value1, float value2, float amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        public static Color4 ValueAt(double time, Color4 startColour, Color4 endColour, double startTime, double endTime, EasingTypes easing = EasingTypes.None)
        {
            if (startColour == endColour)
                return startColour;

            double current = time - startTime;
            double duration = endTime - startTime;

            if (duration == 0 || current == 0)
                return startColour;

            return new Color4(
                            (float)Math.Max(0, Math.Min(1, ApplyEasing(easing, current, startColour.R, endColour.R - startColour.R, duration))),
                            (float)Math.Max(0, Math.Min(1, ApplyEasing(easing, current, startColour.G, endColour.G - startColour.G, duration))),
                            (float)Math.Max(0, Math.Min(1, ApplyEasing(easing, current, startColour.B, endColour.B - startColour.B, duration))),
                            (float)Math.Max(0, Math.Min(1, ApplyEasing(easing, current, startColour.A, endColour.A - startColour.A, duration))));
        }

        public static float ValueAt(double time, float val1, float val2, double startTime, double endTime, EasingTypes easing = EasingTypes.None)
        {
            if (val1 == val2)
                return val1;

            double current = time - startTime;
            double duration = endTime - startTime;

            if (current == 0)
                return val1;
            if (duration == 0)
                return val2;

            return (float)ApplyEasing(easing, current, val1, val2 - val1, duration);
        }

        public static Vector2 ValueAt(double time, Vector2 val1, Vector2 val2, double startTime, double endTime, EasingTypes easing = EasingTypes.None)
        {
            float current = (float)(time - startTime);
            float duration = (float)(endTime - startTime);

            if (duration == 0 || current == 0)
                return val1;

            return new Vector2(
                (float)ApplyEasing(easing, current, val1.X, val2.X - val1.X, duration),
                (float)ApplyEasing(easing, current, val1.Y, val2.Y - val1.Y, duration));
        }

        public static RectangleF ValueAt(double time, RectangleF val1, RectangleF val2, double startTime, double endTime, EasingTypes easing = EasingTypes.None)
        {
            float current = (float)(time - startTime);
            float duration = (float)(endTime - startTime);

            if (duration == 0 || current == 0)
                return val1;

            return new RectangleF(
                (float)ApplyEasing(easing, current, val1.X, val2.X - val1.X, duration),
                (float)ApplyEasing(easing, current, val1.Y, val2.Y - val1.Y, duration),
                (float)ApplyEasing(easing, current, val1.Width, val2.Width - val1.Width, duration),
                (float)ApplyEasing(easing, current, val1.Height, val2.Height - val1.Height, duration));
        }

        public static double ApplyEasing(EasingTypes easing, double time, double initial, double change, double duration)
        {
            if (change == 0 || time == 0 || duration == 0) return initial;
            if (time == duration) return initial + change;

            switch (easing)
            {
                default:
                    return change * (time / duration) + initial;
                case EasingTypes.In:
                case EasingTypes.InQuad:
                    return change * (time /= duration) * time + initial;
                case EasingTypes.Out:
                case EasingTypes.OutQuad:
                    return -change * (time /= duration) * (time - 2) + initial;
                case EasingTypes.InOutQuad:
                    if ((time /= duration / 2) < 1) return change / 2 * time * time + initial;
                    return -change / 2 * ((--time) * (time - 2) - 1) + initial;
                case EasingTypes.InCubic:
                    return change * (time /= duration) * time * time + initial;
                case EasingTypes.OutCubic:
                    return change * ((time = time / duration - 1) * time * time + 1) + initial;
                case EasingTypes.InOutCubic:
                    if ((time /= duration / 2) < 1) return change / 2 * time * time * time + initial;
                    return change / 2 * ((time -= 2) * time * time + 2) + initial;
                case EasingTypes.InQuart:
                    return change * (time /= duration) * time * time * time + initial;
                case EasingTypes.OutQuart:
                    return -change * ((time = time / duration - 1) * time * time * time - 1) + initial;
                case EasingTypes.InOutQuart:
                    if ((time /= duration / 2) < 1) return change / 2 * time * time * time * time + initial;
                    return -change / 2 * ((time -= 2) * time * time * time - 2) + initial;
                case EasingTypes.InQuint:
                    return change * (time /= duration) * time * time * time * time + initial;
                case EasingTypes.OutQuint:
                    return change * ((time = time / duration - 1) * time * time * time * time + 1) + initial;
                case EasingTypes.InOutQuint:
                    if ((time /= duration / 2) < 1) return change / 2 * time * time * time * time * time + initial;
                    return change / 2 * ((time -= 2) * time * time * time * time + 2) + initial;
                case EasingTypes.InSine:
                    return -change * Math.Cos(time / duration * (MathHelper.Pi / 2)) + change + initial;
                case EasingTypes.OutSine:
                    return change * Math.Sin(time / duration * (MathHelper.Pi / 2)) + initial;
                case EasingTypes.InOutSine:
                    return -change / 2 * (Math.Cos(MathHelper.Pi * time / duration) - 1) + initial;
                case EasingTypes.InExpo:
                    return change * Math.Pow(2, 10 * (time / duration - 1)) + initial;
                case EasingTypes.OutExpo:
                    return (time == duration) ? initial + change : change * (-Math.Pow(2, -10 * time / duration) + 1) + initial;
                case EasingTypes.InOutExpo:
                    if ((time /= duration / 2) < 1) return change / 2 * Math.Pow(2, 10 * (time - 1)) + initial;
                    return change / 2 * (-Math.Pow(2, -10 * --time) + 2) + initial;
                case EasingTypes.InCirc:
                    return -change * (Math.Sqrt(1 - (time /= duration) * time) - 1) + initial;
                case EasingTypes.OutCirc:
                    return change * Math.Sqrt(1 - (time = time / duration - 1) * time) + initial;
                case EasingTypes.InOutCirc:
                    if ((time /= duration / 2) < 1) return -change / 2 * (Math.Sqrt(1 - time * time) - 1) + initial;
                    return change / 2 * (Math.Sqrt(1 - (time -= 2) * time) + 1) + initial;
                case EasingTypes.InElastic:
                    {
                        if ((time /= duration) == 1) return initial + change;

                        var p = duration * .3;
                        var a = change;
                        var s = 1.70158;
                        if (a < Math.Abs(change)) { a = change; s = p / 4; }
                        else s = p / (2 * MathHelper.Pi) * Math.Asin(change / a);
                        return -(a * Math.Pow(2, 10 * (time -= 1)) * Math.Sin((time * duration - s) * (2 * MathHelper.Pi) / p)) + initial;
                    }
                case EasingTypes.OutElastic:
                    {
                        if ((time /= duration) == 1) return initial + change;

                        var p = duration * .3;
                        var a = change;
                        var s = 1.70158;
                        if (a < Math.Abs(change)) { a = change; s = p / 4; }
                        else s = p / (2 * MathHelper.Pi) * Math.Asin(change / a);
                        return a * Math.Pow(2, -10 * time) * Math.Sin((time * duration - s) * (2 * MathHelper.Pi) / p) + change + initial;
                    }
                case EasingTypes.OutElasticHalf:
                    {
                        if ((time /= duration) == 1) return initial + change;

                        var p = duration * .3;
                        var a = change;
                        var s = 1.70158;
                        if (a < Math.Abs(change)) { a = change; s = p / 4; }
                        else s = p / (2 * MathHelper.Pi) * Math.Asin(change / a);
                        return a * Math.Pow(2, -10 * time) * Math.Sin((0.5f * time * duration - s) * (2 * MathHelper.Pi) / p) + change + initial;
                    }
                case EasingTypes.OutElasticQuarter:
                    {
                        if ((time /= duration) == 1) return initial + change;

                        var p = duration * .3;
                        var a = change;
                        var s = 1.70158;
                        if (a < Math.Abs(change)) { a = change; s = p / 4; }
                        else s = p / (2 * MathHelper.Pi) * Math.Asin(change / a);
                        return a * Math.Pow(2, -10 * time) * Math.Sin((0.25f * time * duration - s) * (2 * MathHelper.Pi) / p) + change + initial;
                    }
                case EasingTypes.InOutElastic:
                    {
                        if ((time /= duration / 2) == 2) return initial + change;

                        var p = duration * (.3 * 1.5);
                        var a = change;
                        var s = 1.70158;
                        if (a < Math.Abs(change)) { a = change; s = p / 4; }
                        else s = p / (2 * MathHelper.Pi) * Math.Asin(change / a);
                        if (time < 1) return -.5 * (a * Math.Pow(2, 10 * (time -= 1)) * Math.Sin((time * duration - s) * (2 * MathHelper.Pi) / p)) + initial;
                        return a * Math.Pow(2, -10 * (time -= 1)) * Math.Sin((time * duration - s) * (2 * MathHelper.Pi) / p) * .5 + change + initial;
                    }
                case EasingTypes.InBack:
                    {
                        var s = 1.70158;
                        return change * (time /= duration) * time * ((s + 1) * time - s) + initial;
                    }
                case EasingTypes.OutBack:
                    {
                        var s = 1.70158;
                        return change * ((time = time / duration - 1) * time * ((s + 1) * time + s) + 1) + initial;
                    }
                case EasingTypes.InOutBack:
                    {
                        var s = 1.70158;
                        if ((time /= duration / 2) < 1) return change / 2 * (time * time * (((s *= (1.525)) + 1) * time - s)) + initial;
                        return change / 2 * ((time -= 2) * time * (((s *= (1.525)) + 1) * time + s) + 2) + initial;
                    }
                case EasingTypes.InBounce:
                    return change - ApplyEasing(EasingTypes.OutBounce, duration - time, 0, change, duration) + initial;
                case EasingTypes.OutBounce:
                    if ((time /= duration) < (1 / 2.75))
                    {
                        return change * (7.5625 * time * time) + initial;
                    }
                    if (time < (2 / 2.75))
                    {
                        return change * (7.5625 * (time -= (1.5 / 2.75)) * time + .75) + initial;
                    }
                    if (time < (2.5 / 2.75))
                    {
                        return change * (7.5625 * (time -= (2.25 / 2.75)) * time + .9375) + initial;
                    }
                    return change * (7.5625 * (time -= (2.625 / 2.75)) * time + .984375) + initial;
                case EasingTypes.InOutBounce:
                    if (time < duration / 2) return ApplyEasing(EasingTypes.InBounce, time * 2, 0, change, duration) * .5 + initial;
                    return ApplyEasing(EasingTypes.OutBounce, time * 2 - duration, 0, change, duration) * .5 + change * .5 + initial;
            }
        }
    }
}
