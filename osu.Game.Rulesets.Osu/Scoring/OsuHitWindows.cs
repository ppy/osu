// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public class OsuHitWindows : HitWindows.LegacyHitWindows
    {
        /// <summary>
        /// osu! ruleset has a fixed miss window regardless of difficulty settings.
        /// </summary>
        public const double MISS_WINDOW = 400;

        internal static readonly DifficultyRange[] OSU_RANGES =
        {
            new DifficultyRange(HitResult.Great, 80, 50, 20),
            new DifficultyRange(HitResult.Ok, 140, 100, 60),
            new DifficultyRange(HitResult.Meh, 200, 150, 100),
            new DifficultyRange(HitResult.Miss, MISS_WINDOW, MISS_WINDOW, MISS_WINDOW),
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

        protected override DifficultyRange[] GetRanges() => OSU_RANGES;
    }
}
