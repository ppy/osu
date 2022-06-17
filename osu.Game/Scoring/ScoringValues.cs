// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using MessagePack;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Scoring
{
    /// <summary>
    /// Stores the required scoring data that fulfils the minimum requirements for a <see cref="ScoreProcessor"/> to calculate score.
    /// </summary>
    [MessagePackObject]
    public struct ScoringValues
    {
        /// <summary>
        /// The sum of all "basic" <see cref="HitObject"/> scoring values. See: <see cref="HitResultExtensions.IsBasic"/> and <see cref="Judgement.ToNumericResult"/>.
        /// </summary>
        [Key(0)]
        public double BaseScore;

        /// <summary>
        /// The sum of all "bonus" <see cref="HitObject"/> scoring values. See: <see cref="HitResultExtensions.IsBonus"/> and <see cref="Judgement.ToNumericResult"/>.
        /// </summary>
        [Key(1)]
        public double BonusScore;

        /// <summary>
        /// The highest achieved combo.
        /// </summary>
        [Key(2)]
        public int MaxCombo;

        /// <summary>
        /// The count of "basic" <see cref="HitObject"/>s. See: <see cref="HitResultExtensions.IsBasic"/>.
        /// </summary>
        [Key(3)]
        public int CountBasicHitObjects;
    }
}
