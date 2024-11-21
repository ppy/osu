// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    [JsonObject(MemberSerialization.OptIn)]
    public struct OsuDifficultyAttributes : IDifficultyAttributes
    {
        /// <inheritdoc/>
        public double StarRating { get; set; }

        /// <inheritdoc/>
        public int MaxCombo { get; set; }

        /// <summary>
        /// The difficulty corresponding to the aim skill.
        /// </summary>
        [JsonProperty("aim_difficulty")]
        public double AimDifficulty { get; set; }

        /// <summary>
        /// The difficulty corresponding to the speed skill.
        /// </summary>
        [JsonProperty("speed_difficulty")]
        public double SpeedDifficulty { get; set; }

        /// <summary>
        /// The number of clickable objects weighted by difficulty.
        /// Related to <see cref="SpeedDifficulty"/>
        /// </summary>
        [JsonProperty("speed_note_count")]
        public double SpeedNoteCount { get; set; }

        /// <summary>
        /// The difficulty corresponding to the flashlight skill. A null value indicates the non-existence of <see cref="Mods.OsuModFlashlight"/>.
        /// </summary>
        [JsonProperty("flashlight_difficulty")]
        public double? FlashlightDifficulty { get; set; }

        /// <summary>
        /// Describes how much of <see cref="AimDifficulty"/> is contributed to by hitcircles or sliders.
        /// A value closer to 1.0 indicates most of <see cref="AimDifficulty"/> is contributed by hitcircles.
        /// A value closer to 0.0 indicates most of <see cref="AimDifficulty"/> is contributed by sliders.
        /// </summary>
        [JsonProperty("slider_factor")]
        public double SliderFactor { get; set; }

        [JsonProperty("aim_difficult_strain_count")]
        public double AimDifficultStrainCount { get; set; }

        [JsonProperty("speed_difficult_strain_count")]
        public double SpeedDifficultStrainCount { get; set; }

        /// <summary>
        /// The perceived approach rate inclusive of rate-adjusting mods (DT/HT/etc).
        /// </summary>
        /// <remarks>
        /// Rate-adjusting mods don't directly affect the approach rate difficulty value, but have a perceived effect as a result of adjusting audio timing.
        /// </remarks>
        [JsonProperty("approach_rate")]
        public double ApproachRate { get; set; }

        /// <summary>
        /// The perceived overall difficulty inclusive of rate-adjusting mods (DT/HT/etc).
        /// </summary>
        /// <remarks>
        /// Rate-adjusting mods don't directly affect the overall difficulty value, but have a perceived effect as a result of adjusting audio timing.
        /// </remarks>
        [JsonProperty("overall_difficulty")]
        public double OverallDifficulty { get; set; }

        /// <summary>
        /// The beatmap's drain rate. This doesn't scale with rate-adjusting mods.
        /// </summary>
        public double DrainRate { get; set; }

        /// <summary>
        /// The number of hitcircles in the beatmap.
        /// </summary>
        public int HitCircleCount { get; set; }

        /// <summary>
        /// The number of sliders in the beatmap.
        /// </summary>
        public int SliderCount { get; set; }

        /// <summary>
        /// The number of spinners in the beatmap.
        /// </summary>
        public int SpinnerCount { get; set; }

        public IEnumerable<(int attributeId, object value)> ToDatabaseAttributes()
        {
            yield return (IDifficultyAttributes.ATTRIB_ID_MAX_COMBO, MaxCombo);
            yield return (IDifficultyAttributes.ATTRIB_ID_AIM, AimDifficulty);
            yield return (IDifficultyAttributes.ATTRIB_ID_SPEED, SpeedDifficulty);
            yield return (IDifficultyAttributes.ATTRIB_ID_OVERALL_DIFFICULTY, OverallDifficulty);
            yield return (IDifficultyAttributes.ATTRIB_ID_APPROACH_RATE, ApproachRate);
            yield return (IDifficultyAttributes.ATTRIB_ID_DIFFICULTY, StarRating);

            if (ShouldSerializeFlashlightDifficulty())
                yield return (IDifficultyAttributes.ATTRIB_ID_FLASHLIGHT, FlashlightDifficulty!);

            yield return (IDifficultyAttributes.ATTRIB_ID_SLIDER_FACTOR, SliderFactor);

            yield return (IDifficultyAttributes.ATTRIB_ID_AIM_DIFFICULT_STRAIN_COUNT, AimDifficultStrainCount);
            yield return (IDifficultyAttributes.ATTRIB_ID_SPEED_DIFFICULT_STRAIN_COUNT, SpeedDifficultStrainCount);
            yield return (IDifficultyAttributes.ATTRIB_ID_SPEED_NOTE_COUNT, SpeedNoteCount);
        }

        public void FromDatabaseAttributes(IReadOnlyDictionary<int, double> values, IBeatmapOnlineInfo onlineInfo)
        {
            MaxCombo = (int)values[IDifficultyAttributes.ATTRIB_ID_MAX_COMBO];
            AimDifficulty = values[IDifficultyAttributes.ATTRIB_ID_AIM];
            SpeedDifficulty = values[IDifficultyAttributes.ATTRIB_ID_SPEED];
            OverallDifficulty = values[IDifficultyAttributes.ATTRIB_ID_OVERALL_DIFFICULTY];
            ApproachRate = values[IDifficultyAttributes.ATTRIB_ID_APPROACH_RATE];
            StarRating = values[IDifficultyAttributes.ATTRIB_ID_DIFFICULTY];
            FlashlightDifficulty = values.GetValueOrDefault(IDifficultyAttributes.ATTRIB_ID_FLASHLIGHT);
            SliderFactor = values[IDifficultyAttributes.ATTRIB_ID_SLIDER_FACTOR];
            AimDifficultStrainCount = values[IDifficultyAttributes.ATTRIB_ID_AIM_DIFFICULT_STRAIN_COUNT];
            SpeedDifficultStrainCount = values[IDifficultyAttributes.ATTRIB_ID_SPEED_DIFFICULT_STRAIN_COUNT];
            SpeedNoteCount = values[IDifficultyAttributes.ATTRIB_ID_SPEED_NOTE_COUNT];
            DrainRate = onlineInfo.DrainRate;
            HitCircleCount = onlineInfo.CircleCount;
            SliderCount = onlineInfo.SliderCount;
            SpinnerCount = onlineInfo.SpinnerCount;
        }

        #region Newtonsoft.Json implicit ShouldSerialize() methods

        // The properties in this region are used implicitly by Newtonsoft.Json to not serialise certain fields in some cases.
        // They rely on being named exactly the same as the corresponding fields (casing included) and as such should NOT be renamed
        // unless the fields are also renamed.

        [UsedImplicitly]
        public bool ShouldSerializeFlashlightDifficulty() => FlashlightDifficulty is not null;

        #endregion
    }
}
