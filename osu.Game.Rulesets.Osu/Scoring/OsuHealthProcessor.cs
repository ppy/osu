// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public partial class OsuHealthProcessor : DrainingHealthProcessor
    {
        public OsuHealthProcessor(double drainStartTime, double drainLenience = 0)
            : base(drainStartTime, drainLenience)
        {
        }

        protected override double GetHealthIncreaseFor(JudgementResult result)
        {
            switch (result.Type)
            {
                case HitResult.SmallTickMiss:
                    return IBeatmapDifficultyInfo.DifficultyRange(Beatmap.Difficulty.DrainRate, -0.02, -0.075, -0.14);

                case HitResult.LargeTickMiss:
                    return IBeatmapDifficultyInfo.DifficultyRange(Beatmap.Difficulty.DrainRate, -0.02, -0.075, -0.14);

                case HitResult.Miss:
                    return IBeatmapDifficultyInfo.DifficultyRange(Beatmap.Difficulty.DrainRate, -0.03, -0.125, -0.2);

                case HitResult.SmallTickHit:
                    // When classic slider mechanics are enabled, this result comes from the tail.
                    return 0.02;

                case HitResult.LargeTickHit:
                    switch (result.HitObject)
                    {
                        case SliderTick:
                            return 0.015;

                        case SliderHeadCircle:
                        case SliderTailCircle:
                        case SliderRepeat:
                            return 0.02;
                    }

                    break;

                case HitResult.Meh:
                    return 0.002;

                case HitResult.Ok:
                    return 0.011;

                case HitResult.Great:
                    return 0.03;

                case HitResult.SmallBonus:
                    return 0.0085;

                case HitResult.LargeBonus:
                    return 0.01;
            }

            return base.GetHealthIncreaseFor(result);
        }
    }
}
