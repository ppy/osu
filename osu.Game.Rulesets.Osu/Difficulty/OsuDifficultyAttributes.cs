// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyAttributes : DifficultyAttributes
    {
        /// <summary>
        /// The difficulty corresponding to the aim skill.
        /// </summary>
        [JsonProperty("aim_difficulty")]
        public double AimDifficulty { get; set; }

        /// <summary>
        /// The number of <see cref="Slider"/>s weighted by difficulty.
        /// </summary>
        [JsonProperty("aim_difficult_slider_count")]
        public double AimDifficultSliderCount { get; set; }

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
        /// The difficulty corresponding to the flashlight skill.
        /// </summary>
        [JsonProperty("flashlight_difficulty")]
        public double FlashlightDifficulty { get; set; }

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
        [JsonProperty("approach_rate")]
        public double ApproachRate { get; set; }

        /// <summary>
        /// The perceived overall difficulty inclusive of rate-adjusting mods (DT/HT/etc).
        /// </summary>
        [JsonProperty("overall_difficulty")]
        public double OverallDifficulty { get; set; }

        /// <summary>
        /// The perceived hit window for a GREAT hit inclusive of rate-adjusting mods (DT/HT/etc).
        /// </summary>
        [JsonProperty("great_hit_window")]
        public double GreatHitWindow { get; set; }

        /// <summary>
        /// The perceived hit window for an OK hit inclusive of rate-adjusting mods (DT/HT/etc).
        /// </summary>
        [JsonProperty("ok_hit_window")]
        public double OkHitWindow { get; set; }

        /// <summary>
        /// The perceived hit window for a MEH hit inclusive of rate-adjusting mods (DT/HT/etc).
        /// </summary>
        [JsonProperty("meh_hit_window")]
        public double MehHitWindow { get; set; }

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

        public override IEnumerable<(int attributeId, object value)> ToDatabaseAttributes()
        {
            foreach (var v in base.ToDatabaseAttributes())
                yield return v;

            yield return (ATTRIB_ID_AIM, AimDifficulty);
            yield return (ATTRIB_ID_SPEED, SpeedDifficulty);
            yield return (ATTRIB_ID_OVERALL_DIFFICULTY, OverallDifficulty);
            yield return (ATTRIB_ID_APPROACH_RATE, ApproachRate);
            yield return (ATTRIB_ID_DIFFICULTY, StarRating);
            yield return (ATTRIB_ID_GREAT_HIT_WINDOW, GreatHitWindow);

            if (ShouldSerializeFlashlightDifficulty())
                yield return (ATTRIB_ID_FLASHLIGHT, FlashlightDifficulty);

            yield return (ATTRIB_ID_SLIDER_FACTOR, SliderFactor);

            yield return (ATTRIB_ID_AIM_DIFFICULT_STRAIN_COUNT, AimDifficultStrainCount);
            yield return (ATTRIB_ID_SPEED_DIFFICULT_STRAIN_COUNT, SpeedDifficultStrainCount);
            yield return (ATTRIB_ID_SPEED_NOTE_COUNT, SpeedNoteCount);
            yield return (ATTRIB_ID_AIM_DIFFICULT_SLIDER_COUNT, AimDifficultSliderCount);

            yield return (ATTRIB_ID_OK_HIT_WINDOW, OkHitWindow);
            yield return (ATTRIB_ID_MEH_HIT_WINDOW, MehHitWindow);
        }

        public override void FromDatabaseAttributes(IReadOnlyDictionary<int, double> values, IBeatmapOnlineInfo onlineInfo)
        {
            base.FromDatabaseAttributes(values, onlineInfo);

            AimDifficulty = values[ATTRIB_ID_AIM];
            SpeedDifficulty = values[ATTRIB_ID_SPEED];
            OverallDifficulty = values[ATTRIB_ID_OVERALL_DIFFICULTY];
            ApproachRate = values[ATTRIB_ID_APPROACH_RATE];
            StarRating = values[ATTRIB_ID_DIFFICULTY];
            GreatHitWindow = values[ATTRIB_ID_GREAT_HIT_WINDOW];
            FlashlightDifficulty = values.GetValueOrDefault(ATTRIB_ID_FLASHLIGHT);
            SliderFactor = values[ATTRIB_ID_SLIDER_FACTOR];
            AimDifficultStrainCount = values[ATTRIB_ID_AIM_DIFFICULT_STRAIN_COUNT];
            SpeedDifficultStrainCount = values[ATTRIB_ID_SPEED_DIFFICULT_STRAIN_COUNT];
            SpeedNoteCount = values[ATTRIB_ID_SPEED_NOTE_COUNT];
            AimDifficultSliderCount = values[ATTRIB_ID_AIM_DIFFICULT_SLIDER_COUNT];
            OkHitWindow = values[ATTRIB_ID_OK_HIT_WINDOW];
            MehHitWindow = values[ATTRIB_ID_MEH_HIT_WINDOW];
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
        public bool ShouldSerializeFlashlightDifficulty() => Mods.Any(m => m is ModFlashlight);

        #endregion
    }
}
