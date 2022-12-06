// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Scoring
{
    internal partial class TaikoScoreProcessor : ScoreProcessor
    {
        public TaikoScoreProcessor()
            : base(new TaikoRuleset())
        {
        }

        protected override double DefaultAccuracyPortion => 0.75;

        protected override double DefaultComboPortion => 0.25;

        protected override double ClassicScoreMultiplier => 22;
    }
}
