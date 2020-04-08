// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using Vector2 = osuTK.Vector2;

namespace osu.Game.Rulesets.Tau
{
    public static class Extensions
    {
        public static float GetDegreesFromPosition(this Vector2 target, Vector2 self)
        {
            Vector2 offset = self - target;
            return (float)MathHelper.RadiansToDegrees(Math.Atan2(-offset.X, offset.Y));
        }

        public static float GetHitObjectAngle(this Vector2 target)
        {
            Vector2 offset = new Vector2(256, 192) - target; // Using centre of playfield.
            return (float)MathHelper.RadiansToDegrees(Math.Atan2(offset.X, -offset.Y)) - 90;
        }
    }
}
