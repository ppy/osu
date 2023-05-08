﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Scoring
{
    public partial class CatchScoreProcessor : ScoreProcessor
    {
        public CatchScoreProcessor()
            : base(new CatchRuleset())
        {
        }

        protected override double ClassicScoreMultiplier => 28;
    }
}
