// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using MessagePack;
using Newtonsoft.Json;
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
        [JsonProperty("base_score")]
        public double BaseScore;

        /// <summary>
        /// The sum of all "bonus" <see cref="HitObject"/> scoring values. See: <see cref="HitResultExtensions.IsBonus"/> and <see cref="Judgement.ToNumericResult"/>.
        /// </summary>
        [Key(1)]
        [JsonProperty("bonus_score")]
        public double BonusScore;

        /// <summary>
        /// The highest achieved combo.
        /// </summary>
        [Key(2)]
        [JsonProperty("max_combo")]
        public int MaxCombo;

        /// <summary>
        /// The count of <see cref="HitObject"/>s awarding "basic" hit results.
        /// </summary>
        /// <seealso cref="HitResultExtensions.IsBasic"/>
        [Key(3)]
        [JsonProperty("basic_objects_count")]
        public int BasicObjectsCount;

        /// <summary>
        /// The count of <see cref="HitObject"/>s awarding "large tick" hit results.
        /// </summary>
        /// <seealso cref="HitResultExtensions.IsTick"/>
        [Key(4)]
        [JsonProperty("large_ticks_count")]
        public int LargeTicksCount;

        /// <summary>
        /// The count of <see cref="HitObject"/>s awarding "small tick" hit results.
        /// </summary>
        /// <seealso cref="HitResultExtensions.IsTick"/>
        [Key(5)]
        [JsonProperty("small_ticks_count")]
        public int SmallTicksCount;
    }
}
