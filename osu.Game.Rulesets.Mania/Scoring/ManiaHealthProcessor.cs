// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Scoring
{
    public partial class ManiaHealthProcessor : LegacyDrainingHealthProcessor
    {
        public ManiaHealthProcessor(double drainStartTime)
            : base(drainStartTime)
        {
        }

        protected override double ComputeDrainRate()
        {
            // Base call is run only to compute HP recovery (namely, `HpMultiplierNormal`).
            // This closely mirrors (broken) behaviour of stable and as such is preserved unchanged.
            base.ComputeDrainRate();

            return 0;
        }

        protected override IEnumerable<HitObject> EnumerateTopLevelHitObjects() => Beatmap.HitObjects;

        protected override IEnumerable<HitObject> EnumerateNestedHitObjects(HitObject hitObject) => hitObject.NestedHitObjects;

        protected override double GetHealthIncreaseFor(HitObject hitObject, HitResult result)
        {
            double increase = 0;

            switch (result)
            {
                case HitResult.Miss:
                    switch (hitObject)
                    {
                        case HeadNote:
                        case TailNote:
                            return -(Beatmap.Difficulty.DrainRate + 1) * 0.00375;

                        default:
                            return -(Beatmap.Difficulty.DrainRate + 1) * 0.0075;
                    }

                case HitResult.Meh:
                    return -(Beatmap.Difficulty.DrainRate + 1) * 0.0016;

                case HitResult.Ok:
                    return 0;

                case HitResult.Good:
                    increase = 0.004 - Beatmap.Difficulty.DrainRate * 0.0004;
                    break;

                case HitResult.Great:
                    increase = 0.005 - Beatmap.Difficulty.DrainRate * 0.0005;
                    break;

                case HitResult.Perfect:
                    increase = 0.0055 - Beatmap.Difficulty.DrainRate * 0.0005;
                    break;
            }

            return HpMultiplierNormal * increase;
        }
    }
}
