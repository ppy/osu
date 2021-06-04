// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Performance;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    public class HitExplosionEntry : LifetimeEntry
    {
        public readonly float Position;
        public readonly float Scale;
        public readonly Color4 ObjectColour;
        public readonly int RNGSeed;

        public HitExplosionEntry(double startTime, float position, float scale, Color4 objectColour, int rngSeed)
        {
            LifetimeStart = startTime;
            Position = position;
            Scale = scale;
            ObjectColour = objectColour;
            RNGSeed = rngSeed;
        }
    }
}
