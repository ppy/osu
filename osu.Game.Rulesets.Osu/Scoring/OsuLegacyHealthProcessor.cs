// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public partial class OsuLegacyHealthProcessor : LegacyDrainingHealthProcessor
    {
        public OsuLegacyHealthProcessor(double drainStartTime)
            : base(drainStartTime)
        {
        }

        protected override IEnumerable<HitObject> EnumerateTopLevelHitObjects() => Beatmap.HitObjects;

        protected override IEnumerable<HitObject> EnumerateNestedHitObjects(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Slider slider:
                    foreach (var nested in slider.NestedHitObjects)
                        yield return nested;

                    break;

                case Spinner spinner:
                    foreach (var nested in spinner.NestedHitObjects.Where(t => t is not SpinnerBonusTick))
                        yield return nested;

                    break;
            }
        }

        protected override double GetHealthIncreaseFor(HitObject hitObject, HitResult result)
        {
            double increase = 0;

            switch (result)
            {
                case HitResult.SmallTickMiss:
                    return IBeatmapDifficultyInfo.DifficultyRange(Beatmap.Difficulty.DrainRate, -0.02, -0.075, -0.14);

                case HitResult.LargeTickMiss:
                    return IBeatmapDifficultyInfo.DifficultyRange(Beatmap.Difficulty.DrainRate, -0.02, -0.075, -0.14);

                case HitResult.Miss:
                    return IBeatmapDifficultyInfo.DifficultyRange(Beatmap.Difficulty.DrainRate, -0.03, -0.125, -0.2);

                case HitResult.SmallTickHit:
                    // This result always comes from the slider tail, which is judged the same as a repeat.
                    increase = 0.02;
                    break;

                case HitResult.SliderTailHit:
                case HitResult.LargeTickHit:
                    // This result comes from either a slider tick or repeat.
                    increase = hitObject is SliderTick ? 0.015 : 0.02;
                    break;

                case HitResult.Meh:
                    increase = 0.002;
                    break;

                case HitResult.Ok:
                    increase = 0.011;
                    break;

                case HitResult.Great:
                    increase = 0.03;
                    break;

                case HitResult.SmallBonus:
                    increase = 0.0085;
                    break;

                case HitResult.LargeBonus:
                    increase = 0.01;
                    break;

                case HitResult.HealthBonus:
                    increase = 0.05;
                    break;
            }

            return HpMultiplierNormal * increase;
        }
    }
}
