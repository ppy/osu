// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Scoring
{
    internal class CatchScoreProcessor : ScoreProcessor<CatchBaseHit>
    {
        public CatchScoreProcessor()
        {
        }

        public CatchScoreProcessor(RulesetContainer<CatchBaseHit> rulesetContainer)
            : base(rulesetContainer)
        {
        }
    }
}
