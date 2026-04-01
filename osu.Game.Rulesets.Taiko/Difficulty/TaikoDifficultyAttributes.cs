// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    public class TaikoDifficultyAttributes : DifficultyAttributes
    {
        /// <summary>
        /// The combined star rating of all skills.
        /// </summary>
        [JsonProperty("star_rating_classic", Order = -2)]
        public double StarRatingClassic { get; set; }

        /// <summary>
        /// The difficulty corresponding to the mechanical skills in osu!taiko.
        /// This includes colour and stamina combined.
        /// </summary>
        public double MechanicalDifficulty { get; set; }

        /// <summary>
        /// The difficulty corresponding to the rhythm skill.
        /// </summary>
        [JsonProperty("rhythm_difficulty")]
        public double RhythmDifficulty { get; set; }

        /// <summary>
        /// The difficulty corresponding to the reading skill.
        /// </summary>
        public double ReadingDifficulty { get; set; }

        /// <summary>
        /// The difficulty corresponding to the reading skill with the classic mod enabled.
        /// </summary>
        public double ReadingDifficultyClassic { get; set; }

        /// <summary>
        /// The difficulty corresponding to the colour skill.
        /// </summary>
        public double ColourDifficulty { get; set; }

        /// <summary>
        /// The difficulty corresponding to the stamina skill.
        /// </summary>
        public double StaminaDifficulty { get; set; }

        /// <summary>
        /// The ratio of stamina difficulty from mono-color (single colour) streams to total stamina difficulty.
        /// </summary>
        [JsonProperty("mono_stamina_factor")]
        public double MonoStaminaFactor { get; set; }

        /// <summary>
        /// The factor corresponding to the consistency of a map.
        /// </summary>
        [JsonProperty("consistency_factor")]
        public double ConsistencyFactor { get; set; }

        /// <summary>
        /// The factor corresponding to the consistency of a map with the classic mod enabled.
        /// </summary>
        [JsonProperty("consistency_factor_classic")]
        public double ConsistencyFactorClassic { get; set; }

        public double StaminaTopStrains { get; set; }

        public override IEnumerable<(int attributeId, object value)> ToDatabaseAttributes()
        {
            foreach (var v in base.ToDatabaseAttributes())
                yield return v;

            yield return (ATTRIB_ID_DIFFICULTY, StarRating);
            yield return (ATTRIB_ID_RHYTHM_DIFFICULTY, RhythmDifficulty);
            yield return (ATTRIB_ID_MONO_STAMINA_FACTOR, MonoStaminaFactor);
            yield return (ATTRIB_ID_CONSISTENCY_FACTOR, ConsistencyFactor);
        }

        public override void FromDatabaseAttributes(IReadOnlyDictionary<int, double> values, IBeatmapOnlineInfo onlineInfo)
        {
            base.FromDatabaseAttributes(values, onlineInfo);

            StarRating = values[ATTRIB_ID_DIFFICULTY];
            RhythmDifficulty = values[ATTRIB_ID_RHYTHM_DIFFICULTY];
            MonoStaminaFactor = values[ATTRIB_ID_MONO_STAMINA_FACTOR];
            ConsistencyFactor = values[ATTRIB_ID_CONSISTENCY_FACTOR];
        }
    }
}
