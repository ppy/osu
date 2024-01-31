// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public class OsuHitWindows : HitWindows
    {
        /// <summary>
        /// osu! ruleset has a fixed miss window regardless of difficulty settings.
        /// </summary>
        public const double MISS_WINDOW = 400;

        internal static readonly HitWindowRange[] OSU_RANGES =
        {
            new HitWindowRange(HitResult.Great, 80, 50, 20),
            new HitWindowRange(HitResult.Ok, 140, 100, 60),
            new HitWindowRange(HitResult.Meh, 200, 150, 100),
            new HitWindowRange(HitResult.Miss, MISS_WINDOW, MISS_WINDOW, MISS_WINDOW),
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

        protected override HitWindowRange[] GetRanges() => OSU_RANGES;
    }
}
