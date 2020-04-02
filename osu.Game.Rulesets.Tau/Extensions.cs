// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;

namespace osu.Game.Rulesets.Tau
{
    public static class Extensions
    {
        public static float GetDegreesFromPosition(this Vector2 target, Vector2 self)
        {
            Vector2 offset = self - target;
            float degrees = (float)MathHelper.RadiansToDegrees(Math.Atan2(-offset.X, offset.Y)) + 24.3f;

            return degrees;
        }

        public static float GetHitObjectAngle(this Vector2 target, Vector2 self) => target.GetDegreesFromPosition(self) * 4;
    }
}
