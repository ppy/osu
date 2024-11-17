// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    public interface ITaikoDifficultyAttributes
    {
        /// <summary>
        /// The difficulty corresponding to the stamina skill.
        /// </summary>
        public double StaminaDifficulty { get; set; }

        /// <summary>
        /// The ratio of stamina difficulty from mono-color (single colour) streams to total stamina difficulty.
        /// </summary>
        public double MonoStaminaFactor { get; set; }

        /// <summary>
        /// The difficulty corresponding to the rhythm skill.
        /// </summary>
        public double RhythmDifficulty { get; set; }

        /// <summary>
        /// The difficulty corresponding to the colour skill.
        /// </summary>
        public double ColourDifficulty { get; set; }

        /// <summary>
        /// The difficulty corresponding to the hardest parts of the map.
        /// </summary>
        public double PeakDifficulty { get; set; }

        /// <summary>
        /// The perceived hit window for a GREAT hit inclusive of rate-adjusting mods (DT/HT/etc).
        /// </summary>
        /// <remarks>
        /// Rate-adjusting mods don't directly affect the hit window, but have a perceived effect as a result of adjusting audio timing.
        /// </remarks>
        public double GreatHitWindow { get; set; }

        /// <summary>
        /// The perceived hit window for an OK hit inclusive of rate-adjusting mods (DT/HT/etc).
        /// </summary>
        /// <remarks>
        /// Rate-adjusting mods don't directly affect the hit window, but have a perceived effect as a result of adjusting audio timing.
        /// </remarks>
        public double OkHitWindow { get; set; }
    }

    public class TaikoDifficultyAttributes : DifficultyAttributes, ITaikoDifficultyAttributes
    {
        /// <inheritdoc/>
        [JsonProperty("stamina_difficulty")]
        public double StaminaDifficulty { get; set; }

        /// <inheritdoc/>
        [JsonProperty("mono_stamina_factor")]
        public double MonoStaminaFactor { get; set; }

        /// <inheritdoc/>
        [JsonProperty("rhythm_difficulty")]
        public double RhythmDifficulty { get; set; }

        /// <inheritdoc/>
        [JsonProperty("colour_difficulty")]
        public double ColourDifficulty { get; set; }

        /// <inheritdoc/>
        [JsonProperty("peak_difficulty")]
        public double PeakDifficulty { get; set; }

        /// <inheritdoc/>
        [JsonProperty("great_hit_window")]
        public double GreatHitWindow { get; set; }

        /// <inheritdoc/>
        [JsonProperty("ok_hit_window")]
        public double OkHitWindow { get; set; }

        public override IEnumerable<(int attributeId, object value)> ToDatabaseAttributes()
        {
            foreach (var v in base.ToDatabaseAttributes())
                yield return v;

            yield return (ATTRIB_ID_DIFFICULTY, StarRating);
            yield return (ATTRIB_ID_GREAT_HIT_WINDOW, GreatHitWindow);
            yield return (ATTRIB_ID_OK_HIT_WINDOW, OkHitWindow);
            yield return (ATTRIB_ID_MONO_STAMINA_FACTOR, MonoStaminaFactor);
        }

        public override void FromDatabaseAttributes(IReadOnlyDictionary<int, double> values, IBeatmapOnlineInfo onlineInfo)
        {
            base.FromDatabaseAttributes(values, onlineInfo);

            StarRating = values[ATTRIB_ID_DIFFICULTY];
            GreatHitWindow = values[ATTRIB_ID_GREAT_HIT_WINDOW];
            OkHitWindow = values[ATTRIB_ID_OK_HIT_WINDOW];
            MonoStaminaFactor = values[ATTRIB_ID_MONO_STAMINA_FACTOR];
        }
    }
}
