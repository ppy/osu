// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Catch.Difficulty
{
    [JsonObject(MemberSerialization.OptIn)]
    public struct CatchDifficultyAttributes : IDifficultyAttributes
    {
        /// <inheritdoc/>
        public double StarRating { get; set; }

        /// <inheritdoc/>
        public int MaxCombo { get; set; }

        /// <summary>
        /// The perceived approach rate inclusive of rate-adjusting mods (DT/HT/etc).
        /// </summary>
        /// <remarks>
        /// Rate-adjusting mods don't directly affect the approach rate difficulty value, but have a perceived effect as a result of adjusting audio timing.
        /// </remarks>
        [JsonProperty("approach_rate")]
        public double ApproachRate { get; set; }

        public IEnumerable<(int attributeId, object value)> ToDatabaseAttributes()
        {
            yield return (IDifficultyAttributes.ATTRIB_ID_MAX_COMBO, MaxCombo);
            // Todo: osu!catch should not output star rating in the 'aim' attribute.
            yield return (IDifficultyAttributes.ATTRIB_ID_AIM, StarRating);
            yield return (IDifficultyAttributes.ATTRIB_ID_APPROACH_RATE, ApproachRate);
        }

        public void FromDatabaseAttributes(IReadOnlyDictionary<int, double> values, IBeatmapOnlineInfo onlineInfo)
        {
            MaxCombo = (int)values[IDifficultyAttributes.ATTRIB_ID_MAX_COMBO];
            StarRating = values[IDifficultyAttributes.ATTRIB_ID_AIM];
            ApproachRate = values[IDifficultyAttributes.ATTRIB_ID_APPROACH_RATE];
        }
    }
}
