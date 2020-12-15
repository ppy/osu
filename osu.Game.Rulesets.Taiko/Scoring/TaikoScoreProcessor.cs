// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Scoring
{
    internal class TaikoScoreProcessor : ScoreProcessor
    {
        protected override double DefaultAccuracyPortion => 0.75;

        protected override double DefaultComboPortion => 0.25;
    }
}
