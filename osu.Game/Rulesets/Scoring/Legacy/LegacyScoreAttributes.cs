// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Scoring.Legacy
{
    public struct LegacyScoreAttributes
    {
        /// <summary>
        /// The accuracy portion of the legacy (ScoreV1) total score.
        /// </summary>
        public int AccuracyScore;

        /// <summary>
        /// The combo-multiplied portion of the legacy (ScoreV1) total score.
        /// </summary>
        public long ComboScore;

        /// <summary>
        /// A ratio of standardised score to legacy score for the bonus part of total score.
        /// </summary>
        public double BonusScoreRatio;
    }
}
