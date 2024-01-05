// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Scoring
{
    public class ManiaHitWindows : HitWindows
    {
        private readonly double multiplier;

        public ManiaHitWindows()
            : this(1)
        {
        }

        public ManiaHitWindows(double multiplier)
        {
            this.multiplier = multiplier;
        }

        public override bool IsHitResultAllowed(HitResult result)
        {
            switch (result)
            {
                case HitResult.Perfect:
                case HitResult.Great:
                case HitResult.Good:
                case HitResult.Ok:
                case HitResult.Meh:
                case HitResult.Miss:
                    return true;
            }

            return false;
        }

        protected override DifficultyRange[] GetRanges() => base.GetRanges().Select(r =>
            new DifficultyRange(
                r.Result,
                r.Min * multiplier,
                r.Average * multiplier,
                r.Max * multiplier)).ToArray();
    }
}
