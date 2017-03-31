// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Catch.Judgements;
using osu.Game.Modes.Catch.Objects;
using osu.Game.Modes.Scoring;
using osu.Game.Modes.UI;

namespace osu.Game.Modes.Catch.Scoring
{
    internal class CatchScoreProcessor : ScoreProcessor<CatchBaseHit, CatchJudgement>
    {
        public CatchScoreProcessor()
        {
        }

        public CatchScoreProcessor(HitRenderer<CatchBaseHit, CatchJudgement> hitRenderer)
            : base(hitRenderer)
        {
        }

        protected override void OnNewJudgement(CatchJudgement judgement)
        {
        }
    }
}
