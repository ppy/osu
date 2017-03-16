// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Mania.Judgements;
using osu.Game.Modes.Mania.Objects;
using osu.Game.Modes.UI;

namespace osu.Game.Modes.Mania
{
    internal class ManiaScoreProcessor : ScoreProcessor<ManiaBaseHit, ManiaJudgementInfo>
    {
        public ManiaScoreProcessor()
        {
        }

        public ManiaScoreProcessor(HitRenderer<ManiaBaseHit, ManiaJudgementInfo> hitRenderer)
            : base(hitRenderer)
        {
        }

        protected override void UpdateCalculations(ManiaJudgementInfo newJudgement)
        {
        }
    }
}
