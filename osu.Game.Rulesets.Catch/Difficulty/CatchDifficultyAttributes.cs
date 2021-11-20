// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Catch.Difficulty
{
    public class CatchDifficultyAttributes : DifficultyAttributes
    {
        [JsonProperty("approach_rate")]
        public double ApproachRate { get; set; }

        public override IEnumerable<(int attributeId, object value)> ToDatabaseAttributes()
        {
            foreach (var v in base.ToDatabaseAttributes())
                yield return v;

            // Todo: osu!catch should not output star rating in the 'aim' attribute.
            yield return (ATTRIB_ID_AIM, StarRating);
            yield return (ATTRIB_ID_APPROACH_RATE, ApproachRate);
            yield return (ATTRIB_ID_MAX_COMBO, MaxCombo);
        }

        public override void FromDatabaseAttributes(IReadOnlyDictionary<int, double> values)
        {
            base.FromDatabaseAttributes(values);

            StarRating = values[ATTRIB_ID_AIM];
            ApproachRate = values[ATTRIB_ID_APPROACH_RATE];
            MaxCombo = (int)values[ATTRIB_ID_MAX_COMBO];
        }
    }
}
