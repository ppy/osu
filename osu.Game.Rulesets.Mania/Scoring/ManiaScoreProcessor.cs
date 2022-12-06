// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Scoring
{
    internal partial class ManiaScoreProcessor : ScoreProcessor
    {
        public ManiaScoreProcessor()
            : base(new ManiaRuleset())
        {
        }

        protected override double DefaultAccuracyPortion => 0.99;

        protected override double DefaultComboPortion => 0.01;

        protected override double ClassicScoreMultiplier => 16;
    }
}
