// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    [JsonObject(MemberSerialization.OptIn)]
    public struct TaikoDifficultyAttributes : IDifficultyAttributes
    {
        /// <inheritdoc/>
        public double StarRating { get; set; }

        /// <inheritdoc/>
        public int MaxCombo { get; set; }

        /// <summary>
        /// The difficulty corresponding to the stamina skill.
        /// </summary>
        [JsonProperty("stamina_difficulty")]
        public double StaminaDifficulty { get; set; }

        /// <summary>
        /// The ratio of stamina difficulty from mono-color (single colour) streams to total stamina difficulty.
        /// </summary>
        [JsonProperty("mono_stamina_factor")]
        public double MonoStaminaFactor { get; set; }

        /// <summary>
        /// The difficulty corresponding to the rhythm skill.
        /// </summary>
        [JsonProperty("rhythm_difficulty")]
        public double RhythmDifficulty { get; set; }

        /// <summary>
        /// The difficulty corresponding to the colour skill.
        /// </summary>
        [JsonProperty("colour_difficulty")]
        public double ColourDifficulty { get; set; }

        /// <summary>
        /// The difficulty corresponding to the hardest parts of the map.
        /// </summary>
        [JsonProperty("peak_difficulty")]
        public double PeakDifficulty { get; set; }

        /// <summary>
        /// The perceived hit window for a GREAT hit inclusive of rate-adjusting mods (DT/HT/etc).
        /// </summary>
        /// <remarks>
        /// Rate-adjusting mods don't directly affect the hit window, but have a perceived effect as a result of adjusting audio timing.
        /// </remarks>
        [JsonProperty("great_hit_window")]
        public double GreatHitWindow { get; set; }

        /// <summary>
        /// The perceived hit window for an OK hit inclusive of rate-adjusting mods (DT/HT/etc).
        /// </summary>
        /// <remarks>
        /// Rate-adjusting mods don't directly affect the hit window, but have a perceived effect as a result of adjusting audio timing.
        /// </remarks>
        [JsonProperty("ok_hit_window")]
        public double OkHitWindow { get; set; }

        public IEnumerable<(int attributeId, object value)> ToDatabaseAttributes()
        {
            yield return (IDifficultyAttributes.ATTRIB_ID_MAX_COMBO, MaxCombo);
            yield return (IDifficultyAttributes.ATTRIB_ID_DIFFICULTY, StarRating);
            yield return (IDifficultyAttributes.ATTRIB_ID_GREAT_HIT_WINDOW, GreatHitWindow);
            yield return (IDifficultyAttributes.ATTRIB_ID_OK_HIT_WINDOW, OkHitWindow);
            yield return (IDifficultyAttributes.ATTRIB_ID_MONO_STAMINA_FACTOR, MonoStaminaFactor);
        }

        public void FromDatabaseAttributes(IReadOnlyDictionary<int, double> values, IBeatmapOnlineInfo onlineInfo)
        {
            MaxCombo = (int)values[IDifficultyAttributes.ATTRIB_ID_MAX_COMBO];
            StarRating = values[IDifficultyAttributes.ATTRIB_ID_DIFFICULTY];
            GreatHitWindow = values[IDifficultyAttributes.ATTRIB_ID_GREAT_HIT_WINDOW];
            OkHitWindow = values[IDifficultyAttributes.ATTRIB_ID_OK_HIT_WINDOW];
            MonoStaminaFactor = values[IDifficultyAttributes.ATTRIB_ID_MONO_STAMINA_FACTOR];
        }
    }
}
