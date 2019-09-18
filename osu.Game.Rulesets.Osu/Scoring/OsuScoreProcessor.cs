// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Scoring
{
    internal class OsuScoreProcessor : ScoreProcessor<OsuHitObject>
    {
        public OsuScoreProcessor(DrawableRuleset<OsuHitObject> drawableRuleset)
            : base(drawableRuleset)
        {
        }

        private float hpDrainRate;

        protected override void ApplyBeatmap(Beatmap<OsuHitObject> beatmap)
        {
            base.ApplyBeatmap(beatmap);

            hpDrainRate = beatmap.BeatmapInfo.BaseDifficulty.DrainRate;
        }

        protected override double HealthAdjustmentFactorFor(JudgementResult result)
        {
            switch (result.Type)
            {
                case HitResult.Great:
                    return 10.2 - hpDrainRate;

                case HitResult.Good:
                    return 8 - hpDrainRate;

                case HitResult.Meh:
                    return 4 - hpDrainRate;

                // case HitResult.SliderTick:
                //     return Math.Max(7 - hpDrainRate, 0) * 0.01;

                case HitResult.Miss:
                    return hpDrainRate;

                default:
                    return 0;
            }
        }

        protected override JudgementResult CreateResult(HitObject hitObject, Judgement judgement) => new OsuJudgementResult(hitObject, judgement);

        public override HitWindows CreateHitWindows() => new OsuHitWindows();
    }
}
