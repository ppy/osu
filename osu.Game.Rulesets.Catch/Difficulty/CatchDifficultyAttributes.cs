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

        public override IEnumerable<(int attributeId, object value)> ToDatabase()
        {
            foreach (var v in base.ToDatabase())
                yield return v;

            // Todo: Catch should not output star rating in the 'aim' attribute.
            yield return (1, StarRating);
            yield return (7, ApproachRate);
            yield return (9, MaxCombo);
        }

        public override void FromDatabase(IReadOnlyDictionary<int, double> values, int hitCircleCount, int spinnerCount)
        {
            base.FromDatabase(values, hitCircleCount, spinnerCount);

            StarRating = values[1];
            ApproachRate = values[7];
            MaxCombo = (int)values[9];
        }
    }
}
