// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    public class TaikoDifficultyAttributes : DifficultyAttributes
    {
        [JsonProperty("stamina_strain")]
        public double StaminaStrain { get; set; }

        [JsonProperty("rhythm_strain")]
        public double RhythmStrain { get; set; }

        [JsonProperty("colour_strain")]
        public double ColourStrain { get; set; }

        [JsonProperty("approach_rate")]
        public double ApproachRate { get; set; }

        [JsonProperty("great_hit_window")]
        public double GreatHitWindow { get; set; }

        public override IEnumerable<(int attributeId, object value)> ToDatabaseAttributes()
        {
            foreach (var v in base.ToDatabaseAttributes())
                yield return v;

            yield return (ATTRIB_ID_MAX_COMBO, MaxCombo);
            yield return (ATTRIB_ID_STRAIN, StarRating);
            yield return (ATTRIB_ID_GREAT_HIT_WINDOW, GreatHitWindow);
        }

        public override void FromDatabaseAttributes(IReadOnlyDictionary<int, double> values)
        {
            base.FromDatabaseAttributes(values);

            MaxCombo = (int)values[ATTRIB_ID_MAX_COMBO];
            StarRating = values[ATTRIB_ID_STRAIN];
            GreatHitWindow = values[ATTRIB_ID_GREAT_HIT_WINDOW];
        }
    }
}
