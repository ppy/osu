// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;

namespace osu.Game.Rulesets.Osu.Replays.Movers
{
    public class LinearMover : BaseDanceMover
    {
        public override Vector2 Update(double time)
        {
            var t = T(time);

            return new Vector2(
                StartX + (EndX - StartX) * t,
                StartY + (EndY - StartY) * t
            );
        }
    }
}
