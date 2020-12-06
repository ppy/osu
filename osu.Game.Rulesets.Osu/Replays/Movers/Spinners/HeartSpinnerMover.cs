// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osuTK;
using static System.MathF;

namespace osu.Game.Rulesets.Osu.Replays.Movers.Spinners
{
    public class HeartSpinnerMover : BaseDanceObjectMover<Spinner>
    {
        private const float inc = 0.15f;
        private float t;

        public override Vector2 Update(double time)
        {
            t += inc;

            var c = OsuPlayfield.BASE_SIZE / 2;
            var st = Sin(t);

            var h = new Vector2(
                16 * st * st * st,
                13 * Cos(t) - 5 * Cos(2 * t) - 2 * Cos(3 * t) - Cos(4 * t)
            );

            return h * -7.5f + c;
        }
    }
}
