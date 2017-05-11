// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.Scoring
{
    internal class ManiaScoreProcessor : ScoreProcessor<ManiaHitObject, ManiaJudgement>
    {
        public ManiaScoreProcessor()
        {
        }

        public ManiaScoreProcessor(HitRenderer<ManiaHitObject, ManiaJudgement> hitRenderer)
            : base(hitRenderer)
        {
        }

        protected override void OnNewJudgement(ManiaJudgement judgement)
        {
        }
    }
}
