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

        public override IEnumerable<(int attributeId, object value)> ToDatabase()
        {
            foreach (var v in base.ToDatabase())
                yield return v;

            yield return (9, MaxCombo);
            yield return (11, StarRating);
            yield return (13, GreatHitWindow);
        }

        public override void FromDatabase(IReadOnlyDictionary<int, double> values, int hitCircleCount, int spinnerCount)
        {
            base.FromDatabase(values, hitCircleCount, spinnerCount);

            MaxCombo = (int)values[9];
            StarRating = values[11];
            GreatHitWindow = values[13];
        }
    }
}
