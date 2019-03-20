// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Scoring
{
    public class CatchScoreProcessor : ScoreProcessor<CatchHitObject>
    {
        public CatchScoreProcessor(DrawableRuleset<CatchHitObject> drawableRuleset)
            : base(drawableRuleset)
        {
        }

        private float hpDrainRate;

        protected override void ApplyBeatmap(Beatmap<CatchHitObject> beatmap)
        {
            base.ApplyBeatmap(beatmap);

            hpDrainRate = beatmap.BeatmapInfo.BaseDifficulty.DrainRate;
        }

        private const double harshness = 0.01;

        protected override void ApplyResult(JudgementResult result)
        {
            base.ApplyResult(result);

            if (result.Type == HitResult.Miss)
            {
                if (!result.Judgement.IsBonus)
                    Health.Value -= hpDrainRate * (harshness * 2);
                return;
            }

            Health.Value += Math.Max(result.Judgement.HealthIncreaseFor(result) - hpDrainRate, 0) * harshness;
        }

        public override HitWindows CreateHitWindows() => new CatchHitWindows();
    }
}
