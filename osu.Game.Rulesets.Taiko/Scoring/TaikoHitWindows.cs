// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Scoring
{
    public class TaikoHitWindows : HitWindows
    {
        internal static readonly HitWindowRange[] TAIKO_RANGES =
        {
            new HitWindowRange(HitResult.Great, 50, 35, 20),
            new HitWindowRange(HitResult.Ok, 120, 80, 50),
            new HitWindowRange(HitResult.Miss, 135, 95, 70),
        };

        public override bool IsHitResultAllowed(HitResult result)
        {
            switch (result)
            {
                case HitResult.Great:
                case HitResult.Ok:
                case HitResult.Miss:
                    return true;
            }

            return false;
        }

        protected override HitWindowRange[] GetRanges() => TAIKO_RANGES;
    }
}
