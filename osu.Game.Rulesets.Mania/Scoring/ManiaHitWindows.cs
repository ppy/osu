// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Scoring
{
    public class ManiaHitWindows : HitWindows
    {
        internal static readonly HitWindowRange[] MANIA_RANGES =
        {
            new HitWindowRange(HitResult.Perfect, 22.4D, 19.4D, 13.9D),
            new HitWindowRange(HitResult.Great, 64, 49, 34),
            new HitWindowRange(HitResult.Good, 97, 82, 67),
            new HitWindowRange(HitResult.Ok, 127, 112, 97),
            new HitWindowRange(HitResult.Meh, 151, 136, 121),
            new HitWindowRange(HitResult.Miss, 188, 173, 158),
        };

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

        protected override HitWindowRange[] GetRanges() => MANIA_RANGES.Select(r =>
            new HitWindowRange(
                r.Result,
                r.Min * multiplier,
                r.Average * multiplier,
                r.Max * multiplier)).ToArray();

        protected override bool LegacyIsInclusive => true;
    }
}
