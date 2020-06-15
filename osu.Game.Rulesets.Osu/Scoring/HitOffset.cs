// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public class HitOffset
    {
        public readonly Vector2 Position1;
        public readonly Vector2 Position2;
        public readonly Vector2 HitPosition;
        public readonly float Radius;

        public HitOffset(Vector2 position1, Vector2 position2, Vector2 hitPosition, float radius)
        {
            Position1 = position1;
            Position2 = position2;
            HitPosition = hitPosition;
            Radius = radius;
        }
    }
}
