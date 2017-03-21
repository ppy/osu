// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Taiko.Judgements;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.UI;

namespace osu.Game.Modes.Taiko
{
    internal class TaikoScoreProcessor : ScoreProcessor<TaikoHitObject, TaikoJudgementInfo>
    {
        public TaikoScoreProcessor()
        {
        }

        public TaikoScoreProcessor(HitRenderer<TaikoHitObject, TaikoJudgementInfo> hitRenderer)
            : base(hitRenderer)
        {
        }

        protected override void UpdateCalculations(TaikoJudgementInfo newJudgement)
        {
        }
    }
}
