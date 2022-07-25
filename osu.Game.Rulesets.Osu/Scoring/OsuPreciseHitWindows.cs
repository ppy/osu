// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public class OsuPreciseHitWindows : HitWindows
    {
        private static readonly DifficultyRange[] osu_ranges =
        {
            new DifficultyRange(HitResult.Great, 85, 55, 25),
            new DifficultyRange(HitResult.Ok, 160, 120, 80),
            new DifficultyRange(HitResult.Meh, 230, 180, 130),
            new DifficultyRange(HitResult.Miss, 400, 400, 400),
        };

        public override bool IsHitResultAllowed(HitResult result)
        {
            switch (result)
            {
                case HitResult.Great:
                case HitResult.Ok:
                case HitResult.Meh:
                case HitResult.Miss:
                    return true;
            }

            return false;
        }

        protected override DifficultyRange[] GetRanges() => osu_ranges;
    }
}
