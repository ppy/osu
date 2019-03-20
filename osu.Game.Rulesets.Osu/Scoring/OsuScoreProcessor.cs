// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Extensions;
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

        private readonly Dictionary<ComboResult, int> comboResultCounts = new Dictionary<ComboResult, int>();

        protected override void ApplyBeatmap(Beatmap<OsuHitObject> beatmap)
        {
            base.ApplyBeatmap(beatmap);

            hpDrainRate = beatmap.BeatmapInfo.BaseDifficulty.DrainRate;
        }

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);
            comboResultCounts.Clear();
        }

        private const double harshness = 0.01;

        protected override void ApplyResult(JudgementResult result)
        {
            base.ApplyResult(result);

            var osuResult = (OsuJudgementResult)result;

            if (result.Type != HitResult.None)
                comboResultCounts[osuResult.ComboType] = comboResultCounts.GetOrDefault(osuResult.ComboType) + 1;

            switch (result.Type)
            {
                case HitResult.Great:
                    Health.Value += (10.2 - hpDrainRate) * harshness;
                    break;

                case HitResult.Good:
                    Health.Value += (8 - hpDrainRate) * harshness;
                    break;

                case HitResult.Meh:
                    Health.Value += (4 - hpDrainRate) * harshness;
                    break;

                /*case HitResult.SliderTick:
                    Health.Value += Math.Max(7 - hpDrainRate, 0) * 0.01;
                    break;*/

                case HitResult.Miss:
                    Health.Value -= hpDrainRate * (harshness * 2);
                    break;
            }
        }

        protected override JudgementResult CreateResult(Judgement judgement) => new OsuJudgementResult(judgement);

        public override HitWindows CreateHitWindows() => new OsuHitWindows();
    }
}
