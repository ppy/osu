// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osuTK.Graphics;

namespace osu.Game.Utils
{
    public static class LegacyUtils
    {
        public static Color4 InterpolateNonLinear(double time, Color4 startColour, Color4 endColour, double startTime, double endTime, Easing easing = Easing.None)
            => InterpolateNonLinear(time, startColour, endColour, startTime, endTime, new DefaultEasingFunction(easing));

        public static Colour4 InterpolateNonLinear(double time, Colour4 startColour, Colour4 endColour, double startTime, double endTime, Easing easing = Easing.None)
            => InterpolateNonLinear(time, startColour, endColour, startTime, endTime, new DefaultEasingFunction(easing));

        /// <summary>
        /// Interpolates between two sRGB <see cref="Color4"/>s directly in sRGB space.
        /// </summary>
        public static Color4 InterpolateNonLinear<TEasing>(double time, Color4 startColour, Color4 endColour, double startTime, double endTime, TEasing easing) where TEasing : IEasingFunction
        {
            if (startColour == endColour)
                return startColour;

            double current = time - startTime;
            double duration = endTime - startTime;

            if (duration == 0 || current == 0)
                return startColour;

            float t = Math.Max(0, Math.Min(1, (float)easing.ApplyEasing(current / duration)));

            return new Color4(
                startColour.R + t * (endColour.R - startColour.R),
                startColour.G + t * (endColour.G - startColour.G),
                startColour.B + t * (endColour.B - startColour.B),
                startColour.A + t * (endColour.A - startColour.A));
        }

        /// <summary>
        /// Interpolates between two sRGB <see cref="Colour4"/>s directly in sRGB space.
        /// </summary>
        public static Colour4 InterpolateNonLinear<TEasing>(double time, Colour4 startColour, Colour4 endColour, double startTime, double endTime, TEasing easing) where TEasing : IEasingFunction
        {
            if (startColour == endColour)
                return startColour;

            double current = time - startTime;
            double duration = endTime - startTime;

            if (duration == 0 || current == 0)
                return startColour;

            float t = Math.Max(0, Math.Min(1, (float)easing.ApplyEasing(current / duration)));

            return new Colour4(
                startColour.R + t * (endColour.R - startColour.R),
                startColour.G + t * (endColour.G - startColour.G),
                startColour.B + t * (endColour.B - startColour.B),
                startColour.A + t * (endColour.A - startColour.A));
        }
    }
}
