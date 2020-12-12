// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Replays.Movers.Spinners
{
    public class SimpleSpinnerMover : BaseDanceObjectMover<Spinner>
    {
        private int idx;

        public override Vector2 Update(double time)
        {
            var c = OsuPlayfield.BASE_SIZE / 2;
            if (++idx >= 4) idx = 0;

            return idx switch
            {
                0 => c + new Vector2(-1, -1),
                1 => c + new Vector2(1, -1),
                2 => c + new Vector2(1, 1),
                _ => c + new Vector2(-1, 1),
            };
        }
    }
}
