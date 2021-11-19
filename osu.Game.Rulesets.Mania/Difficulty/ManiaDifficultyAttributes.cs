// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Mania.Difficulty
{
    public class ManiaDifficultyAttributes : DifficultyAttributes
    {
        [JsonProperty("great_hit_window")]
        public double GreatHitWindow { get; set; }

        [JsonProperty("score_multiplier")]
        public double ScoreMultiplier { get; set; }

        public override IEnumerable<(int attributeId, object value)> ToDatabaseAttributes()
        {
            foreach (var v in base.ToDatabaseAttributes())
                yield return v;

            // Todo: osu!mania doesn't output MaxCombo attribute for some reason.
            yield return (ATTRIB_ID_STRAIN, StarRating);
            yield return (ATTRIB_ID_GREAT_HIT_WINDOW, GreatHitWindow);
            yield return (ATTRIB_ID_SCORE_MULTIPLIER, ScoreMultiplier);
        }

        public override void FromDatabaseAttributes(IReadOnlyDictionary<int, double> values)
        {
            base.FromDatabaseAttributes(values);

            StarRating = values[ATTRIB_ID_STRAIN];
            GreatHitWindow = values[ATTRIB_ID_GREAT_HIT_WINDOW];
            ScoreMultiplier = values[ATTRIB_ID_SCORE_MULTIPLIER];
        }
    }
}
